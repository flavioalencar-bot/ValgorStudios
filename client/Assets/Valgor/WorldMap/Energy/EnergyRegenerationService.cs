using System;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Energy
{
    /// <summary>
    /// Regeneração determinística por timestamp (não depende de FPS).
    /// </summary>
    public sealed class EnergyRegenerationService
    {
        private readonly PlayerEnergyWallet _wallet;
        private readonly IWorldMapClock _clock;

        public EnergyRegenerationService(PlayerEnergyWallet wallet, IWorldMapClock clock)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public void ApplyUntil(DateTime nowUtc)
        {
            if (nowUtc < _wallet.LastUpdatedAt)
            {
                return;
            }

            if (_wallet.CurrentEnergy >= _wallet.MaxEnergy)
            {
                _wallet.MarkUpdated(nowUtc);
                return;
            }

            var interval = TimeSpan.FromSeconds(_wallet.RegenIntervalSec);
            if (interval <= TimeSpan.Zero || _wallet.RegenAmount <= 0)
            {
                _wallet.MarkUpdated(nowUtc);
                return;
            }

            var elapsed = nowUtc - _wallet.LastUpdatedAt;
            var ticks = (long)Math.Floor(elapsed.TotalSeconds / interval.TotalSeconds);
            if (ticks <= 0)
            {
                return;
            }

            var regenerated = checked((int)Math.Min(int.MaxValue, ticks * _wallet.RegenAmount));
            var room = _wallet.MaxEnergy - _wallet.CurrentEnergy;
            if (regenerated > room)
            {
                regenerated = room;
            }

            if (regenerated > 0)
            {
                _wallet.Add(regenerated);
            }

            // Avança em intervalos inteiros para não duplicar na reconexão.
            _wallet.MarkUpdated(_wallet.LastUpdatedAt.AddSeconds(ticks * interval.TotalSeconds));
            if (_wallet.CurrentEnergy >= _wallet.MaxEnergy)
            {
                _wallet.MarkUpdated(nowUtc);
            }
        }

        public void Apply() => ApplyUntil(_clock.UtcNow);

        public TimeSpan? EstimateTimeToFull()
        {
            if (_wallet.CurrentEnergy >= _wallet.MaxEnergy || _wallet.RegenAmount <= 0)
            {
                return TimeSpan.Zero;
            }

            var missing = _wallet.MaxEnergy - _wallet.CurrentEnergy;
            var ticksNeeded = (int)Math.Ceiling(missing / (double)_wallet.RegenAmount);
            return TimeSpan.FromSeconds(ticksNeeded * _wallet.RegenIntervalSec);
        }
    }
}
