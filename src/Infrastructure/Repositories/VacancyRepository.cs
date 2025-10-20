using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VacancyRepository(AppDbContext db) : IVacancyRepository
{
    public async Task<List<Vacancy>> GetAllAsync(CancellationToken ct = default)
        => await db.Vacancies.OrderByDescending(v => v.PublishedOn).ToListAsync(ct);

    public async Task<Vacancy?> GetAsync(Guid id, CancellationToken ct = default)
        => await db.Vacancies.FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Vacancy> AddAsync(Vacancy vacancy, CancellationToken ct = default)
    {
        db.Vacancies.Add(vacancy);
        await db.SaveChangesAsync(ct);
        return vacancy;
    }

    public async Task UpdateAsync(Vacancy vacancy, CancellationToken ct = default)
    {
        db.Vacancies.Update(vacancy);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var v = await db.Vacancies.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (v is null) return;
        db.Vacancies.Remove(v);
        await db.SaveChangesAsync(ct);
    }
}
