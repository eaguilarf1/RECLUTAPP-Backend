using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Reclutapp API");

app.MapGet("/vacancies", async (IVacancyRepository repo, CancellationToken ct) =>
    Results.Ok(await repo.ListAsync(ct)));

app.MapGet("/vacancies/{id:guid}", async (Guid id, IVacancyRepository repo, CancellationToken ct) =>
{
    var v = await repo.GetAsync(id, ct);
    return v is null ? Results.NotFound() : Results.Ok(v);
});

app.MapPost("/vacancies", async (Vacancy v, IVacancyRepository repo, CancellationToken ct) =>
{
    await repo.AddAsync(v, ct);
    return Results.Created($"/vacancies/{v.Id}", v);
});

app.MapPut("/vacancies/{id:guid}", async (Guid id, Vacancy v, IVacancyRepository repo, CancellationToken ct) =>
{
    v.Id = id;
    await repo.UpdateAsync(v, ct);
    return Results.NoContent();
});

app.MapDelete("/vacancies/{id:guid}", async (Guid id, IVacancyRepository repo, CancellationToken ct) =>
{
    await repo.DeleteAsync(id, ct);
    return Results.NoContent();
});

app.Run();
