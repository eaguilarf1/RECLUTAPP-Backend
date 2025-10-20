using Domain.Vacancies;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vacancy>(b =>
        {
            b.ToTable("Vacancies");
            b.HasKey(v => v.Id);
            b.Property(v => v.Title).HasMaxLength(200).IsRequired();
            b.Property(v => v.Recruiter).HasMaxLength(150).IsRequired();
            b.Property(v => v.PublishedOn).IsRequired();
            b.HasIndex(v => v.PublishedOn);
        });
    }
}
