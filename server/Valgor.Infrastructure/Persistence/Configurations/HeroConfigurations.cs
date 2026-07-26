using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Valgor.Domain.Heroes;

namespace Valgor.Infrastructure.Persistence.Configurations;

internal sealed class HeroFactionConfiguration : IEntityTypeConfiguration<HeroFaction>
{
    public void Configure(EntityTypeBuilder<HeroFaction> builder)
    {
        builder.ToTable("hero_factions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Color).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Archetype).HasMaxLength(256).IsRequired();
        builder.Property(x => x.BeatsFactionId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.LosesToFactionId).HasMaxLength(64).IsRequired();
    }
}

internal sealed class FactionAdvantageConfiguration : IEntityTypeConfiguration<FactionAdvantage>
{
    public void Configure(EntityTypeBuilder<FactionAdvantage> builder)
    {
        builder.ToTable("faction_advantages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttackerFactionId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DefenderFactionId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DamageMultiplier).HasPrecision(8, 4).IsRequired();
        builder.HasIndex(x => new { x.AttackerFactionId, x.DefenderFactionId }).IsUnique();
    }
}

internal sealed class FactionTeamBonusConfiguration : IEntityTypeConfiguration<FactionTeamBonus>
{
    public void Configure(EntityTypeBuilder<FactionTeamBonus> builder)
    {
        builder.ToTable("faction_team_bonuses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalTroopAttackMultiplier).HasPrecision(8, 4).IsRequired();
        builder.HasIndex(x => new { x.SameFactionCount, x.OtherFactionCount }).IsUnique();
    }
}

internal sealed class HeroDefinitionConfiguration : IEntityTypeConfiguration<HeroDefinition>
{
    public void Configure(EntityTypeBuilder<HeroDefinition> builder)
    {
        builder.ToTable("hero_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Gender).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Rarity).HasMaxLength(32).IsRequired();
        builder.Property(x => x.FactionId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ClassName).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Role).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Position).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Weapon).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Element).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.DefaultSkinId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PrefabAddress).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PortraitAddress).HasMaxLength(256).IsRequired();
        builder.Ignore(x => x.DisplayName);
        builder.Ignore(x => x.Effects);
        builder.Ignore(x => x.Skins);

        builder.HasOne(x => x.SpecialPower)
            .WithOne()
            .HasForeignKey<HeroSpecialPower>(x => x.HeroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FactionId);
    }
}

internal sealed class HeroSpecialPowerConfiguration : IEntityTypeConfiguration<HeroSpecialPower>
{
    public void Configure(EntityTypeBuilder<HeroSpecialPower> builder)
    {
        builder.ToTable("hero_special_powers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.HeroId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.AnimationState).HasMaxLength(64).IsRequired();
        builder.Property(x => x.VfxAddress).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SfxAddress).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.HeroId).IsUnique();
    }
}

internal sealed class HeroSpecialEffectConfiguration : IEntityTypeConfiguration<HeroSpecialEffect>
{
    public void Configure(EntityTypeBuilder<HeroSpecialEffect> builder)
    {
        builder.ToTable("hero_special_effects");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HeroId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.SpecialPowerId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => new { x.SpecialPowerId, x.SortOrder }).IsUnique();
    }
}

internal sealed class HeroSkinConfiguration : IEntityTypeConfiguration<HeroSkin>
{
    public void Configure(EntityTypeBuilder<HeroSkin> builder)
    {
        builder.ToTable("hero_skins");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.HeroId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Rarity).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ModelAddress).HasMaxLength(256).IsRequired();
        builder.Property(x => x.MaterialSetAddress).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PortraitAddress).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.HeroId);
    }
}

internal sealed class PlayerHeroConfiguration : IEntityTypeConfiguration<PlayerHero>
{
    public void Configure(EntityTypeBuilder<PlayerHero> builder)
    {
        builder.ToTable("player_heroes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HeroId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ActiveSkinId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.PlayerId, x.HeroId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class BattleHeroSpecialStateConfiguration : IEntityTypeConfiguration<BattleHeroSpecialState>
{
    public void Configure(EntityTypeBuilder<BattleHeroSpecialState> builder)
    {
        builder.ToTable("battle_hero_special_states");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BattleId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.HeroId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.LastIdempotencyKey).HasMaxLength(128);
        builder.HasIndex(x => new { x.BattleId, x.PlayerId, x.HeroId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
