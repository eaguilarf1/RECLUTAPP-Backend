using Domain.Vacancies;

namespace Application.Abstractions.Repositories;

public interface IVacancyRepository
{
    Task<List<Vacancy>> GetAllAsync(CancellationToken ct = default);
    Task<Vacancy?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Vacancy> AddAsync(Vacancy vacancy, CancellationToken ct = default);
    Task UpdateAsync(Vacancy vacancy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
