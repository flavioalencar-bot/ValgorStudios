using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.UI;
using Valgor.Core;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;

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
            ("temple", BuildingState.Locked, 0),
            ("dragon-tower", BuildingState.Ready, 1),
            ("arena", BuildingState.Available, 0),
            ("laboratory", BuildingState.Locked, 0)
        };

        public bool IsLoaded { get; private set; }
        public CityEconomy Economy { get; private set; } = null!;
        public DragonService Dragons { get; private set; } = null!;
        public CityController Controller { get; private set; } = null!;
        public ResourceWallet Wallet => Economy.Wallet;

        private void Awake()
        {
            Economy = ResolveEconomy();
            Dragons = ResolveDragons(Economy);
            Controller = new CityController(Economy, new BuildingSelectionService());
            Controller.BindDragons(Dragons);
            GameBootstrap.Services?.Register<IPlayerCityModule>(this);
            GameBootstrap.Services?.Register<IResourceModule>(new CityResourceModule(Economy.Wallet));
            CreateBuildings();
            Economy.ApplyOfflineAndPersist(Controller.Buildings);
            CreateHud();
            ConfigureCamera();
        }

        private void Update() => Controller.Tick();

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
            if (BetaFocusHints.TryConsumeBuildingFocus(out var buildingId))
            {
                Controller.TrySelectByDefinitionId(buildingId);
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
            const int columns = 4;
            const float spacing = 7f;
            var root = new GameObject("BuildingSlots").transform;
            for (var index = 0; index < BuildingLayout.Length; index++)
            {
                var layout = BuildingLayout[index];
                var definition = BuildingCatalog.Get(layout.Id);
                var instance = new BuildingInstance(layout.Id, layout.Level, layout.State);
                var row = index / columns;
                var column = index % columns;
                var position = new Vector3((column - 1.5f) * spacing, 0.7f, (row - 1.5f) * spacing);

                var slotObject = new GameObject($"Slot_{index + 1}_{layout.Id}");
                slotObject.transform.SetParent(root);
                slotObject.transform.position = position;
                var slot = slotObject.AddComponent<BuildingSlot>();
                slot.Initialize($"slot-{index + 1}", layout.Id, instance);

                var primitive = GameObject.CreatePrimitive(index % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                primitive.transform.SetParent(slotObject.transform, false);
                primitive.transform.localScale = layout.Id == "castle"
                    ? new Vector3(2.3f, 3.5f, 2.3f)
                    : new Vector3(2.3f, 1.5f, 2.3f);
                var view = primitive.AddComponent<BuildingView>();
                view.Initialize(instance, definition);
                Controller.Add(slot, instance, definition, view);
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
            if (camera != null && camera.GetComponent<Valgor.City.Camera.CityCameraController>() == null)
            {
                camera.gameObject.AddComponent<Valgor.City.Camera.CityCameraController>();
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
