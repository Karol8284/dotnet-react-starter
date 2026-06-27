using Domain.Entities;
using Domain.Entities.JWT;
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

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Konfiguracja modeli i relacji między encjami
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.AvatarUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
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
