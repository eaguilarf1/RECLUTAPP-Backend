using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VacancyRepository : IVacancyRepository
{
    private readonly AppDbContext _db;
    public VacancyRepository(AppDbContext db) => _db = db;

    public Task<List<Vacancy>> ListAsync(CancellationToken ct = default) =>
        _db.Vacancies.AsNoTracking().OrderByDescending(v => v.PublishedOn).ToListAsync(ct);

    public Task<Vacancy?> GetAsync(Guid id, CancellationToken ct = default) =>
        _db.Vacancies.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task AddAsync(Vacancy v, CancellationToken ct = default)
    {
        if (v.Id == Guid.Empty) v.Id = Guid.NewGuid();
        await _db.Vacancies.AddAsync(v, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Vacancy v, CancellationToken ct = default)
    {
        _db.Vacancies.Update(v);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Vacancies.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e is null) return;
        _db.Vacancies.Remove(e);
        await _db.SaveChangesAsync(ct);
    }
}
