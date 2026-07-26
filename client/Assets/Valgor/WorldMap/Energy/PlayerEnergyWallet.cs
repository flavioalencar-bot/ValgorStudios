using System;

namespace Valgor.WorldMap.Energy
{
    public sealed class EnergyChangedEvent : EventArgs
    {
        public EnergyChangedEvent(int previousEnergy, int currentEnergy, int maxEnergy)
        {
            PreviousEnergy = previousEnergy;
            CurrentEnergy = currentEnergy;
            MaxEnergy = maxEnergy;
        }

        public int PreviousEnergy { get; }
        public int CurrentEnergy { get; }
        public int MaxEnergy { get; }
    }

    public sealed class EnergySettings
    {
        public int CurrentEnergy { get; set; } = 100;
        public int MaxEnergy { get; set; } = 100;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public double RegenIntervalSec { get; set; } = 60;
        public int RegenAmount { get; set; } = 1;
        public int MarchDispatchCost { get; set; }
        public string PersistenceKey { get; set; } = "valgor.worldmap.energy.v1";
    }

    public sealed class PlayerEnergyWallet
    {
        public PlayerEnergyWallet(EnergySettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            CurrentEnergy = Math.Clamp(settings.CurrentEnergy, 0, settings.MaxEnergy);
            MaxEnergy = settings.MaxEnergy;
            LastUpdatedAt = settings.LastUpdatedAt;
        }

        public EnergySettings Settings { get; }
        public int CurrentEnergy { get; private set; }
        public int MaxEnergy { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
        public double RegenIntervalSec => Settings.RegenIntervalSec;
        public int RegenAmount => Settings.RegenAmount;

        public event EventHandler<EnergyChangedEvent>? Changed;

        public void Configure(int maxEnergy, double regenIntervalSec, int regenAmount)
        {
            if (maxEnergy < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxEnergy));
            }

            if (regenIntervalSec <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(regenIntervalSec));
            }

            if (regenAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(regenAmount));
            }

            MaxEnergy = maxEnergy;
            Settings.MaxEnergy = maxEnergy;
            Settings.RegenIntervalSec = regenIntervalSec;
            Settings.RegenAmount = regenAmount;
            if (CurrentEnergy > MaxEnergy)
            {
                SetCurrent(MaxEnergy);
            }
        }

        public bool TrySpend(int amount, out string error)
        {
            if (amount < 0)
            {
                error = "Custo de energia inválido.";
                return false;
            }

            if (CurrentEnergy < amount)
            {
                error = "Energia insuficiente.";
                return false;
            }

            SetCurrent(CurrentEnergy - amount);
            error = string.Empty;
            return true;
        }

        public void Add(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            SetCurrent(Math.Min(MaxEnergy, checked(CurrentEnergy + amount)));
        }

        public void SyncFromExternal(int currentEnergy, DateTime lastUpdatedAt)
        {
            LastUpdatedAt = lastUpdatedAt;
            SetCurrent(Math.Clamp(currentEnergy, 0, MaxEnergy));
        }

        public void MarkUpdated(DateTime utcNow)
        {
            LastUpdatedAt = utcNow;
            Settings.LastUpdatedAt = utcNow;
        }

        private void SetCurrent(int value)
        {
            var previous = CurrentEnergy;
            CurrentEnergy = value;
            Settings.CurrentEnergy = value;
            Settings.LastUpdatedAt = LastUpdatedAt;
            if (previous != CurrentEnergy)
            {
                Changed?.Invoke(this, new EnergyChangedEvent(previous, CurrentEnergy, MaxEnergy));
            }
        }
    }
}
