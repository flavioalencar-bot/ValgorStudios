using System;
using Valgor.City.Data;

namespace Valgor.City.Production
{
    /// <summary>
    /// Tick baseado em timestamp (não por frame). Determinístico.
    /// </summary>
    public sealed class ProductionTickService
    {
        private readonly ResourceProductionService _production;
        private readonly IGameClock _clock;
        private readonly ProductionSettings _settings;
        private DateTime _nextTickUtc;

        public ProductionTickService(
            ResourceProductionService production,
            IGameClock clock,
            ProductionSettings settings)
        {
            _production = production;
            _clock = clock;
            _settings = settings;
            _nextTickUtc = clock.UtcNow;
        }

        public void Update()
        {
            var now = _clock.UtcNow;
            if (now < _nextTickUtc)
            {
                return;
            }

            _production.ApplyUntil(now);
            _nextTickUtc = now + _settings.TickInterval;
        }

        public void ForceApply() => _production.ApplyUntil(_clock.UtcNow);
    }
}
