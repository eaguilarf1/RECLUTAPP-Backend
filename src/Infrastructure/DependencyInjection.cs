using Application.Abstractions.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Security;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("SqlServer")
                 ?? throw new InvalidOperationException("Missing 'SqlServer' connection string.");

        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IVacancyRepository, VacancyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        var googleSettings = new GoogleAuthSettings
        {
            ClientId = config["GoogleAuth:ClientId"] ?? string.Empty
        };

        services.AddSingleton(googleSettings);
        services.AddScoped<GoogleTokenValidator>();

        return services;
    }
}
