namespace Domain.Vacancies;

public sealed class Vacancy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public string Recruiter { get; set; } = default!;
    public DateTime PublishedOn { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }
    public string? Location { get; set; }
    public VacancyStatus Status { get; set; } = VacancyStatus.Active;
}

