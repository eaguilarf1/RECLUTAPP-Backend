using Domain.Vacancies;

namespace Api;

public sealed record VacancyCreateDto(
    string Title,
    string Recruiter,
    string? Description,
    string? Location,
    VacancyStatus Status = VacancyStatus.Active);

public sealed record VacancyUpdateDto(
    string Title,
    string Recruiter,
    string? Description,
    string? Location,
    VacancyStatus Status);

public sealed record VacancyItem(
    Guid Id,
    string Title,
    string Recruiter,
    string? Description,
    string? Location,
    VacancyStatus Status,
    DateTime PublishedOn);

public sealed record VacanciesPagedResponse(
    int page, int pageSize, int total, IReadOnlyList<VacancyItem> items);

public static class VacancyMappings
{
    public static VacancyItem ToItem(this Vacancy v) =>
        new(v.Id, v.Title, v.Recruiter, v.Description, v.Location, v.Status, v.PublishedOn);
}
