using System;
using System.Collections.Generic;
using System.Linq;
using Valgor.Core.Modules;
using Valgor.Dragons.Data;
using Valgor.Dragons.Deployment;
using Valgor.Dragons.Feeding;
using Valgor.Dragons.Growth;
using Valgor.Dragons.Recovery;

namespace Valgor.Dragons.Core
{
    /// <summary>
    /// Fachada do sistema de dragões: ninho, estados, alimentação, recuperação e destaque em marcha.
    /// </summary>
    public sealed class DragonService : IDragonGateway
    {
        private readonly Dictionary<string, DragonInstance> _dragons = new();
        private readonly IDragonRepository _repository;
        private readonly DragonSettings _settings;
        private readonly Func<DateTime> _utcNow;
        private readonly DragonStateMachine _stateMachine = new();
        private IDragonResourceWallet? _wallet;
        private Action? _persistWallet;

        public DragonService(
            DragonSettings settings,
            IDragonRepository repository,
            Func<DateTime>? utcNow = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            Hunger = new DragonHungerService(_settings, _stateMachine);
            Feeding = new DragonFeedingService(_settings, _stateMachine, Hunger);
            Recovery = new DragonRecoveryService(_settings, _stateMachine);
            Deployment = new DragonDeploymentService(_stateMachine);
            Growth = new DragonGrowthService(_settings);
            Bond = new DragonBondService(_settings);
            Evolution = new DragonEvolutionService(_settings);
            Roost = new DragonRoost(
                _settings.DefaultRoostId,
                "dragon-tower",
                _settings.DefaultRoostCapacity);
        }

        public bool IsReady => true;
        public DragonRoost Roost { get; private set; }
        public int RoostOccupantCount => Roost.OccupantIds.Count;
        public int RoostCapacity => Roost.Capacity;
        public DragonHungerService Hunger { get; }
        public DragonFeedingService Feeding { get; }
        public DragonRecoveryService Recovery { get; }
        public DragonDeploymentService Deployment { get; }
        public DragonGrowthService Growth { get; }
        public DragonBondService Bond { get; }
        public DragonEvolutionService Evolution { get; }
        public DragonStateMachine StateMachine => _stateMachine;
        public IReadOnlyDictionary<string, DragonInstance> Dragons => _dragons;
        public event EventHandler<DragonChangedEvent>? Changed;

        public void BindWallet(IDragonResourceWallet? wallet, Action? persistWallet = null)
        {
            _wallet = wallet;
            _persistWallet = persistWallet;
        }

        public void LoadOrInitialize()
        {
            var snapshot = _repository.Load();
            if (snapshot?.Roost != null)
            {
                Roost = snapshot.Roost;
            }

            _dragons.Clear();
            if (snapshot != null)
            {
                foreach (var pair in snapshot.Dragons)
                {
                    _dragons[pair.Key] = pair.Value.Clone();
                    if (!string.IsNullOrEmpty(pair.Value.AssignedMarchId))
                    {
                        Deployment.RestoreAssignment(pair.Value.AssignedMarchId!, pair.Key);
                    }
                }
            }

            if (_dragons.Count == 0)
            {
                SeedStarterDragons();
            }

            foreach (var dragon in _dragons.Values)
            {
                Growth.EnsureSeedDefaults(dragon);
            }

            Tick();
            Persist();
        }

        public void Tick()
        {
            var now = _utcNow();
            foreach (var dragon in _dragons.Values.ToList())
            {
                var previous = dragon.State;
                DragonCatalog.TryGet(dragon.DefinitionId, out var definition);
                Recovery.Advance(
                    dragon,
                    now,
                    d => definition != null && Hunger.IsReadyHunger(d, definition));
                if (definition != null)
                {
                    Hunger.ApplyDecay(dragon, definition, now);
                }

                Growth.SyncWithLifecycle(dragon, previous, dragon.State);
                Growth.TryAdvance(dragon);

                if (previous != dragon.State)
                {
                    Raise(dragon.InstanceId, previous, dragon.State);
                }
            }

            Persist();
        }

        public int GetReadyDragonCount() =>
            _dragons.Values.Count(d => d.State == DragonState.Ready);

        public int GetProvisionalDragonPower() =>
            _dragons.Values
                .Where(d => d.State == DragonState.Deployed)
                .Sum(ResolveCombatPower);

        public IReadOnlyList<DragonStatusInfo> GetDragonStatuses()
        {
            var list = new List<DragonStatusInfo>(_dragons.Count);
            foreach (var pair in _dragons)
            {
                if (!DragonCatalog.TryGet(pair.Value.DefinitionId, out var definition))
                {
                    continue;
                }

                list.Add(new DragonStatusInfo(
                    pair.Key,
                    definition.DisplayName,
                    pair.Value.State.ToString().ToUpperInvariant(),
                    pair.Value.Hunger,
                    definition.MaxHunger,
                    pair.Value.GrowthStage.ToString().ToUpperInvariant(),
                    pair.Value.BondLevel,
                    pair.Value.GrowthPoints,
                    ResolveStamina(pair.Value)));
            }

            return list;
        }

        private static int ResolveStamina(DragonInstance dragon) =>
            dragon.State switch
            {
                DragonState.Ready or DragonState.Deployed => 100,
                DragonState.Resting => 70,
                DragonState.Hungry => 45,
                DragonState.Recovering => 35,
                DragonState.Exhausted or DragonState.Injured => 10,
                DragonState.Juvenile => 55,
                _ => 0
            };

        public bool TryFeed(string dragonId, out string error)
        {
            if (_wallet == null)
            {
                error = "Carteira indisponível.";
                return false;
            }

            if (!TryGet(dragonId, out var dragon) || !DragonCatalog.TryGet(dragon.DefinitionId, out var definition))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            var previous = dragon.State;
            if (!Feeding.TryFeed(dragon, definition, _wallet, out error))
            {
                return false;
            }

            Bond.AddBondPoints(dragon, _settings.BondPointsPerFeed);
            Growth.AddGrowthPoints(dragon, _settings.GrowthPointsPerFeed);
            _persistWallet?.Invoke();
            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryStartRecovery(string dragonId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            var previous = dragon.State;
            if (!Recovery.TryStartRecovery(dragon, _utcNow(), out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryDeployToMarch(string dragonId, string marchId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            var previous = dragon.State;
            if (!Deployment.TryDeploy(dragon, marchId, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryDeployFirstReadyToMarch(string marchId, out string error)
        {
            var ready = _dragons.Values.FirstOrDefault(d => d.State == DragonState.Ready);
            if (ready == null)
            {
                error = "Nenhum dragão READY disponível.";
                return false;
            }

            return TryDeployToMarch(ready.InstanceId, marchId, out error);
        }

        public bool TryEnterCombatForMarch(string marchId, out string error)
        {
            if (!Deployment.TryGetDragonForMarch(marchId, out var dragonId) ||
                !TryGet(dragonId, out var dragon))
            {
                error = "Nenhum dragão destacado nesta marcha.";
                return false;
            }

            var previous = dragon.State;
            if (!Deployment.TryEnterCombat(dragon, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryRecallFromMarch(string marchId, out string error)
        {
            if (!Deployment.TryGetDragonForMarch(marchId, out var dragonId) ||
                !TryGet(dragonId, out var dragon))
            {
                error = "Nenhum dragão destacado nesta marcha.";
                return false;
            }

            var previous = dragon.State;
            if (!Deployment.TryRecall(dragon, injured: false, out error))
            {
                return false;
            }

            Bond.AddBondPoints(dragon, _settings.BondPointsPerMission);
            Growth.AddGrowthPoints(dragon, _settings.GrowthPointsPerMission);
            Recovery.TryStartRecovery(dragon, _utcNow(), out _);
            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryEvolve(string dragonId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            var previous = dragon.State;
            if (!Evolution.TryEvolve(dragon, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryGetStatus(string dragonId, out string displayName, out string stateLabel)
        {
            displayName = string.Empty;
            stateLabel = string.Empty;
            if (!TryGet(dragonId, out var dragon) ||
                !DragonCatalog.TryGet(dragon.DefinitionId, out var definition))
            {
                return false;
            }

            displayName = definition.DisplayName;
            stateLabel = dragon.State.ToString().ToUpperInvariant();
            return true;
        }

        public bool TryGetStatusByWorldCode(string worldNodeCode, out string displayName, out string stateLabel)
        {
            displayName = string.Empty;
            stateLabel = string.Empty;
            if (!DragonCatalog.TryGetByWorldCode(worldNodeCode, out var definition))
            {
                return false;
            }

            var instance = _dragons.Values.FirstOrDefault(d => d.DefinitionId == definition.Id);
            if (instance == null)
            {
                displayName = definition.DisplayName;
                stateLabel = DragonState.Locked.ToString().ToUpperInvariant();
                return true;
            }

            displayName = definition.DisplayName;
            stateLabel = instance.State.ToString().ToUpperInvariant();
            return true;
        }

        public bool TryUnlockAndHatch(string definitionId, out string error)
        {
            if (!DragonCatalog.TryGet(definitionId, out _))
            {
                error = "Definição de dragão inválida.";
                return false;
            }

            if (!Roost.HasSlot)
            {
                error = "Ninho sem vagas.";
                return false;
            }

            var candidate = _dragons.Values.FirstOrDefault(d =>
                d.DefinitionId == definitionId &&
                d.State is DragonState.Locked or DragonState.Egg);
            if (candidate == null)
            {
                error = "Nenhum ovo disponível para esta espécie.";
                return false;
            }

            var previous = candidate.State;
            if (candidate.State == DragonState.Locked)
            {
                if (!_stateMachine.TryTransition(candidate, DragonState.Egg, out error))
                {
                    return false;
                }
            }

            if (candidate.State == DragonState.Egg)
            {
                if (!_stateMachine.TryTransition(candidate, DragonState.Hatching, out error))
                {
                    return false;
                }
            }

            Recovery.BeginTimedState(candidate, _utcNow(), _settings.HatchDurationHours);
            Persist();
            Raise(candidate.InstanceId, previous, candidate.State);
            error = string.Empty;
            return true;
        }

        public bool TryGet(string dragonId, out DragonInstance dragon) =>
            _dragons.TryGetValue(dragonId, out dragon!);

        public void Persist()
        {
            var snapshot = new DragonSnapshot
            {
                SavedAtUtc = _utcNow(),
                Roost = Roost
            };
            foreach (var pair in _dragons)
            {
                snapshot.Dragons[pair.Key] = pair.Value.Clone();
            }

            _repository.Save(snapshot);
        }

        private void SeedStarterDragons()
        {
            var ember = new DragonInstance(
                "dragon-ember-1",
                "ember-whelp",
                DragonState.Ready,
                hunger: 80,
                roostId: Roost.RoostId)
            {
                GrowthStage = DragonGrowthStage.Adult
            };
            var ash = new DragonInstance(
                "dragon-ash-1",
                "ash-drake",
                DragonState.Locked,
                hunger: 0,
                roostId: Roost.RoostId)
            {
                GrowthStage = DragonGrowthStage.Egg
            };
            _dragons[ember.InstanceId] = ember;
            _dragons[ash.InstanceId] = ash;
            Roost.OccupantIds.Clear();
            Roost.OccupantIds.Add(ember.InstanceId);
            Roost.OccupantIds.Add(ash.InstanceId);
        }

        private int ResolveCombatPower(DragonInstance dragon)
        {
            if (!DragonCatalog.TryGet(dragon.DefinitionId, out var definition))
            {
                return 0;
            }

            var power = definition.BasePower *
                        DragonGrowthService.PowerMultiplier(dragon.GrowthStage) *
                        DragonBondService.PowerMultiplier(dragon.BondLevel);
            return (int)Math.Round(power);
        }

        private void Raise(string dragonId, DragonState previous, DragonState current) =>
            Changed?.Invoke(this, new DragonChangedEvent(dragonId, previous, current));

        public static DragonService Create(
            IDragonResourceWallet? wallet = null,
            Action? persistWallet = null,
            IDragonRepository? repository = null,
            Func<DateTime>? utcNow = null)
        {
            var settings = new DragonSettings();
            var service = new DragonService(
                settings,
                repository ?? new DragonRepository(settings.PersistenceKey),
                utcNow);
            service.BindWallet(wallet, persistWallet);
            service.LoadOrInitialize();
            return service;
        }
    }
}
