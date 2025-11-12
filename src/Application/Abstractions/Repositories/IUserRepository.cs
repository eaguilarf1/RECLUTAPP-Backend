using Domain.Users;

namespace Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<(IReadOnlyList<User> items, int total)> GetPagedAsync(
        int page, int pageSize, string? search, Role? role, CancellationToken ct = default);

    Task<User?> GetAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default); // ← NUEVO
    Task<User>  AddAsync(User user, CancellationToken ct = default);
    Task        UpdateAsync(User user, CancellationToken ct = default);
    Task        DeleteAsync(Guid id, CancellationToken ct = default);
}
