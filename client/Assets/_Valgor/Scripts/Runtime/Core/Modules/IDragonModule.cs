using System.Collections.Generic;

namespace Valgor.Core.Modules
{
    /// <summary>
    /// Contrato futuro do sistema de dragões (módulo carregado).
    /// </summary>
    public interface IDragonModule
    {
        bool IsReady { get; }
    }

    /// <summary>
    /// Acesso mínimo a comida/essência sem acoplar o domínio de dragões à City.
    /// </summary>
    public interface IDragonResourceWallet
    {
        long GetFood();

        long GetDragonEssence();

        long GetDiamonds();

        bool TrySpendFood(long amount);

        bool TrySpendDragonEssence(long amount);

        bool TrySpendDiamonds(long amount);
    }

    /// <summary>
    /// Resumo de um dragão para HUD/inspeção.
    /// </summary>
    public readonly struct DragonStatusInfo
    {
        public DragonStatusInfo(
            string dragonId,
            string displayName,
            string stateLabel,
            int hunger,
            int maxHunger,
            string growthStageLabel = "",
            int bondLevel = 0,
            int growthPoints = 0,
            int stamina = 100,
            int dragonLevel = 0,
            int careCount = 0,
            int careRequired = 0,
            int experience = 0,
            int experienceRequired = 0,
            int energy = 0,
            int maxEnergy = 100,
            int health = 0,
            int maxHealth = 100,
            bool isLevelingUp = false,
            int pendingLevel = 0,
            int maxAllowedLevel = 30)
        {
            DragonId = dragonId;
            DisplayName = displayName;
            StateLabel = stateLabel;
            Hunger = hunger;
            MaxHunger = maxHunger;
            GrowthStageLabel = growthStageLabel;
            BondLevel = bondLevel;
            GrowthPoints = growthPoints;
            Stamina = stamina;
            DragonLevel = dragonLevel;
            CareCount = careCount;
            CareRequired = careRequired;
            Experience = experience;
            ExperienceRequired = experienceRequired;
            Energy = energy;
            MaxEnergy = maxEnergy;
            Health = health;
            MaxHealth = maxHealth;
            IsLevelingUp = isLevelingUp;
            PendingLevel = pendingLevel;
            MaxAllowedLevel = maxAllowedLevel;
        }

        public string DragonId { get; }
        public string DisplayName { get; }
        public string StateLabel { get; }
        public int Hunger { get; }
        public int MaxHunger { get; }
        public string GrowthStageLabel { get; }
        public int BondLevel { get; }
        public int GrowthPoints { get; }
        public int Stamina { get; }
        public int DragonLevel { get; }
        public int CareCount { get; }
        public int CareRequired { get; }
        public int Experience { get; }
        public int ExperienceRequired { get; }
        public int Energy { get; }
        public int MaxEnergy { get; }
        public int Health { get; }
        public int MaxHealth { get; }
        public bool IsLevelingUp { get; }
        public int PendingLevel { get; }
        public int MaxAllowedLevel { get; }
    }

    /// <summary>
    /// Gateway fino para City/WorldMap consumirem dragões sem acoplar o domínio interno.
    /// </summary>
    public interface IDragonGateway : IDragonModule
    {
        int GetReadyDragonCount();

        int GetProvisionalDragonPower();

        int RoostOccupantCount { get; }

        int RoostCapacity { get; }

        /// <summary>Castelo mínimo para conteúdo Fase 1 (ovo).</summary>
        int EggUnlockCastleLevel { get; }

        bool IsDragonContentUnlocked { get; }

        string EggJourneyPhaseLabel { get; }

        string DescribeEggJourney();

        IReadOnlyList<DragonStatusInfo> GetDragonStatuses();

        bool TryFeed(string dragonId, out string error);

        bool TryStartRecovery(string dragonId, out string error);

        bool TryUnlockAndHatch(string definitionId, out string error);

        bool TryAcceptEggMission(out string error);

        bool TryConquerEgg(out string error);

        bool TryBeginIncubation(out string error);

        bool TryCareIncubation(out string error);

        bool TryEvolve(string dragonId, out string error);

        bool TryDeployToMarch(string dragonId, string marchId, out string error);

        bool TryDeployFirstReadyToMarch(string marchId, out string error);

        bool TryEnterCombatForMarch(string marchId, out string error);

        bool TryRecallFromMarch(string marchId, out string error);

        bool TryGetStatus(string dragonId, out string displayName, out string stateLabel);

        bool TryGetStatusByWorldCode(string worldNodeCode, out string displayName, out string stateLabel);

        /// <summary>Espelha nível do Castelo da City (chamado no tick da cidade).</summary>
        void SyncCastleLevel(int castleLevel);

        /// <summary>Espelha níveis Castelo + Torre para caps Fase 2.</summary>
        void SyncBuildingLevels(int castleLevel, int towerLevel);

        int GetMaxAllowedDragonLevel();

        string DescribeDragonProgression(string dragonId);

        bool TryStartLevelUp(string dragonId, out string error);

        bool TryInstantCompleteLevelUp(string dragonId, out string error);

        /// <summary>Fase 3 — configura slot de habilidade (0–2). abilityId vazio limpa o slot.</summary>
        bool TrySetAbilitySlot(string dragonId, int slotIndex, string abilityId, out string error);

        string DescribeDragonAbilities(string dragonId);

        /// <summary>
        /// Aplica resultado do combate PvE ao dragão da marcha (energia/saúde/XP/ferida).
        /// difficultyBand: 0 Trivial … 3 Hard, outros = Failed.
        /// </summary>
        bool TryApplyCombatOutcomeForMarch(
            string marchId,
            bool victory,
            int difficultyBand,
            out string error,
            out string summary);

        int GetSupportPowerForMarch(string marchId);

        /// <summary>Fase 4 — vínculo de montaria com herói compatível.</summary>
        bool TryCreateMountBond(string dragonId, string heroId, out string error);

        bool TryClearMountBond(string dragonId, out string error);

        bool TryTrainMountBond(string dragonId, out string error);

        bool TryEquipMount(string dragonId, out string error);

        bool TryUnequipMount(string dragonId, out string error);

        string DescribeMountBond(string dragonId);

        bool TryGetMarchDragonPresence(
            string marchId,
            out string dragonId,
            out string stageLabel,
            out bool isMounted,
            out string bondedHeroId);

        void Tick();

        void Persist();
    }
}
