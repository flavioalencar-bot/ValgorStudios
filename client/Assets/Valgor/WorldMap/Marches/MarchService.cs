using System;
using System.Collections.Generic;
using Valgor.Core.Modules;
using Valgor.WorldMap.Data;

namespace Valgor.WorldMap.Marches
{
    /// <summary>
    /// Marchas completas com ocupação de nós. Usa apenas <see cref="IHeroesGateway"/>.
    /// Avanço determinístico por timestamp.
    /// </summary>
    public sealed class MarchService
    {
        private readonly IWorldMapClock _clock;
        private readonly WorldMapSettings _settings;
        private readonly IHeroesGateway _heroes;
        private readonly Func<string, WorldMapNodeDefinition> _resolveDefinition;
        private readonly Func<string, WorldNodeInstance> _resolveNode;
        private readonly MarchStateMachine _stateMachine = new();
        private readonly MarchTravelCalculator _travel;
        private readonly WorldNodeOccupationService _occupation;
        private readonly IMarchRepository _repository;
        private MarchOrder? _active;
        private MarchOrder? _lastCompleted;
        private DateTime _lastAdvanceUtc;

        public MarchService(
            IWorldMapClock clock,
            WorldMapSettings settings,
            IHeroesGateway heroes,
            Func<string, WorldMapNodeDefinition> resolveDefinition,
            Func<string, WorldNodeInstance> resolveNode,
            WorldNodeOccupationService occupation,
            IMarchRepository? repository = null)
        {
            _clock = clock;
            _settings = settings;
            _heroes = heroes;
            _resolveDefinition = resolveDefinition;
            _resolveNode = resolveNode;
            _occupation = occupation;
            _repository = repository ?? new MarchRepository();
            _travel = new MarchTravelCalculator(settings);
            _lastAdvanceUtc = clock.UtcNow;
        }

        public MarchOrder? Active => _active;
        public MarchOrder? LastCompleted => _lastCompleted;
        public MarchStateMachine StateMachine => _stateMachine;
        public WorldNodeOccupationService Occupation => _occupation;
        public MarchTravelCalculator Travel => _travel;
        public IMarchRepository Repository => _repository;
        public DateTime LastAdvanceUtc => _lastAdvanceUtc;

        public event EventHandler<MarchChangedEvent>? Changed;

        public TimeSpan EstimateTravel(string fromNodeId, string toNodeId) =>
            _travel.Calculate(_resolveDefinition(fromNodeId), _resolveDefinition(toNodeId));

        public bool TryDispatch(string targetNodeId, string? playerId, out string error)
        {
            Advance(_clock.UtcNow);

            if (_active != null && IsLive(_active.State))
            {
                error = "Já existe uma marcha ativa.";
                return false;
            }

            var target = _resolveDefinition(targetNodeId);
            var node = _resolveNode(targetNodeId);

            if (node.Status == WorldNodeStatus.Locked || target.DefaultStatus == WorldNodeStatus.Locked)
            {
                error = "Nó bloqueado.";
                return false;
            }

            if (target is WorldCityNode city && city.IsPlayerHome)
            {
                error = "A marcha já parte da cidade do jogador.";
                return false;
            }

            if (!_occupation.CanAcceptIncomingMarch(node, string.Empty, target.Kind))
            {
                error = "Nó já ocupado por outra marcha.";
                return false;
            }

            if (!_heroes.TryReserveMarchSlot(targetNodeId, out var teamId))
            {
                error = "Slot de marcha indisponível.";
                return false;
            }

            var originId = _settings.PlayerHomeNodeId;
            var origin = _resolveDefinition(originId);
            var speed = _settings.MarchSpeedUnitsPerHour;
            var now = _clock.UtcNow;
            var arrival = _travel.EstimateArrival(now, origin, target, speed);

            var march = new MarchOrder(
                Guid.NewGuid().ToString("N"),
                playerId ?? _settings.DefaultPlayerId,
                originId,
                targetNodeId,
                teamId,
                now,
                arrival,
                MarchState.Preparing,
                speed,
                _settings.DefaultMarchCapacity,
                target.Kind);

            if (!_stateMachine.TryTransition(march, MarchState.Marching, out error))
            {
                return false;
            }

            _active = march;
            _lastAdvanceUtc = now;
            PersistMarch();
            Raise(null);
            error = string.Empty;
            return true;
        }

        public bool TryBeginGathering(out string error)
        {
            Advance(_clock.UtcNow);
            if (_active == null)
            {
                error = "Nenhuma marcha ativa.";
                return false;
            }

            var previous = _active.State;
            if (!_stateMachine.TryTransition(_active, MarchState.Gathering, out error))
            {
                return false;
            }

            PersistMarch();
            Raise(previous);
            return true;
        }

        public bool TryReturn(out string error)
        {
            Advance(_clock.UtcNow);
            if (_active == null)
            {
                error = "Nenhuma marcha ativa.";
                return false;
            }

            if (_active.State is not (MarchState.Arrived or MarchState.Gathering))
            {
                error = "Não há marcha no destino para retornar.";
                return false;
            }

            var previous = _active.State;
            if (!_stateMachine.TryTransition(_active, MarchState.Returning, out error))
            {
                return false;
            }

            ReleaseOccupation(_active);
            var duration = EstimateTravel(_active.TargetNodeId, _active.OriginNodeId);
            var now = _clock.UtcNow;
            _active.DepartureAt = now;
            _active.ReturnAt = now.Add(duration);
            _active.ArrivalAt = _active.ReturnAt.Value;
            PersistMarch();
            Raise(previous);
            return true;
        }

        public bool TryCancel(out string error)
        {
            Advance(_clock.UtcNow);
            if (_active == null)
            {
                error = "Nenhuma marcha ativa.";
                return false;
            }

            if (!_stateMachine.CanCancel(_active.State))
            {
                error = $"Cancelamento não permitido em {_active.State}.";
                return false;
            }

            var previous = _active.State;
            if (!_stateMachine.TryTransition(_active, MarchState.Cancelled, out error))
            {
                return false;
            }

            ReleaseOccupation(_active);
            PersistMarch();
            Raise(previous);
            _active = null;
            PersistMarch();
            Raise(MarchState.Cancelled);
            return true;
        }

        /// <summary>
        /// Registra carga/recompensa uma única vez. Marchas concluídas ou já recompensadas são rejeitadas.
        /// </summary>
        public bool TryDeliverLoad(long amount, out string error)
        {
            if (_active == null)
            {
                error = "Nenhuma marcha ativa.";
                return false;
            }

            if (_active.State is MarchState.Completed or MarchState.Cancelled)
            {
                error = "Marcha concluída não pode entregar recompensa novamente.";
                return false;
            }

            if (_active.RewardsDelivered)
            {
                error = "Marcha concluída não pode entregar recompensa novamente.";
                return false;
            }

            if (_active.State is not (MarchState.Arrived or MarchState.Gathering))
            {
                error = "Coleta indisponível no estado atual.";
                return false;
            }

            if (amount < 0)
            {
                error = "Carga inválida.";
                return false;
            }

            if (_active.State == MarchState.Arrived &&
                !_stateMachine.TryTransition(_active, MarchState.Gathering, out error))
            {
                return false;
            }

            var load = Math.Min(amount, _active.Capacity - _active.ResourceLoad);
            _active.ResourceLoad += load;
            _active.RewardsDelivered = true;
            PersistMarch();
            Raise(_active.State);
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
                return;
            }

            if (_active.State == MarchState.Marching && utcNow >= _active.ArrivalAt)
            {
                var previous = _active.State;
                if (_stateMachine.TryTransition(_active, MarchState.Arrived, out _))
                {
                    var node = _resolveNode(_active.TargetNodeId);
                    if (!_occupation.TryOccupy(node, _active, out _))
                    {
                        // Se ocupação falhar (corrida), cancela a chegada.
                        _stateMachine.TryTransition(_active, MarchState.Cancelled, out _);
                        ReleaseOccupation(_active);
                        PersistMarch();
                        Raise(previous);
                        _active = null;
                        PersistMarch();
                        Raise(MarchState.Cancelled);
                        _lastAdvanceUtc = utcNow;
                        return;
                    }

                    PersistMarch();
                    Raise(previous);
                }
            }
            else if (_active.State == MarchState.Returning &&
                     _active.ReturnAt.HasValue &&
                     utcNow >= _active.ReturnAt.Value)
            {
                var previous = _active.State;
                if (_stateMachine.TryTransition(_active, MarchState.Completed, out _))
                {
                    ReleaseOccupation(_active);
                    _lastCompleted = _active;
                    PersistMarch();
                    Raise(previous);
                    _active = null;
                    PersistMarch();
                    Raise(MarchState.Completed);
                }
            }

            _lastAdvanceUtc = utcNow;
        }

        public void Restore(MarchOrder? march, DateTime lastAdvanceUtc, MarchOrder? lastCompleted = null)
        {
            _active = march?.Clone();
            _lastCompleted = lastCompleted?.Clone();
            _lastAdvanceUtc = lastAdvanceUtc;
            if (_active != null)
            {
                _repository.Save(new MarchSnapshot
                {
                    SavedAtUtc = _clock.UtcNow,
                    LastAdvanceUtc = lastAdvanceUtc,
                    March = _active.Clone()
                });
            }

            Advance(_clock.UtcNow);
        }

        public void PersistMarch()
        {
            _repository.Save(new MarchSnapshot
            {
                SavedAtUtc = _clock.UtcNow,
                LastAdvanceUtc = _lastAdvanceUtc,
                March = _active?.Clone()
            });
        }

        private void ReleaseOccupation(MarchOrder march)
        {
            if (string.IsNullOrEmpty(march.OccupyingNodeId))
            {
                // Ainda pode estar marcado no nó alvo.
                var target = _resolveNode(march.TargetNodeId);
                _occupation.Release(target, march);
                return;
            }

            var node = _resolveNode(march.OccupyingNodeId);
            _occupation.Release(node, march);
        }

        private static bool IsLive(MarchState state) =>
            state is MarchState.Preparing or MarchState.Marching or MarchState.Arrived
                or MarchState.Gathering or MarchState.Returning;

        private void Raise(MarchState? previous) =>
            Changed?.Invoke(this, new MarchChangedEvent(_active, previous));
    }
}
