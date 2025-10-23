using Infrastructure;
using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext + repos 
builder.Services.AddInfrastructure(builder.Configuration);

// ---- CORS ----
const string CorsPolicy = "frontend";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(CorsPolicy);

// ========== Vacancies ==========

app.MapGet("/vacancies", async (
    [FromServices] IVacancyRepository repo,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] string? search = null,
    [FromQuery] VacancyStatus? status = null,
    CancellationToken ct = default
) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 || pageSize > 100 ? 12 : pageSize;

    var (items, total) = await repo.GetPagedAsync(page, pageSize, search, status, ct);

    var result = new VacanciesPagedResponse(
        Page: page,
        PageSize: pageSize,
        Total: total,
        Items: items.Select(x => x.ToItem()).ToList()
    );

    return Results.Ok(result);
});

app.MapGet("/vacancies/{id:guid}", async (
    Guid id,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    var v = await repo.GetAsync(id, ct);
    return v is null ? Results.NotFound() : Results.Ok(v.ToItem());
});

app.MapPost("/vacancies", async (
    [FromBody] VacancyCreateDto dto,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Recruiter))
        return Results.BadRequest("Title and Recruiter are required.");

    var defaultStatus = Enum.GetValues<VacancyStatus>()[0];

    var v = new Vacancy
    {
        Title = dto.Title.Trim(),
        Recruiter = dto.Recruiter.Trim(),
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
        Status = dto.Status ?? defaultStatus,
        PublishedOn = dto.PublishedOn?.ToUniversalTime() ?? DateTime.UtcNow
    };

    var created = await repo.AddAsync(v, ct);
    return Results.Created($"/vacancies/{created.Id}", created.ToItem());
});

app.MapPut("/vacancies/{id:guid}", async (
    Guid id,
    [FromBody] VacancyUpdateDto dto,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    var current = await repo.GetAsync(id, ct);
    if (current is null) return Results.NotFound();

    current.Title = dto.Title.Trim();
    current.Recruiter = dto.Recruiter.Trim();
    current.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
    current.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();
    if (dto.Status.HasValue) current.Status = dto.Status.Value;
    if (dto.PublishedOn.HasValue)
    current.PublishedOn = dto.PublishedOn.Value.ToUniversalTime();  


    await repo.UpdateAsync(current, ct);
    return Results.NoContent();
});

app.MapDelete("/vacancies/{id:guid}", async (
    Guid id,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    await repo.DeleteAsync(id, ct);
    return Results.NoContent();
});

app.Run();


//   DTOs

public record VacancyItem(
    Guid Id,
    string Title,
    string Recruiter,
    DateTime PublishedOn,
    string? Description,
    string? Location,
    VacancyStatus Status
);

public record VacanciesPagedResponse(
    int Page,
    int PageSize,
    int Total,
    List<VacancyItem> Items
);

public record VacancyCreateDto
{
    public string Title { get; init; } = default!;
    public string Recruiter { get; init; } = default!;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public VacancyStatus? Status { get; init; } 
    public DateTime? PublishedOn { get; init; }
}

public record VacancyUpdateDto
{
    public string Title { get; init; } = default!;
    public string Recruiter { get; init; } = default!;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public VacancyStatus? Status { get; init; } 
    public DateTime? PublishedOn { get; init; } 
}

//   Mapping helpers

public static class VacancyMapExtensions
{
    public static VacancyItem ToItem(this Vacancy v) =>
        new(
            v.Id,
            v.Title,
            v.Recruiter,
            v.PublishedOn,
            v.Description,
            v.Location,
            v.Status
        );
}
