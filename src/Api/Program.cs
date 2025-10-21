using Infrastructure;
using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Api;
using Domain.Users;

// ...

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registro de DbContext + repos vía extensión de Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

const string SpaCorsPolicy = "SpaCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: SpaCorsPolicy, policy =>
        policy
            .WithOrigins("http://localhost:4200") // URL del frontend Angular
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

app.UseCors(SpaCorsPolicy);
app.UseSwagger();
app.UseSwaggerUI();

// ========== Vacancies ==========

app.MapGet("/vacancies", async (
    int page, int pageSize, string? search, VacancyStatus? status,
    IVacancyRepository repo, CancellationToken ct) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 || pageSize > 100 ? 12 : pageSize;

    var (items, total) = await repo.GetPagedAsync(page, pageSize, search, status, ct);
    var result = new VacanciesPagedResponse(page, pageSize, total, items.Select(x => x.ToItem()).ToList());
    return Results.Ok(result);
});

app.MapGet("/vacancies/{id:guid}", async (Guid id, IVacancyRepository repo, CancellationToken ct) =>
{
    var v = await repo.GetAsync(id, ct);
    return v is null ? Results.NotFound() : Results.Ok(v.ToItem());
});

app.MapPost("/vacancies", async (VacancyCreateDto dto, IVacancyRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Recruiter))
        return Results.BadRequest("Title and Recruiter are required.");

    var v = new Vacancy
    {
        Title = dto.Title.Trim(),
        Recruiter = dto.Recruiter.Trim(),
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
        Status = dto.Status,
        PublishedOn = DateTime.UtcNow
    };

    var created = await repo.AddAsync(v, ct);
    return Results.Created($"/vacancies/{created.Id}", created.ToItem());
});

app.MapPut("/vacancies/{id:guid}", async (Guid id, VacancyUpdateDto dto, IVacancyRepository repo, CancellationToken ct) =>
{
    var current = await repo.GetAsync(id, ct);
    if (current is null) return Results.NotFound();

    current.Title = dto.Title.Trim();
    current.Recruiter = dto.Recruiter.Trim();
    current.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
    current.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();
    current.Status = dto.Status;

    await repo.UpdateAsync(current, ct);
    return Results.NoContent();
});

app.MapDelete("/vacancies/{id:guid}", async (Guid id, IVacancyRepository repo, CancellationToken ct) =>

{
    await repo.DeleteAsync(id, ct);
    return Results.NoContent();
});

app.Run();
