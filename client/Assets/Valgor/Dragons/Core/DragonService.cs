using System;
using System.Collections.Generic;
using System.Linq;
using Valgor.Core.Modules;
using Valgor.Dragons.Combat;
using Valgor.Dragons.Data;
using Valgor.Dragons.Deployment;
using Valgor.Dragons.Feeding;
using Valgor.Dragons.Growth;
using Valgor.Dragons.Mount;
using Valgor.Dragons.Recovery;

namespace Valgor.Dragons.Core
{
    /// <summary>
    /// Fachada: ovo (F1) + progressão (F2) + combate PvE (F3) + montaria (F4).
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
        private DragonEggJourneyPhase _eggJourneyPhase = DragonEggJourneyPhase.Locked;
        private int _syncedCastleLevel;
        private int _syncedTowerLevel = 1;
        private int _energyDecayAccumulator;
        private string _lastCombatSummary = string.Empty;

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
            Progression = new DragonProgressionService(_settings, _utcNow);
            Abilities = new DragonAbilityService();
            Combat = new DragonCombatService(_settings, Abilities);
            Mount = new DragonMountService(_settings);
            Roost = new DragonRoost(
                _settings.DefaultRoostId,
                "dragon-tower",
                _settings.DefaultRoostCapacity);
        }

        public bool IsReady => true;
        public DragonRoost Roost { get; private set; }
        public int RoostOccupantCount => Roost.OccupantIds.Count;
        public int RoostCapacity => Roost.Capacity;
        public int EggUnlockCastleLevel => _settings.EggUnlockCastleLevel;
        public bool IsDragonContentUnlocked =>
            _eggJourneyPhase >= DragonEggJourneyPhase.Unlocked ||
            _syncedCastleLevel >= _settings.EggUnlockCastleLevel;
        public string EggJourneyPhaseLabel => _eggJourneyPhase.ToString().ToUpperInvariant();
        public DragonEggJourneyPhase EggJourneyPhase => _eggJourneyPhase;
        public DragonHungerService Hunger { get; }
        public DragonFeedingService Feeding { get; }
        public DragonRecoveryService Recovery { get; }
        public DragonDeploymentService Deployment { get; }
        public DragonGrowthService Growth { get; }
        public DragonBondService Bond { get; }
        public DragonEvolutionService Evolution { get; }
        public DragonProgressionService Progression { get; }
        public DragonAbilityService Abilities { get; }
        public DragonCombatService Combat { get; }
        public DragonMountService Mount { get; }
        public DragonStateMachine StateMachine => _stateMachine;
        public IReadOnlyDictionary<string, DragonInstance> Dragons => _dragons;
        public string LastCombatSummary => _lastCombatSummary;
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
                _eggJourneyPhase = snapshot.EggJourneyPhase;
                _syncedCastleLevel = snapshot.SyncedCastleLevel;
                _syncedTowerLevel = Math.Max(1, snapshot.SyncedTowerLevel);
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
                SeedPhase1Egg();
            }
            else
            {
                MigrateLegacyJourneyIfNeeded();
            }

            foreach (var dragon in _dragons.Values)
            {
                Growth.EnsureSeedDefaults(dragon);
                Progression.EnsureCombatStats(dragon);
                Abilities.EnsureDefaults(dragon);
            }

            SyncBuildingLevels(_syncedCastleLevel, _syncedTowerLevel);
            Tick();
            Persist();
        }

        public void SyncCastleLevel(int castleLevel) =>
            SyncBuildingLevels(castleLevel, _syncedTowerLevel);

        public void SyncBuildingLevels(int castleLevel, int towerLevel)
        {
            _syncedCastleLevel = Math.Max(0, castleLevel);
            _syncedTowerLevel = Math.Max(0, towerLevel);
            if (Roost != null && _syncedTowerLevel > 0)
            {
                Roost.Level = Math.Max(Roost.Level, _syncedTowerLevel);
            }

            if (_syncedCastleLevel >= _settings.EggUnlockCastleLevel &&
                _eggJourneyPhase == DragonEggJourneyPhase.Locked)
            {
                _eggJourneyPhase = DragonEggJourneyPhase.Unlocked;
                Persist();
            }
        }

        public int GetMaxAllowedDragonLevel() =>
            DragonProgressionRules.EffectiveMaxLevel(_syncedCastleLevel, _syncedTowerLevel);

        public string DescribeEggJourney()
        {
            var need = _settings.EggUnlockCastleLevel;
            return _eggJourneyPhase switch
            {
                DragonEggJourneyPhase.Locked =>
                    $"Conteúdo dracônico bloqueado. Evolua o Castelo até Nv.{need} (atual {_syncedCastleLevel}).",
                DragonEggJourneyPhase.Unlocked =>
                    "Castelo pronto. Aceite a missão do Ovo na Torre dos Dragões.",
                DragonEggJourneyPhase.MissionActive =>
                    "Missão ativa: conquiste o Ovo Dracônico (Buscar o Ovo na Torre).",
                DragonEggJourneyPhase.EggOwned =>
                    "Ovo conquistado. Inicie a incubação no ninho.",
                DragonEggJourneyPhase.Incubating =>
                    DescribeIncubationStatus(),
                DragonEggJourneyPhase.Born =>
                    "Dragão nascido (Nv.1). Cuide e alimente na Torre.",
                _ => "Jornada do ovo."
            };
        }

        private string DescribeIncubationStatus()
        {
            if (!TryGetFirstDragon(out var dragon))
            {
                return "Incubação em andamento.";
            }

            var care = $"{dragon.CareCount}/{_settings.CareRequiredForHatch}";
            var remaining = dragon.StateEndsAtUtc.HasValue
                ? Math.Max(0, (dragon.StateEndsAtUtc.Value - _utcNow()).TotalMinutes)
                : 0;
            return $"Incubando — cuidados {care} · restante ~{remaining:0} min. Cuide do ovo até nascer.";
        }

        public void Tick()
        {
            var now = _utcNow();
            _energyDecayAccumulator++;
            foreach (var dragon in _dragons.Values.ToList())
            {
                var previous = dragon.State;
                var previousLevel = dragon.DragonLevel;
                DragonCatalog.TryGet(dragon.DefinitionId, out var definition);
                Progression.AdvanceTimers(dragon);
                Recovery.Advance(
                    dragon,
                    now,
                    d => definition != null && Hunger.IsReadyHunger(d, definition),
                    CanCompleteHatch);
                if (definition != null)
                {
                    Hunger.ApplyDecay(dragon, definition, now);
                }

                Growth.SyncWithLifecycle(dragon, previous, dragon.State);
                Growth.TryAdvance(dragon);
                Progression.ApplyStageFromLevel(dragon);

                if (_energyDecayAccumulator >= 30 &&
                    dragon.DragonLevel >= 1 &&
                    !dragon.IsLevelingUp &&
                    dragon.State is DragonState.Ready or DragonState.Resting)
                {
                    dragon.Energy = Math.Max(0, dragon.Energy - _settings.EnergyDecayPerTick);
                }

                if (previous == DragonState.Hatching && dragon.State == DragonState.Juvenile)
                {
                    OnDragonBorn(dragon);
                }

                if (previous != dragon.State || previousLevel != dragon.DragonLevel)
                {
                    Raise(dragon.InstanceId, previous, dragon.State);
                }
            }

            if (_energyDecayAccumulator >= 30)
            {
                _energyDecayAccumulator = 0;
            }

            Persist();
        }

        private void OnDragonBorn(DragonInstance dragon)
        {
            if (dragon.DragonLevel < 1)
            {
                dragon.DragonLevel = 1;
            }

            dragon.Energy = _settings.MaxEnergy;
            dragon.Health = _settings.MaxHealth;
            dragon.Experience = 0;
            Progression.ApplyStageFromLevel(dragon);
            Abilities.EnsureDefaults(dragon);
            _eggJourneyPhase = DragonEggJourneyPhase.Born;
        }

        private bool CanCompleteHatch(DragonInstance dragon) =>
            dragon.CareCount >= _settings.CareRequiredForHatch;

        public int GetReadyDragonCount() =>
            _dragons.Values.Count(d => d.State == DragonState.Ready);

        public int GetProvisionalDragonPower() =>
            _dragons.Values
                .Where(d => d.State == DragonState.Deployed)
                .Sum(ResolveCombatPower);

        public IReadOnlyList<DragonStatusInfo> GetDragonStatuses()
        {
            var maxAllowed = GetMaxAllowedDragonLevel();
            var list = new List<DragonStatusInfo>(_dragons.Count);
            foreach (var pair in _dragons)
            {
                if (!DragonCatalog.TryGet(pair.Value.DefinitionId, out var definition))
                {
                    continue;
                }

                var d = pair.Value;
                var xpNeed = DragonProgressionRules.ExperienceRequiredForLevel(d.DragonLevel);
                list.Add(new DragonStatusInfo(
                    pair.Key,
                    definition.DisplayName,
                    d.IsLevelingUp ? "LEVELING" : d.State.ToString().ToUpperInvariant(),
                    d.Hunger,
                    definition.MaxHunger,
                    DragonProgressionRules.StageDisplayName(d.GrowthStage),
                    d.BondLevel,
                    d.GrowthPoints,
                    ResolveStamina(d),
                    d.DragonLevel,
                    d.CareCount,
                    _settings.CareRequiredForHatch,
                    d.Experience,
                    xpNeed,
                    d.Energy,
                    _settings.MaxEnergy,
                    d.Health,
                    _settings.MaxHealth,
                    d.IsLevelingUp,
                    d.PendingLevel,
                    maxAllowed));
            }

            return list;
        }

        private int ResolveStamina(DragonInstance dragon) =>
            dragon.DragonLevel >= 1 ? dragon.Energy : 0;

        public bool TryAcceptEggMission(out string error)
        {
            SyncCastleLevel(_syncedCastleLevel);
            if (_eggJourneyPhase == DragonEggJourneyPhase.Locked)
            {
                error = $"Castelo Nv.{_settings.EggUnlockCastleLevel} necessário (atual {_syncedCastleLevel}).";
                return false;
            }

            if (_eggJourneyPhase != DragonEggJourneyPhase.Unlocked)
            {
                error = _eggJourneyPhase > DragonEggJourneyPhase.Unlocked
                    ? "Missão do ovo já foi aceita."
                    : "Missão indisponível.";
                return false;
            }

            _eggJourneyPhase = DragonEggJourneyPhase.MissionActive;
            Persist();
            error = string.Empty;
            return true;
        }

        public bool TryConquerEgg(out string error)
        {
            if (_eggJourneyPhase != DragonEggJourneyPhase.MissionActive)
            {
                error = _eggJourneyPhase < DragonEggJourneyPhase.MissionActive
                    ? "Aceite a missão do Ovo primeiro."
                    : "Ovo já conquistado.";
                return false;
            }

            if (!TryGetFirstDragon(out var dragon))
            {
                error = "Ninho sem ovo.";
                return false;
            }

            if (dragon.State != DragonState.Locked)
            {
                error = "Ovo já está no ninho.";
                return false;
            }

            var previous = dragon.State;
            if (!_stateMachine.TryTransition(dragon, DragonState.Egg, out error))
            {
                return false;
            }

            dragon.GrowthStage = DragonGrowthStage.Egg;
            dragon.CareCount = 0;
            dragon.DragonLevel = 0;
            _eggJourneyPhase = DragonEggJourneyPhase.EggOwned;
            Persist();
            Raise(dragon.InstanceId, previous, dragon.State);
            error = string.Empty;
            return true;
        }

        public bool TryBeginIncubation(out string error)
        {
            if (_eggJourneyPhase != DragonEggJourneyPhase.EggOwned)
            {
                error = _eggJourneyPhase < DragonEggJourneyPhase.EggOwned
                    ? "Conquiste o ovo primeiro."
                    : "Incubação já iniciada ou concluída.";
                return false;
            }

            if (!TryGetFirstDragon(out var dragon) || dragon.State != DragonState.Egg)
            {
                error = "Nenhum ovo no ninho para incubar.";
                return false;
            }

            var previous = dragon.State;
            if (!_stateMachine.TryTransition(dragon, DragonState.Hatching, out error))
            {
                return false;
            }

            dragon.CareCount = 0;
            Recovery.BeginTimedState(dragon, _utcNow(), _settings.HatchDurationHours);
            _eggJourneyPhase = DragonEggJourneyPhase.Incubating;
            Persist();
            Raise(dragon.InstanceId, previous, dragon.State);
            error = string.Empty;
            return true;
        }

        public bool TryCareIncubation(out string error)
        {
            if (_eggJourneyPhase != DragonEggJourneyPhase.Incubating)
            {
                error = "Não há incubação em andamento.";
                return false;
            }

            if (!TryGetFirstDragon(out var dragon) || dragon.State != DragonState.Hatching)
            {
                error = "Ovo não está incubando.";
                return false;
            }

            if (dragon.CareCount >= _settings.CareRequiredForHatch)
            {
                error = "Cuidados suficientes — aguarde o nascimento.";
                return false;
            }

            if (_wallet == null)
            {
                error = "Carteira indisponível.";
                return false;
            }

            if (!_wallet.TrySpendFood(_settings.CareFoodCost))
            {
                error = $"Comida insuficiente (precisa {_settings.CareFoodCost}).";
                return false;
            }

            dragon.CareCount++;
            dragon.LastUpdatedUtc = _utcNow();

            // Cada cuidado acelera um pouco a incubação.
            if (dragon.StateEndsAtUtc.HasValue && _settings.CareExtendsHatchHours > 0)
            {
                var accelerated = dragon.StateEndsAtUtc.Value.AddHours(-_settings.CareExtendsHatchHours);
                var minEnd = _utcNow().AddMinutes(0.5);
                dragon.StateEndsAtUtc = accelerated < minEnd ? minEnd : accelerated;
            }

            _persistWallet?.Invoke();
            Persist();
            error = string.Empty;

            // Pode nascer imediatamente se timer + care ok.
            Tick();
            return true;
        }

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

            if (dragon.State is DragonState.Locked or DragonState.Egg or DragonState.Hatching)
            {
                error = "Alimente após o nascimento. Use Cuidar durante a incubação.";
                return false;
            }

            if (dragon.IsLevelingUp)
            {
                error = "Aguarde a evolução/ritual concluir.";
                return false;
            }

            var previous = dragon.State;
            if (!Feeding.TryFeed(dragon, definition, _wallet, out error))
            {
                return false;
            }

            Bond.AddBondPoints(dragon, _settings.BondPointsPerFeed);
            Growth.AddGrowthPoints(dragon, _settings.GrowthPointsPerFeed);
            Progression.AddExperience(dragon, _settings.ExperiencePerFeed);
            Progression.ApplyFeedRestores(dragon);
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

            if (dragon.IsLevelingUp)
            {
                error = "Dragão em evolução/ritual — não pode ser destacado.";
                return false;
            }

            if (!Combat.CanSupportCombat(dragon, out error))
            {
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
            var ready = _dragons.Values.FirstOrDefault(d =>
                d.State == DragonState.Ready &&
                Combat.CanSupportCombat(d, out _));
            if (ready == null)
            {
                // Fallback: qualquer READY (mensagem de energia/saúde no deploy).
                ready = _dragons.Values.FirstOrDefault(d => d.State == DragonState.Ready);
            }

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

            if (!Combat.CanSupportCombat(dragon, out error))
            {
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

            var injured = dragon.PendingCombatInjury;
            var previous = dragon.State;
            if (!Deployment.TryRecall(dragon, injured, out error))
            {
                return false;
            }

            dragon.PendingCombatInjury = false;
            Bond.AddBondPoints(dragon, _settings.BondPointsPerMission);
            Growth.AddGrowthPoints(dragon, _settings.GrowthPointsPerMission);
            if (dragon.IsMounted && !string.IsNullOrEmpty(dragon.BondedHeroId))
            {
                Mount.AddMountBondPoints(dragon, _settings.MountBondPointsPerMission);
            }

            Recovery.TryStartRecovery(dragon, _utcNow(), out _);
            Persist();
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TrySetAbilitySlot(string dragonId, int slotIndex, string abilityId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            if (slotIndex < 0 || slotIndex > 2)
            {
                error = "Slot inválido (0–2).";
                return false;
            }

            var ability = DragonAbilityId.None;
            if (!string.IsNullOrWhiteSpace(abilityId) &&
                !DragonAbilityCatalog.TryParse(abilityId, out ability))
            {
                error = "Habilidade inválida.";
                return false;
            }

            var slot = (DragonAbilitySlot)slotIndex;
            if (!Abilities.TrySetSlot(dragon, slot, ability, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            return true;
        }

        public string DescribeDragonAbilities(string dragonId)
        {
            if (!TryGet(dragonId, out var dragon) || dragon.DragonLevel < 1)
            {
                return "Habilidades indisponíveis.";
            }

            Abilities.EnsureDefaults(dragon);
            var unlocked = Abilities.GetUnlocked(dragon.DragonLevel);
            var loadout = Abilities.DescribeLoadout(dragon);
            return $"Loadout: {loadout}. Desbloqueadas: {unlocked.Count}.";
        }

        public bool TryApplyCombatOutcomeForMarch(
            string marchId,
            bool victory,
            int difficultyBand,
            out string error,
            out string summary)
        {
            summary = string.Empty;
            if (!Deployment.TryGetDragonForMarch(marchId, out var dragonId) ||
                !TryGet(dragonId, out var dragon) ||
                !DragonCatalog.TryGet(dragon.DefinitionId, out var definition))
            {
                error = "Nenhum dragão em combate nesta marcha.";
                return false;
            }

            var difficulty = difficultyBand switch
            {
                0 => DragonCombatDifficulty.Trivial,
                1 => DragonCombatDifficulty.Easy,
                2 => DragonCombatDifficulty.Fair,
                3 => DragonCombatDifficulty.Hard,
                _ => DragonCombatDifficulty.Failed
            };

            var power = Combat.ResolveSupportPower(dragon, definition);
            var result = Combat.ApplyOutcome(dragon, victory, difficulty, power);
            dragon.PendingCombatInjury = result.Injured;
            _lastCombatSummary = result.Summary;
            summary = result.Summary;
            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            error = string.Empty;
            return true;
        }

        public int GetSupportPowerForMarch(string marchId)
        {
            if (!Deployment.TryGetDragonForMarch(marchId, out var dragonId) ||
                !TryGet(dragonId, out var dragon))
            {
                return 0;
            }

            return ResolveCombatPower(dragon);
        }

        public bool TryCreateMountBond(string dragonId, string heroId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            if (!Mount.TryCreateBond(dragon, heroId, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            return true;
        }

        public bool TryClearMountBond(string dragonId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            if (!Mount.TryClearBond(dragon, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            return true;
        }

        public bool TryTrainMountBond(string dragonId, out string error)
        {
            if (_wallet == null)
            {
                error = "Carteira indisponível.";
                return false;
            }

            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            if (string.IsNullOrEmpty(dragon.BondedHeroId))
            {
                error = "Crie o vínculo de montaria primeiro.";
                return false;
            }

            if (dragon.State is DragonState.Deployed or DragonState.Recovering or DragonState.Injured
                or DragonState.Exhausted)
            {
                error = "Treine o vínculo com o dragão no ninho.";
                return false;
            }

            if (_wallet.GetFood() < _settings.MountTrainFoodCost ||
                _wallet.GetDragonEssence() < _settings.MountTrainEssenceCost)
            {
                error = "Recursos insuficientes para treinar o vínculo.";
                return false;
            }

            if (!_wallet.TrySpendFood(_settings.MountTrainFoodCost) ||
                !_wallet.TrySpendDragonEssence(_settings.MountTrainEssenceCost))
            {
                error = "Falha ao debitar recursos.";
                return false;
            }

            Mount.AddMountBondPoints(dragon, _settings.MountBondPointsPerTrain);
            _persistWallet?.Invoke();
            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            error = string.Empty;
            return true;
        }

        public bool TryEquipMount(string dragonId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            if (!Mount.TryEquipMount(dragon, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            return true;
        }

        public bool TryUnequipMount(string dragonId, out string error)
        {
            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            if (!Mount.TryUnequipMount(dragon, out error))
            {
                return false;
            }

            Persist();
            Raise(dragonId, dragon.State, dragon.State);
            return true;
        }

        public string DescribeMountBond(string dragonId)
        {
            if (!TryGet(dragonId, out var dragon) || dragon.DragonLevel < 1)
            {
                return "Montaria indisponível.";
            }

            return Mount.Describe(dragon);
        }

        public bool TryGetMarchDragonPresence(
            string marchId,
            out string dragonId,
            out string stageLabel,
            out bool isMounted,
            out string bondedHeroId)
        {
            dragonId = string.Empty;
            stageLabel = string.Empty;
            isMounted = false;
            bondedHeroId = string.Empty;
            if (!Deployment.TryGetDragonForMarch(marchId, out var id) || !TryGet(id, out var dragon))
            {
                return false;
            }

            dragonId = id;
            stageLabel = DragonProgressionRules.StageDisplayName(dragon.GrowthStage);
            isMounted = dragon.IsMounted;
            bondedHeroId = dragon.BondedHeroId ?? string.Empty;
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

        /// <summary>
        /// Compat: inicia incubação se o ovo já estiver conquistado; não pula cuidados.
        /// </summary>
        public bool TryUnlockAndHatch(string definitionId, out string error)
        {
            if (!DragonCatalog.TryGet(definitionId, out _))
            {
                error = "Definição de dragão inválida.";
                return false;
            }

            if (_eggJourneyPhase == DragonEggJourneyPhase.EggOwned)
            {
                return TryBeginIncubation(out error);
            }

            if (_eggJourneyPhase == DragonEggJourneyPhase.MissionActive)
            {
                if (!TryConquerEgg(out error))
                {
                    return false;
                }

                return TryBeginIncubation(out error);
            }

            error = "Conquiste o ovo pela jornada (Castelo ≥ 20 → missão → conquista).";
            return false;
        }

        public bool TryGet(string dragonId, out DragonInstance dragon) =>
            _dragons.TryGetValue(dragonId, out dragon!);

        public void Persist()
        {
            var snapshot = new DragonSnapshot
            {
                SavedAtUtc = _utcNow(),
                Roost = Roost,
                EggJourneyPhase = _eggJourneyPhase,
                SyncedCastleLevel = _syncedCastleLevel,
                SyncedTowerLevel = _syncedTowerLevel,
                PersistenceVersion = 5
            };
            foreach (var pair in _dragons)
            {
                snapshot.Dragons[pair.Key] = pair.Value.Clone();
            }

            _repository.Save(snapshot);
        }

        private bool TryGetFirstDragon(out DragonInstance dragon)
        {
            if (_dragons.TryGetValue(_settings.FirstDragonInstanceId, out dragon!))
            {
                return true;
            }

            dragon = _dragons.Values.FirstOrDefault()!;
            return dragon != null;
        }

        private void SeedPhase1Egg()
        {
            _eggJourneyPhase = DragonEggJourneyPhase.Locked;
            var ember = new DragonInstance(
                _settings.FirstDragonInstanceId,
                _settings.FirstDragonDefinitionId,
                DragonState.Locked,
                hunger: 0,
                roostId: Roost.RoostId)
            {
                GrowthStage = DragonGrowthStage.Egg,
                DragonLevel = 0,
                CareCount = 0
            };
            _dragons[ember.InstanceId] = ember;
            Roost.OccupantIds.Clear();
            Roost.OccupantIds.Add(ember.InstanceId);
        }

        private void MigrateLegacyJourneyIfNeeded()
        {
            if (_eggJourneyPhase >= DragonEggJourneyPhase.Born)
            {
                return;
            }

            var anyBorn = _dragons.Values.Any(d =>
                d.State is DragonState.Juvenile or DragonState.Ready or DragonState.Deployed
                    or DragonState.Hungry or DragonState.Resting or DragonState.Exhausted
                    or DragonState.Injured or DragonState.Recovering);
            if (anyBorn)
            {
                _eggJourneyPhase = DragonEggJourneyPhase.Born;
                foreach (var d in _dragons.Values.Where(x => x.DragonLevel < 1 && x.State != DragonState.Locked))
                {
                    d.DragonLevel = Math.Max(1, d.DragonLevel);
                }

                return;
            }

            if (_dragons.Values.Any(d => d.State == DragonState.Hatching))
            {
                _eggJourneyPhase = DragonEggJourneyPhase.Incubating;
                return;
            }

            if (_dragons.Values.Any(d => d.State == DragonState.Egg))
            {
                _eggJourneyPhase = DragonEggJourneyPhase.EggOwned;
            }
        }

        private int ResolveCombatPower(DragonInstance dragon)
        {
            if (!DragonCatalog.TryGet(dragon.DefinitionId, out var definition))
            {
                return 0;
            }

            return Combat.ResolveSupportPower(dragon, definition);
        }

        private void Raise(string dragonId, DragonState previous, DragonState current) =>
            Changed?.Invoke(this, new DragonChangedEvent(dragonId, previous, current));

        /// <summary>Notifica ouvintes (ninho/UI) após mutação direta em QA/debug.</summary>
        public void NotifyChanged(string? dragonId = null)
        {
            if (!string.IsNullOrEmpty(dragonId) && TryGet(dragonId, out var one))
            {
                Raise(one.InstanceId, one.State, one.State);
                return;
            }

            foreach (var pair in _dragons)
            {
                Raise(pair.Key, pair.Value.State, pair.Value.State);
            }
        }

        public static DragonService Create(
            IDragonResourceWallet? wallet = null,
            Action? persistWallet = null,
            IDragonRepository? repository = null,
            Func<DateTime>? utcNow = null)
        {
            var settings = new DragonSettings();
            ApplyQaTimingOverrides(settings);
            var service = new DragonService(
                settings,
                repository ?? new DragonRepository(
                    settings.PersistenceKey,
                    settings.LegacyPersistenceKey,
                    settings.LegacyV5PersistenceKey,
                    settings.LegacyV4PersistenceKey),
                utcNow);
            service.BindWallet(wallet, persistWallet);
            service.LoadOrInitialize();
            return service;
        }

        private static void ApplyQaTimingOverrides(DragonSettings settings)
        {
            var cityQa = Valgor.Core.CityProgressionQa.IsActive;
            var dragonE2E = Valgor.Core.DragonPhase2Qa.IsE2ETest ||
                            Valgor.Core.DragonPhase2Qa.IsActive;
            if (!cityQa && !dragonE2E)
            {
                return;
            }

            // Timers curtos para E2E jogável (ainda conclui por tempo real).
            settings.LevelUpDurationHours = 2.0 / 3600.0;
            settings.RitualDurationHours = 3.0 / 3600.0;
            settings.HatchDurationHours = 2.5 / 3600.0;
            settings.CareExtendsHatchHours = 0;

            if (Valgor.Core.DragonPhase2Qa.IsE2ETest)
            {
                settings.PersistenceKey = Valgor.Core.DragonPhase2Qa.PersistenceKey;
            }
        }

        public string DescribeDragonProgression(string dragonId)
        {
            if (!TryGet(dragonId, out var dragon) || dragon.DragonLevel < 1)
            {
                return DescribeEggJourney();
            }

            var max = GetMaxAllowedDragonLevel();
            var next = dragon.DragonLevel + 1;
            var xpNeed = DragonProgressionRules.ExperienceRequiredForLevel(dragon.DragonLevel);
            var stage = DragonProgressionRules.StageDisplayName(dragon.GrowthStage);
            if (dragon.IsLevelingUp)
            {
                var ritual = DragonProgressionRules.IsRitualTarget(dragon.PendingLevel);
                var label = ritual
                    ? DragonProgressionRules.RitualName(dragon.PendingLevel)
                    : "Evolução";
                var rem = dragon.LevelUpEndsAtUtc.HasValue
                    ? Math.Max(0, (dragon.LevelUpEndsAtUtc.Value - _utcNow()).TotalMinutes)
                    : 0;
                return $"{label} → Nv.{dragon.PendingLevel} · restante ~{rem:0} min. · visual {stage}";
            }

            if (dragon.DragonLevel >= DragonProgressionRules.AbsoluteMaxLevel)
            {
                return $"Nv.{dragon.DragonLevel} máximo · estágio {stage} · vínculo Nv.{dragon.BondLevel}";
            }

            var gate = next > max
                ? $" Bloqueado no limite Nv.{max} (Castelo {_syncedCastleLevel}/Torre {_syncedTowerLevel})."
                : string.Empty;
            var ritualHint = DragonProgressionRules.IsRitualTarget(next)
                ? $" Próximo: {DragonProgressionRules.RitualName(next)}."
                : string.Empty;
            return
                $"Nv.{dragon.DragonLevel}/{max} · XP {dragon.Experience}/{xpNeed} · " +
                $"energia {dragon.Energy}/{_settings.MaxEnergy} · saúde {dragon.Health}/{_settings.MaxHealth} · " +
                $"vínculo Nv.{dragon.BondLevel} · {stage}.{ritualHint}{gate}";
        }

        public bool TryStartLevelUp(string dragonId, out string error)
        {
            if (_wallet == null)
            {
                error = "Carteira indisponível.";
                return false;
            }

            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            var previous = dragon.State;
            if (!Progression.TryStartLevelUp(dragon, GetMaxAllowedDragonLevel(), _wallet, out error))
            {
                return false;
            }

            _persistWallet?.Invoke();
            Persist();
            // Sempre notifica: visual/timer do ritual precisam refrescar sem troca antecipada.
            Raise(dragonId, previous, dragon.State);
            return true;
        }

        public bool TryInstantCompleteLevelUp(string dragonId, out string error)
        {
            if (_wallet == null)
            {
                error = "Carteira indisponível.";
                return false;
            }

            if (!TryGet(dragonId, out var dragon))
            {
                error = "Dragão não encontrado.";
                return false;
            }

            var previous = dragon.State;
            var previousLevel = dragon.DragonLevel;
            if (!Progression.TryInstantComplete(dragon, _wallet, out error))
            {
                return false;
            }

            _persistWallet?.Invoke();
            Persist();
            if (previousLevel != dragon.DragonLevel)
            {
                Raise(dragonId, previous, dragon.State);
            }

            return true;
        }
    }
}
