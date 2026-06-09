using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

/// <summary>
/// Główny DbContext aplikacji
/// Definiuje wszystkie DbSets (tabele) w bazie danych
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Konfiguracja modeli i relacji między encjami
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.AvatarUrl)
                .HasMaxLength(1024);

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .IsRequired();
        });
    }
}
