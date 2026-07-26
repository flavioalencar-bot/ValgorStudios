using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Valgor.Domain.Common;
using Valgor.Domain.Heroes;
using Valgor.Domain.Users;

namespace Valgor.Infrastructure.Persistence;

public sealed class ValgorDbContext(DbContextOptions<ValgorDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<HeroDefinition> HeroDefinitions => Set<HeroDefinition>();
    public DbSet<HeroSpecialPower> HeroSpecialPowers => Set<HeroSpecialPower>();
    public DbSet<HeroSpecialEffect> HeroSpecialEffects => Set<HeroSpecialEffect>();
    public DbSet<HeroSkin> HeroSkins => Set<HeroSkin>();
    public DbSet<HeroFaction> HeroFactions => Set<HeroFaction>();
    public DbSet<FactionAdvantage> FactionAdvantages => Set<FactionAdvantage>();
    public DbSet<FactionTeamBonus> FactionTeamBonuses => Set<FactionTeamBonus>();
    public DbSet<PlayerHero> PlayerHeroes => Set<PlayerHero>();
    public DbSet<BattleHeroSpecialState> BattleHeroSpecialStates => Set<BattleHeroSpecialState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ValgorDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkUpdated(DateTime.UtcNow);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();

        builder.Property(user => user.UpdatedAtUtc);

        builder.Ignore(user => user.DomainEvents);
    }
}
