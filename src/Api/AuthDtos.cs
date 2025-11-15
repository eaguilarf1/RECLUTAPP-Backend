using Domain.Users;

namespace Api;

public sealed record LoginDto(string Email, string Password);
public sealed record RegisterDto(string Name, string Email, string Password);
public sealed record AdminCreateUserDto(string Name, string Email, string Password, Role Role);

public sealed record AuthUser(Guid Id, string Name, string Email, Role Role, DateTime CreatedAt);
public sealed record AuthResponse(string Token, AuthUser User);

public static class AuthMappings
{
    public static AuthUser ToAuthUser(this User u) =>
        new(u.Id, u.Name, u.Email, u.Role, u.CreatedAt);
}

public sealed class GoogleLoginRequest
{
    public string IdToken { get; set; } = default!;
}
