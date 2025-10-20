using Application.Abstractions.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(opt =>
        {
            if (string.IsNullOrWhiteSpace(cs))
                opt.UseInMemoryDatabase("reclutapp");
            else
                opt.UseSqlServer(cs);
        });

        services.AddScoped<IVacancyRepository, VacancyRepository>();
        return services;
    }
}
