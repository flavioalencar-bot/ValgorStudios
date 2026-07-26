using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Buildings;
using Valgor.City.Core;
using Valgor.City.Data;
using Valgor.City.UI;
using Valgor.Core.Modules;

namespace Valgor.City
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CityBootstrap : MonoBehaviour, IPlayerCityModule
    {
        private static readonly (string Id, BuildingState State)[] BuildingLayout =
        {
            ("castle", BuildingState.Ready),
            ("farm", BuildingState.Available),
            ("lumbermill", BuildingState.Ready),
            ("quarry", BuildingState.Available),
            ("mine", BuildingState.Locked),
            ("warehouse", BuildingState.Ready),
            ("academy", BuildingState.Available),
            ("institute", BuildingState.Locked),
            ("hospital", BuildingState.Available),
            ("market", BuildingState.Ready),
            ("temple", BuildingState.Locked),
            ("dragon-tower", BuildingState.Locked),
            ("arena", BuildingState.Available),
            ("laboratory", BuildingState.Locked)
        };

        public bool IsLoaded { get; private set; }
        public ResourceWallet Wallet { get; private set; } = null!;
        public CityController Controller { get; private set; } = null!;

        private void Awake()
        {
            Wallet = CreateWallet();
            Controller = new CityController(Wallet, new BuildingSelectionService());
            GameBootstrap.Services?.Register<IPlayerCityModule>(this);
            GameBootstrap.Services?.Register<IResourceModule>(new CityResourceModule(Wallet));
            CreateBuildings();
            CreateHud();
            ConfigureCamera();
        }

        private void OnEnable() => Enter();

        private void OnDisable()
        {
            if (IsLoaded)
            {
                Exit();
            }
        }

        public void Enter() => IsLoaded = true;
        public void Exit() => IsLoaded = false;

        private static ResourceWallet CreateWallet()
        {
            var wallet = new ResourceWallet();
            wallet.Add(ResourceType.Gold, 5000);
            wallet.Add(ResourceType.Food, 3000);
            wallet.Add(ResourceType.Wood, 3000);
            wallet.Add(ResourceType.Stone, 2000);
            wallet.Add(ResourceType.Iron, 1000);
            wallet.Add(ResourceType.DragonEssence, 100);
            wallet.Add(ResourceType.Diamonds, 50);
            return wallet;
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
                var level = layout.Id == "castle" ? 1 : 0;
                var instance = new BuildingInstance(layout.Id, level, layout.State);
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
                primitive.transform.localScale = layout.Id == "castle" ? new Vector3(2.3f, 3.5f, 2.3f) : new Vector3(2.3f, 1.5f, 2.3f);
                var view = primitive.AddComponent<BuildingView>();
                view.Initialize(instance, definition);
                Controller.Add(slot, instance, definition, view);
            }
        }

        private void CreateHud()
        {
            var hud = GetComponent<CityHudController>() ?? gameObject.AddComponent<CityHudController>();
            hud.Initialize(Wallet, Controller);
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
