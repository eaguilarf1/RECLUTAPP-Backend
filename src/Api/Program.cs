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
app.MapGet("/vacancies", async (IVacancyRepository repo, CancellationToken ct)
    => Results.Ok(await repo.GetAllAsync(ct)));

app.MapGet("/vacancies/{id:guid}", async (Guid id, IVacancyRepository repo, CancellationToken ct)
    => (await repo.GetAsync(id, ct)) is { } v ? Results.Ok(v) : Results.NotFound());

app.MapPost("/vacancies", async (Vacancy v, IVacancyRepository repo, CancellationToken ct) =>
{
    var created = await repo.AddAsync(v, ct);
    return Results.Created($"/vacancies/{created.Id}", created);
});

app.MapPut("/vacancies/{id:guid}", async (Guid id, Vacancy input, IVacancyRepository repo, CancellationToken ct) =>
{
    var current = await repo.GetAsync(id, ct);
    if (current is null) return Results.NotFound();
    current.Title = input.Title;
    current.Recruiter = input.Recruiter;
    current.PublishedOn = input.PublishedOn;
    await repo.UpdateAsync(current, ct);
    return Results.NoContent();
});

app.MapDelete("/vacancies/{id:guid}", async (Guid id, IVacancyRepository repo, CancellationToken ct) =>
{
    await repo.DeleteAsync(id, ct);
    return Results.NoContent();
});

// ========== Users (Admin) ==========
app.MapGet("/users", async (
    int page, int pageSize, string? search, Role? role,
    IUserRepository repo, CancellationToken ct) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 || pageSize > 100 ? 10 : pageSize;

    var (items, total) = await repo.GetPagedAsync(page, pageSize, search, role, ct);
    var result = new UsersPagedResponse(
        page, pageSize, total, items.Select(x => x.ToItem()).ToList());
    return Results.Ok(result);
});

app.MapGet("/users/{id:guid}", async (Guid id, IUserRepository repo, CancellationToken ct) =>
{
    var u = await repo.GetAsync(id, ct);
    return u is null ? Results.NotFound() : Results.Ok(u.ToItem());
});

app.MapPost("/users", async (UserCreateDto dto, IUserRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
        return Results.BadRequest("Name and Email are required.");

    var user = new User { Name = dto.Name.Trim(), Email = dto.Email.Trim(), Role = dto.Role };
    var created = await repo.AddAsync(user, ct);
    return Results.Created($"/users/{created.Id}", created.ToItem());
});

app.MapPut("/users/{id:guid}", async (Guid id, UserUpdateDto dto, IUserRepository repo, CancellationToken ct) =>
{
    var current = await repo.GetAsync(id, ct);
    if (current is null) return Results.NotFound();

    current.Name = dto.Name.Trim();
    current.Email = dto.Email.Trim();
    current.Role = dto.Role;

    await repo.UpdateAsync(current, ct);
    return Results.NoContent();
});

app.MapDelete("/users/{id:guid}", async (Guid id, IUserRepository repo, CancellationToken ct) =>
{
    await repo.DeleteAsync(id, ct);
    return Results.NoContent();
});

app.Run();
