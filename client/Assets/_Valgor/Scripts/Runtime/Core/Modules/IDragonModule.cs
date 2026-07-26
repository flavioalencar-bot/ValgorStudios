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

        bool TrySpendFood(long amount);

        bool TrySpendDragonEssence(long amount);
    }

    /// <summary>
    /// Resumo de um dragão para HUD/inspeção.
    /// </summary>
    public readonly struct DragonStatusInfo
    {
        public DragonStatusInfo(string dragonId, string displayName, string stateLabel, int hunger, int maxHunger)
        {
            DragonId = dragonId;
            DisplayName = displayName;
            StateLabel = stateLabel;
            Hunger = hunger;
            MaxHunger = maxHunger;
        }

        public string DragonId { get; }
        public string DisplayName { get; }
        public string StateLabel { get; }
        public int Hunger { get; }
        public int MaxHunger { get; }
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

        IReadOnlyList<DragonStatusInfo> GetDragonStatuses();

        bool TryFeed(string dragonId, out string error);

        bool TryStartRecovery(string dragonId, out string error);

        bool TryUnlockAndHatch(string definitionId, out string error);

        bool TryDeployToMarch(string dragonId, string marchId, out string error);

        bool TryDeployFirstReadyToMarch(string marchId, out string error);

        bool TryEnterCombatForMarch(string marchId, out string error);

        bool TryRecallFromMarch(string marchId, out string error);

        bool TryGetStatus(string dragonId, out string displayName, out string stateLabel);

        bool TryGetStatusByWorldCode(string worldNodeCode, out string displayName, out string stateLabel);

        void Tick();

        void Persist();
    }
}
