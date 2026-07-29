using System.Collections.Generic;

namespace Valgor.Core.Modules
{
    /// <summary>
    /// Stub até o módulo de dragões registrar a implementação real.
    /// </summary>
    public sealed class ProvisionalDragonGateway : IDragonGateway
    {
        public bool IsReady => false;
        public int RoostOccupantCount => 0;
        public int RoostCapacity => 0;
        public int EggUnlockCastleLevel => 20;
        public bool IsDragonContentUnlocked => false;
        public string EggJourneyPhaseLabel => "LOCKED";

        public int GetReadyDragonCount() => 0;

        public int GetProvisionalDragonPower() => 0;

        public string DescribeEggJourney() => "Sistema de dragões indisponível.";

        public IReadOnlyList<DragonStatusInfo> GetDragonStatuses() => System.Array.Empty<DragonStatusInfo>();

        public bool TryFeed(string dragonId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryStartRecovery(string dragonId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryUnlockAndHatch(string definitionId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryAcceptEggMission(out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryConquerEgg(out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryBeginIncubation(out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryCareIncubation(out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryEvolve(string dragonId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryDeployToMarch(string dragonId, string marchId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryDeployFirstReadyToMarch(string marchId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryEnterCombatForMarch(string marchId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryRecallFromMarch(string marchId, out string error)
        {
            error = "Sistema de dragões indisponível.";
            return false;
        }

        public bool TryGetStatus(string dragonId, out string displayName, out string stateLabel)
        {
            displayName = string.Empty;
            stateLabel = string.Empty;
            return false;
        }

        public bool TryGetStatusByWorldCode(string worldNodeCode, out string displayName, out string stateLabel)
        {
            displayName = string.Empty;
            stateLabel = string.Empty;
            return false;
        }

        public void SyncCastleLevel(int castleLevel)
        {
        }

        public void Tick()
        {
        }

        public void Persist()
        {
        }
    }
}
