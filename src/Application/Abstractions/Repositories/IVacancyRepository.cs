using Domain.Vacancies;

namespace Application.Abstractions.Repositories;

public interface IVacancyRepository
{
    Task<List<Vacancy>> ListAsync(CancellationToken ct = default);
    Task<Vacancy?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Vacancy v, CancellationToken ct = default);
    Task UpdateAsync(Vacancy v, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
