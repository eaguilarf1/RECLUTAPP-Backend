using Infrastructure;
using Application.Abstractions.Repositories;
using Domain.Vacancies;
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

app.Run();
