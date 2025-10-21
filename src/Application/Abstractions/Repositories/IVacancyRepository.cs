using Domain.Vacancies;

namespace Application.Abstractions.Repositories;

public interface IVacancyRepository
{
    Task<(IReadOnlyList<Vacancy> items, int total)> GetPagedAsync(
        int page, int pageSize, string? search, VacancyStatus? status, CancellationToken ct = default);

    Task<Vacancy?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Vacancy>  AddAsync(Vacancy v, CancellationToken ct = default);
    Task           UpdateAsync(Vacancy v, CancellationToken ct = default);
    Task           DeleteAsync(Guid id, CancellationToken ct = default);
}
