namespace Domain.Users;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public Role Role { get; set; } = Role.Candidate;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
