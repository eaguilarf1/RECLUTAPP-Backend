namespace Domain.Vacancies;

public class Vacancy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Recruiter { get; set; } = string.Empty;
    public DateTime? PublishedOn { get; set; } = DateTime.UtcNow;
}
