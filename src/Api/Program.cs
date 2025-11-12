using Infrastructure;
using Application.Abstractions.Repositories;
using Domain.Vacancies;
using Domain.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

const string CorsPolicy = "frontend";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "p3dR6u2aR!tY7nQv9ZcX1mLk0bV5wS8yT4rU2iO6eQ1aM3nP";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "reclutapp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "reclutapp-web";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/register", async (
    [FromBody] RegisterDto dto,
    [FromServices] IUserRepository usersRepo,
    [FromServices] IJwtTokenService jwt,
    CancellationToken ct
) =>
{
    var email = dto.Email.Trim().ToLowerInvariant();
    var existing = await usersRepo.GetByEmailAsync(email, ct);
    if (existing is not null)
        return Results.Conflict(new { message = "El correo ya está registrado." });

    var user = new User
    {
        Name = dto.Name.Trim(),
        Email = email,
        Role = Role.Candidate,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim()),
        IsActive = true
    };

    await usersRepo.AddAsync(user, ct);

    var token = jwt.Create(user);
    return Results.Ok(new AuthResponse(token, user.ToAuthUser()));
});

app.MapPost("/auth/login", async (
    [FromBody] LoginDto dto,
    [FromServices] IUserRepository usersRepo,
    [FromServices] IJwtTokenService jwt,
    CancellationToken ct
) =>
{
    var email = dto.Email.Trim().ToLowerInvariant();
    var user = await usersRepo.GetByEmailAsync(email, ct);
    if (user is null || !user.IsActive)
        return Results.Unauthorized();

    var ok = BCrypt.Net.BCrypt.Verify(dto.Password.Trim(), user.PasswordHash);
    if (!ok)
        return Results.Unauthorized();

    var token = jwt.Create(user);
    return Results.Ok(new AuthResponse(token, user.ToAuthUser()));
});

app.MapGet("/users", async (
    [FromServices] IUserRepository repo,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] string? search = null,
    [FromQuery] Role? role = null,
    CancellationToken ct = default
) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 || pageSize > 100 ? 12 : pageSize;

    var (items, total) = await repo.GetPagedAsync(page, pageSize, search, role, ct);
    var mapped = items.Select(u => u.ToAuthUser()).ToList();

    return Results.Ok(new { page, pageSize, total, items = mapped });
})
.RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));

app.MapPost("/users", async (
    [FromBody] AdminCreateUserDto dto,
    [FromServices] IUserRepository usersRepo,
    CancellationToken ct
) =>
{
    var email = dto.Email.Trim().ToLowerInvariant();
    var existing = await usersRepo.GetByEmailAsync(email, ct);
    if (existing is not null)
        return Results.Conflict(new { message = "El correo ya está registrado." });

    Role parsedRole;
    var raw = (dto.Role ?? "").Trim();
    if (int.TryParse(raw, out var n) && Enum.IsDefined(typeof(Role), n))
        parsedRole = (Role)n;
    else
    {
        var s = raw.ToUpperInvariant();
        if (s.StartsWith("ADMIN")) parsedRole = Role.Admin;
        else if (s.StartsWith("RECRU")) parsedRole = Role.Recruiter;
        else parsedRole = Role.Candidate;
    }

    var user = new User
    {
        Name = dto.Name.Trim(),
        Email = email,
        Role = parsedRole,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim()),
        IsActive = true
    };

    await usersRepo.AddAsync(user, ct);
    return Results.Created($"/users/{user.Id}", new { user.Id });
})
.RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));

app.MapPut("/users/{id:guid}", async (
    Guid id,
    [FromBody] AdminUpdateUserDto dto,
    [FromServices] IUserRepository usersRepo,
    CancellationToken ct
) =>
{
    var user = await usersRepo.GetAsync(id, ct);
    if (user is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(dto.Email))
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var existing = await usersRepo.GetByEmailAsync(email, ct);
        if (existing is not null && existing.Id != id)
            return Results.Conflict(new { message = "El correo ya está registrado." });
        user.Email = email;
    }

    if (!string.IsNullOrWhiteSpace(dto.Name))
        user.Name = dto.Name.Trim();

    if (!string.IsNullOrWhiteSpace(dto.Role))
    {
        var raw = dto.Role.Trim();
        Role parsed;
        if (int.TryParse(raw, out var n) && Enum.IsDefined(typeof(Role), n))
            parsed = (Role)n;
        else
        {
            var s = raw.ToUpperInvariant();
            if (s.StartsWith("ADMIN")) parsed = Role.Admin;
            else if (s.StartsWith("RECRU")) parsed = Role.Recruiter;
            else parsed = Role.Candidate;
        }
        user.Role = parsed;
    }

    if (dto.IsActive.HasValue)
        user.IsActive = dto.IsActive.Value;

    await usersRepo.UpdateAsync(user, ct);
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));

app.MapDelete("/users/{id:guid}", async (
    Guid id,
    [FromServices] IUserRepository usersRepo,
    CancellationToken ct
) =>
{
    await usersRepo.DeleteAsync(id, ct);
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));

app.MapGet("/vacancies", async (
    [FromServices] IVacancyRepository repo,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] string? search = null,
    [FromQuery] VacancyStatus? status = null,
    CancellationToken ct = default
) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 || pageSize > 100 ? 12 : pageSize;

    var (items, total) = await repo.GetPagedAsync(page, pageSize, search, status, ct);

    var result = new VacanciesPagedResponse(
        Page: page,
        PageSize: pageSize,
        Total: total,
        Items: items.Select(x => x.ToItem()).ToList()
    );

    return Results.Ok(result);
});

app.MapGet("/vacancies/{id:guid}", async (
    Guid id,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    var v = await repo.GetAsync(id, ct);
    return v is null ? Results.NotFound() : Results.Ok(v.ToItem());
});

app.MapPost("/vacancies", async (
    [FromBody] VacancyCreateDto dto,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Recruiter))
        return Results.BadRequest("Title and Recruiter are required.");

    var defaultStatus = Enum.GetValues<VacancyStatus>()[0];

    var v = new Vacancy
    {
        Title = dto.Title.Trim(),
        Recruiter = dto.Recruiter.Trim(),
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
        Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
        Status = dto.Status ?? defaultStatus,
        PublishedOn = dto.PublishedOn?.ToUniversalTime() ?? DateTime.UtcNow
    };

    var created = await repo.AddAsync(v, ct);
    return Results.Created($"/vacancies/{created.Id}", created.ToItem());
});

app.MapPut("/vacancies/{id:guid}", async (
    Guid id,
    [FromBody] VacancyUpdateDto dto,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    var current = await repo.GetAsync(id, ct);
    if (current is null) return Results.NotFound();

    current.Title = dto.Title.Trim();
    current.Recruiter = dto.Recruiter.Trim();
    current.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
    current.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();
    if (dto.Status.HasValue) current.Status = dto.Status.Value;
    if (dto.PublishedOn.HasValue)
        current.PublishedOn = dto.PublishedOn.Value.ToUniversalTime();

    await repo.UpdateAsync(current, ct);
    return Results.NoContent();
});

app.MapDelete("/vacancies/{id:guid}", async (
    Guid id,
    [FromServices] IVacancyRepository repo,
    CancellationToken ct
) =>
{
    await repo.DeleteAsync(id, ct);
    return Results.NoContent();
});

app.MapGet("/admin/stats", async (
    [FromServices] IUserRepository usersRepo,
    [FromServices] IVacancyRepository vacRepo,
    CancellationToken ct
) =>
{
    var (_, totalUsers) = await usersRepo.GetPagedAsync(1, 1, null, null, ct);
    var activeStatus = Enum.GetValues<VacancyStatus>()[0];
    var (_, activeVacancies) = await vacRepo.GetPagedAsync(1, 1, null, activeStatus, ct);
    var (lastUsers, _) = await usersRepo.GetPagedAsync(1, 3, null, null, ct);

    return Results.Ok(new
    {
        totalUsers,
        activeVacancies,
        lastUsers = lastUsers.Select(u => u.ToAuthUser()).ToList()
    });
})
.RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var admin = await repo.GetByEmailAsync("admin@reclutapp.local");
    if (admin is null)
    {
        var seeded = new User
        {
            Name = "Admin",
            Email = "admin@reclutapp.local",
            Role = Role.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            IsActive = true
        };
        await repo.AddAsync(seeded);
    }
}

app.Run();

public record VacancyItem(
    Guid Id,
    string Title,
    string Recruiter,
    DateTime PublishedOn,
    string? Description,
    string? Location,
    VacancyStatus Status
);

public record VacanciesPagedResponse(
    int Page,
    int PageSize,
    int Total,
    List<VacancyItem> Items
);

public record VacancyCreateDto
{
    public string Title { get; init; } = default!;
    public string Recruiter { get; init; } = default!;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public VacancyStatus? Status { get; init; }
    public DateTime? PublishedOn { get; init; }
}

public record VacancyUpdateDto
{
    public string Title { get; init; } = default!;
    public string Recruiter { get; init; } = default!;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public VacancyStatus? Status { get; init; }
    public DateTime? PublishedOn { get; init; }
}

public static class VacancyMapExtensions
{
    public static VacancyItem ToItem(this Vacancy v) =>
        new(
            v.Id,
            v.Title,
            v.Recruiter,
            v.PublishedOn,
            v.Description,
            v.Location,
            v.Status
        );
}

public sealed record LoginDto(string Email, string Password);
public sealed record RegisterDto(string Name, string Email, string Password);
public sealed record AdminCreateUserDto(string Name, string Email, string Password, string Role);
public sealed record AdminUpdateUserDto(string? Name, string? Email, string? Role, bool? IsActive);

public sealed record AuthUser(Guid Id, string Name, string Email, Role Role, DateTime CreatedAt);
public sealed record AuthResponse(string Token, AuthUser User);

public static class AuthMappings
{
    public static AuthUser ToAuthUser(this User u) =>
        new(u.Id, u.Name, u.Email, u.Role, u.CreatedAt);
}
