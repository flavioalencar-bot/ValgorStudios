using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.UI;
using Valgor.City.Visual;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.UI;

namespace Valgor.City
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CityBootstrap : MonoBehaviour, IPlayerCityModule
    {
        private static readonly (string Id, BuildingState State, int Level)[] BuildingLayout =
        {
            ("castle", BuildingState.Ready, 1),
            ("farm", BuildingState.Ready, 1),
            ("lumbermill", BuildingState.Ready, 1),
            ("quarry", BuildingState.Ready, 1),
            ("mine", BuildingState.Available, 0),
            ("warehouse", BuildingState.Ready, 1),
            ("academy", BuildingState.Available, 0),
            ("institute", BuildingState.Locked, 0),
            ("hospital", BuildingState.Available, 0),
            ("market", BuildingState.Ready, 1),
            ("temple", BuildingState.Available, 0),
            ("dragon-tower", BuildingState.Ready, 1),
            ("arena", BuildingState.Available, 0),
            ("laboratory", BuildingState.Available, 0),
            ("wall", BuildingState.Ready, 1)
        };

        public bool IsLoaded { get; private set; }
        public CityEconomy Economy { get; private set; } = null!;
        public DragonService Dragons { get; private set; } = null!;
        public CityController Controller { get; private set; } = null!;
        public ResourceWallet Wallet => Economy.Wallet;

        private void Awake()
        {
            Valgor.City.Qa.CityProgressionQaBootstrap.ApplyBeforeEconomy();
            Economy = ResolveEconomy();
            Dragons = ResolveDragons(Economy);
            Controller = new CityController(Economy, new BuildingSelectionService());
            Controller.BindDragons(Dragons);
            GameBootstrap.Services?.Register<IPlayerCityModule>(this);
            GameBootstrap.Services?.Register<IResourceModule>(new CityResourceModule(Economy.Wallet));
            CityEnvironmentBuilder.Build(transform);
            CreateBuildings();
            ApplyWallFortifications();
            Controller.BuildingChanged += ApplyWallFortifications;
            Controller.SyncBetaProgress();
            Economy.ApplyOfflineAndPersist(Controller.Buildings);
            if (Valgor.Core.CityProgressionQa.IsActive)
            {
                Valgor.City.Qa.CityProgressionQaBootstrap.TopUpWallet(Economy.Wallet);
                Valgor.City.Qa.CityProgressionQaBootstrap.TopUpEnergyPrefs();
                Economy.PersistWallet();
            }

            Economy.ApplyPendingMissionRewards();
            Controller.SyncCastleVisuals(animate: false);
            Controller.SyncBetaProgress();
            Controller.RefreshPresentation();
            CreateHud();
            ConfigureCamera();
            if (Valgor.Core.CityProgressionQa.IsActive)
            {
                var qa = gameObject.GetComponent<Valgor.City.Qa.CityProgressionQaController>() ??
                         gameObject.AddComponent<Valgor.City.Qa.CityProgressionQaController>();
                qa.Bind(Controller);
                var hudQa = gameObject.GetComponent<Valgor.City.Qa.CityProgressionQaHud>() ??
                            gameObject.AddComponent<Valgor.City.Qa.CityProgressionQaHud>();
                hudQa.Initialize(qa);
                if (Valgor.Core.CityProgressionQa.IsAutoTest)
                {
                    var auto = gameObject.GetComponent<Valgor.City.Qa.CityProgressionQaAutoTest>() ??
                               gameObject.AddComponent<Valgor.City.Qa.CityProgressionQaAutoTest>();
                    auto.Begin(qa, Controller);
                }
            }
        }

        private void OnDestroy()
        {
            if (Controller != null)
            {
                Controller.BuildingChanged -= ApplyWallFortifications;
            }
        }

        private BuildingView? _wallView;
        private int _lastWallVisualLevel = int.MinValue;

        private void ApplyWallFortifications()
        {
            var level = Controller != null ? Controller.GetBuildingLevel("wall") : 1;
            if (level != _lastWallVisualLevel)
            {
                _lastWallVisualLevel = level;
                CityEnvironmentBuilder.ApplyWallLevel(transform, level);
            }

            BindWallSelectionProxies();
        }

        private void BindWallSelectionProxies()
        {
            if (_wallView == null)
            {
                return;
            }

            var fort = CityEnvironmentBuilder.FindFortifications(transform);
            if (fort == null)
            {
                return;
            }

            var buildingLayer = LayerMask.NameToLayer("Building");
            var colliders = fort.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col == null)
                {
                    continue;
                }

                if (buildingLayer >= 0)
                {
                    col.gameObject.layer = buildingLayer;
                }

                var proxy = col.GetComponent<BuildingSelectionClickProxy>();
                if (proxy == null)
                {
                    proxy = col.gameObject.AddComponent<BuildingSelectionClickProxy>();
                }

                proxy.Bind(_wallView);
            }
        }

        private void Update()
        {
            Economy.ApplyPendingMissionRewards();
            Controller.Tick();
        }

        private void OnEnable() => Enter();

        private void OnDisable()
        {
            if (IsLoaded)
            {
                Controller.Persist();
                Dragons.Persist();
                Exit();
            }
        }

        public void Enter()
        {
            IsLoaded = true;
            Economy.ApplyPendingMissionRewards();
            BetaJourneyGuide.NotifyReturnedToCity();
            PlayerPrefs.Save();
            if (BetaFocusHints.TryConsumeBuildingFocus(out var buildingId))
            {
                Controller.TrySelectByDefinitionId(buildingId);
                BetaJourneyGuide.NotifyDragonTowerFocused();
            }
        }

        public void Exit() => IsLoaded = false;

        private static CityEconomy ResolveEconomy()
        {
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<CityEconomy>(out var existing))
            {
                return existing;
            }

            var economy = CityEconomy.Create();
            GameBootstrap.Services?.Register(economy);
            return economy;
        }

        private static DragonService ResolveDragons(CityEconomy economy)
        {
            var wallet = new CityDragonResourceWallet(economy.Wallet);
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<DragonService>(out var existing))
            {
                existing.BindWallet(wallet, economy.PersistWallet);
                return existing;
            }

            var dragons = DragonService.Create(wallet, economy.PersistWallet);
            GameBootstrap.Services?.Register(dragons);
            GameBootstrap.Services?.Register<IDragonModule>(dragons);
            GameBootstrap.Services?.Register<IDragonGateway>(dragons);
            return dragons;
        }

        private void CreateBuildings()
        {
            var root = new GameObject("BuildingSlots").transform;
            root.SetParent(transform, false);
            for (var index = 0; index < BuildingLayout.Length; index++)
            {
                var layout = BuildingLayout[index];
                var definition = BuildingCatalog.Get(layout.Id);
                var instance = new BuildingInstance(layout.Id, layout.Level, layout.State);
                var position = CityLayout.WorldPosition(layout.Id);

                var slotObject = new GameObject($"Slot_{index + 1}_{layout.Id}");
                slotObject.transform.SetParent(root, false);
                slotObject.transform.position = position;
                var slot = slotObject.AddComponent<BuildingSlot>();
                slot.Initialize($"slot-{index + 1}", layout.Id, instance);

                var identity = CityLayout.IdentityColor(layout.Id);
                var bounds = CityBuildingMeshFactory.Build(layout.Id, slotObject.transform, identity, layout.Level);
                var box = slotObject.AddComponent<BoxCollider>();
                box.center = bounds.center;
                var minSize = layout.Id == "wall"
                    ? new Vector3(4.5f, 3.5f, 3.0f)
                    : new Vector3(2.2f, 2.2f, 2.2f);
                box.size = Vector3.Max(bounds.size, minSize);

                var buildingLayer = LayerMask.NameToLayer("Building");
                if (buildingLayer >= 0)
                {
                    SetLayerRecursive(slotObject, buildingLayer);
                }

                var view = slotObject.AddComponent<BuildingView>();
                view.Initialize(instance, definition, labelHeight: bounds.max.y + 0.6f);
                Controller.Add(slot, instance, definition, view);

                if (layout.Id == "wall")
                {
                    _wallView = view;
                }

                if (layout.Id == "dragon-tower")
                {
                    var nest = slotObject.AddComponent<DragonNestView>();
                    nest.Bind(Dragons);
                }
            }
        }

        private void CreateHud()
        {
            var hud = GetComponent<CityHudController>() ?? gameObject.AddComponent<CityHudController>();
            hud.Initialize(Controller, Dragons);
        }

        private static void ConfigureCamera()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                return;
            }

            if (camera.GetComponent<Valgor.City.Camera.CityCameraController>() == null)
            {
                camera.gameObject.AddComponent<Valgor.City.Camera.CityCameraController>();
            }

            if (camera.GetComponent<Valgor.City.Input.CityBuildingPointerInput>() == null)
            {
                camera.gameObject.AddComponent<Valgor.City.Input.CityBuildingPointerInput>();
            }
        }

        private static void SetLayerRecursive(GameObject root, int layer)
        {
            root.layer = layer;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.layer = layer;
            }
        }

        private sealed class CityResourceModule : IResourceModule
        {
            public CityResourceModule(ResourceWallet wallet) => Wallet = wallet;
            public ResourceWallet Wallet { get; }
            public bool IsReady => true;
        }
    }
}
