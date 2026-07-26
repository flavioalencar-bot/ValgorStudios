using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valgor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroesSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "battle_hero_special_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BattleId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsAlive = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CooldownUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastIdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_battle_hero_special_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "faction_advantages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttackerFactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DefenderFactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DamageMultiplier = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faction_advantages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "faction_team_bonuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SameFactionCount = table.Column<int>(type: "integer", nullable: false),
                    OtherFactionCount = table.Column<int>(type: "integer", nullable: false),
                    TotalTroopAttackMultiplier = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faction_team_bonuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hero_definitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Gender = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Rarity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClassName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Role = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Position = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Weapon = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Element = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DefaultSkinId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PrefabAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PortraitAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hero_factions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Color = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Archetype = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BeatsFactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LosesToFactionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_factions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hero_skins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HeroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Rarity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ModelAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MaterialSetAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PortraitAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CompetitiveNormalization = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_skins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hero_special_effects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SpecialPowerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_special_effects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "player_heroes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Stars = table.Column<int>(type: "integer", nullable: false),
                    Fragments = table.Column<int>(type: "integer", nullable: false),
                    ActiveSkinId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Unlocked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_heroes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hero_special_powers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HeroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActiveDurationSec = table.Column<float>(type: "real", nullable: false),
                    CooldownSec = table.Column<float>(type: "real", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Interruptible = table.Column<bool>(type: "boolean", nullable: false),
                    CanActivateWhileControlled = table.Column<bool>(type: "boolean", nullable: false),
                    AnimationState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VfxAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SfxAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_special_powers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hero_special_powers_hero_definitions_HeroId",
                        column: x => x.HeroId,
                        principalTable: "hero_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_battle_hero_special_states_BattleId_PlayerId_HeroId",
                table: "battle_hero_special_states",
                columns: new[] { "BattleId", "PlayerId", "HeroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faction_advantages_AttackerFactionId_DefenderFactionId",
                table: "faction_advantages",
                columns: new[] { "AttackerFactionId", "DefenderFactionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faction_team_bonuses_SameFactionCount_OtherFactionCount",
                table: "faction_team_bonuses",
                columns: new[] { "SameFactionCount", "OtherFactionCount" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hero_definitions_FactionId",
                table: "hero_definitions",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_hero_skins_HeroId",
                table: "hero_skins",
                column: "HeroId");

            migrationBuilder.CreateIndex(
                name: "IX_hero_special_effects_SpecialPowerId_SortOrder",
                table: "hero_special_effects",
                columns: new[] { "SpecialPowerId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hero_special_powers_HeroId",
                table: "hero_special_powers",
                column: "HeroId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_heroes_PlayerId_HeroId",
                table: "player_heroes",
                columns: new[] { "PlayerId", "HeroId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "battle_hero_special_states");

            migrationBuilder.DropTable(
                name: "faction_advantages");

            migrationBuilder.DropTable(
                name: "faction_team_bonuses");

            migrationBuilder.DropTable(
                name: "hero_factions");

            migrationBuilder.DropTable(
                name: "hero_skins");

            migrationBuilder.DropTable(
                name: "hero_special_effects");

            migrationBuilder.DropTable(
                name: "hero_special_powers");

            migrationBuilder.DropTable(
                name: "player_heroes");

            migrationBuilder.DropTable(
                name: "hero_definitions");
        }
    }
}
