using Domain.Vacancies;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<User>    Users     => Set<User>();

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

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(u => u.Id);
            b.Property(u => u.Name).HasMaxLength(150).IsRequired();
            b.Property(u => u.Email).HasMaxLength(200).IsRequired();
            b.Property(u => u.Role).IsRequired();
            b.Property(u => u.CreatedAt).IsRequired();
            b.HasIndex(u => u.Email).IsUnique();
        });
    }
}
