using Domain.Users;

namespace Api;

public sealed record UserCreateDto(string Name, string Email, Role Role);
public sealed record UserUpdateDto(string Name, string Email, Role Role);

public sealed record UsersPagedResponse(
    int page, int pageSize, int total, IReadOnlyList<UserItem> items);

public sealed record UserItem(
    Guid Id, string Name, string Email, Role Role, DateTime CreatedAt);

public static class UserMappings
{
    public static UserItem ToItem(this User u)
        => new(u.Id, u.Name, u.Email, u.Role, u.CreatedAt);
}
