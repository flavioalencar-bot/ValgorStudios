using System;
using System.Collections.Generic;
using Valgor.Core.Modules;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Core
{
    /// <summary>
    /// Marcha provisória. Usa apenas <see cref="IHeroesGateway"/> — sem lógica interna de heróis.
    /// Avanço determinístico por timestamp (não depende de FPS).
    /// </summary>
    public sealed class MarchService
    {
        private readonly IWorldMapClock _clock;
        private readonly WorldMapSettings _settings;
        private readonly IHeroesGateway _heroes;
        private readonly Func<string, WorldMapNodeDefinition> _resolveDefinition;
        private MarchOrder? _active;
        private DateTime _lastAdvanceUtc;

        public MarchService(
            IWorldMapClock clock,
            WorldMapSettings settings,
            IHeroesGateway heroes,
            Func<string, WorldMapNodeDefinition> resolveDefinition)
        {
            _clock = clock;
            _settings = settings;
            _heroes = heroes;
            _resolveDefinition = resolveDefinition;
            _lastAdvanceUtc = clock.UtcNow;
        }

        public MarchOrder? Active => _active;
        public event EventHandler<MarchChangedEventArgs>? Changed;

        public TimeSpan EstimateTravel(string fromNodeId, string toNodeId)
        {
            var from = _resolveDefinition(fromNodeId);
            var to = _resolveDefinition(toNodeId);
            return TravelTimeCalculator.Calculate(from, to, _settings);
        }

        public bool TryDispatch(string targetNodeId, out string error)
        {
            Advance(_clock.UtcNow);
            if (_active != null &&
                _active.Phase is MarchPhase.TravelingOutbound or MarchPhase.Returning or MarchPhase.Arrived)
            {
                error = "Já existe uma marcha ativa.";
                return false;
            }

            var target = _resolveDefinition(targetNodeId);
            if (target.DefaultStatus == WorldNodeStatus.Locked)
            {
                error = "Nó bloqueado.";
                return false;
            }

            if (target is WorldCityNode city && city.IsPlayerHome)
            {
                error = "A marcha já parte da cidade do jogador.";
                return false;
            }

            if (!_heroes.TryReserveMarchSlot(targetNodeId, out var reservationId))
            {
                error = "Slot de marcha indisponível.";
                return false;
            }

            var originId = _settings.PlayerHomeNodeId;
            var duration = EstimateTravel(originId, targetNodeId);
            var now = _clock.UtcNow;
            _active = new MarchOrder(
                Guid.NewGuid().ToString("N"),
                reservationId,
                originId,
                targetNodeId,
                now,
                now.Add(duration),
                MarchPhase.TravelingOutbound)
            {
                CurrentNodeId = originId
            };
            _lastAdvanceUtc = now;
            Raise();
            error = string.Empty;
            return true;
        }

        public bool TryReturn(out string error)
        {
            Advance(_clock.UtcNow);
            if (_active == null || _active.Phase != MarchPhase.Arrived)
            {
                error = "Não há marcha no destino para retornar.";
                return false;
            }

            var duration = EstimateTravel(_active.TargetNodeId, _active.OriginNodeId);
            var now = _clock.UtcNow;
            _active.DepartedAtUtc = now;
            _active.ArrivesAtUtc = now.Add(duration);
            _active.Phase = MarchPhase.Returning;
            _active.CurrentNodeId = _active.TargetNodeId;
            Raise();
            error = string.Empty;
            return true;
        }

        public void Advance(DateTime utcNow)
        {
            if (_active == null)
            {
                _lastAdvanceUtc = utcNow;
                return;
            }

            if (utcNow < _lastAdvanceUtc)
            {
                // Relógio não regride produção/marcha — evita duplicação em reconexão.
                return;
            }

            if (_active.Phase == MarchPhase.TravelingOutbound && utcNow >= _active.ArrivesAtUtc)
            {
                _active.Phase = MarchPhase.Arrived;
                _active.CurrentNodeId = _active.TargetNodeId;
                Raise();
            }
            else if (_active.Phase == MarchPhase.Returning && utcNow >= _active.ArrivesAtUtc)
            {
                _active.Phase = MarchPhase.Completed;
                _active.CurrentNodeId = _active.OriginNodeId;
                Raise();
                _active = null;
                Raise();
            }

            _lastAdvanceUtc = utcNow;
        }

        public void Restore(MarchOrder? march, DateTime lastAdvanceUtc)
        {
            _active = march;
            _lastAdvanceUtc = lastAdvanceUtc;
            Advance(_clock.UtcNow);
        }

        private void Raise() => Changed?.Invoke(this, new MarchChangedEventArgs(_active));
    }
}
