using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VacancyRepository(AppDbContext db) : IVacancyRepository
{
    public async Task<(IReadOnlyList<Vacancy> items, int total)> GetPagedAsync(
        int page, int pageSize, string? search, VacancyStatus? status, CancellationToken ct = default)
    {
        var q = db.Vacancies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(v => v.Title.Contains(s) || (v.Description != null && v.Description.Contains(s)));
        }
        if (status.HasValue)
        {
            q = q.Where(v => v.Status == status.Value);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(v => v.PublishedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Vacancy?> GetAsync(Guid id, CancellationToken ct = default)
        => db.Vacancies.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Vacancy> AddAsync(Vacancy v, CancellationToken ct = default)
    {
        db.Vacancies.Add(v);
        await db.SaveChangesAsync(ct);
        return v;
    }

    public async Task UpdateAsync(Vacancy v, CancellationToken ct = default)
    {
        db.Vacancies.Update(v);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Vacancies.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return;
        db.Vacancies.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
