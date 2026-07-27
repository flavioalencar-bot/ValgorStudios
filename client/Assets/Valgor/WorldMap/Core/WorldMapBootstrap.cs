using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.Core.Modules;
using Valgor.Dragons.Core;
using Valgor.WorldMap.Camera;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Nodes;
using Valgor.WorldMap.Simulation;
using Valgor.WorldMap.Territory;
using Valgor.WorldMap.UI;
using Valgor.WorldMap.Visual;

namespace Valgor.WorldMap
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class WorldMapBootstrap : MonoBehaviour, IWorldMapModule
    {
        public bool IsLoaded { get; private set; }
        public WorldMapSession Session { get; private set; } = null!;
        public WorldMapController Controller { get; private set; } = null!;

        private void Awake()
        {
            Session = ResolveSession();
            Session.LoadOrInitialize();
            Controller = new WorldMapController(Session);
            GameBootstrap.Services?.Register<IWorldMapModule>(this);
            CreateTerrain();
            CreateRegions();
            CreateNodes();
            CreateMarchArmy();
            CreateHud();
            ConfigureCamera();
            Controller.ApplyNodeVisibility();
            Controller.RestoreSelectionVisuals();
        }

        private void Update() => Controller.Tick();

        private void OnEnable() => Enter();

        private void OnDisable()
        {
            if (IsLoaded)
            {
                Controller.Persist();
                Exit();
            }
        }

        public void Enter() => IsLoaded = true;

        public void Exit() => IsLoaded = false;

        private static WorldMapSession ResolveSession()
        {
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<WorldMapSession>(out var existing))
            {
                existing.BindDragons(ResolveDragons());
                EnsureSimulation(existing);
                return existing;
            }

            IHeroesGateway heroes = GameBootstrap.Services != null &&
                                    GameBootstrap.Services.TryGet<IHeroesGateway>(out var gateway)
                ? gateway
                : new BetaHeroesGateway();

            var clock = ResolveSimulationClock();
            var settings = WorldMapSettings.Default;
            var session = new WorldMapSession(
                settings,
                clock,
                heroes,
                new LocalWorldMapRepository(settings.PersistenceKey));

            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<CityEconomy>(out var economy))
            {
                session.BindWallet(economy.Wallet, economy.PersistWallet);
            }

            session.BindDragons(ResolveDragons());

            GameBootstrap.Services?.Register(session);
            EnsureSimulation(session);
            return session;
        }

        private static IDragonGateway ResolveDragons()
        {
            if (GameBootstrap.Services != null &&
                GameBootstrap.Services.TryGet<IDragonGateway>(out var gateway) &&
                gateway.IsReady)
            {
                return gateway;
            }

            IDragonResourceWallet? wallet = null;
            Action? persist = null;
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<CityEconomy>(out var economy))
            {
                wallet = new CityDragonResourceWallet(economy.Wallet);
                persist = economy.PersistWallet;
            }

            var dragons = DragonService.Create(wallet, persist);
            GameBootstrap.Services?.Register(dragons);
            GameBootstrap.Services?.Register<IDragonModule>(dragons);
            GameBootstrap.Services?.Register<IDragonGateway>(dragons);
            return dragons;
        }

        private static WorldSimulationClock ResolveSimulationClock()
        {
            if (GameBootstrap.Services != null &&
                GameBootstrap.Services.TryGet<WorldSimulationClock>(out var existing))
            {
                return existing;
            }

            var clock = new WorldSimulationClock();
            GameBootstrap.Services?.Register(clock);
            return clock;
        }

        private static void EnsureSimulation(WorldMapSession session)
        {
            if (GameBootstrap.Services == null)
            {
                return;
            }

            if (!GameBootstrap.Services.TryGet<WorldSimulationCoordinator>(out var coordinator))
            {
                coordinator = new WorldSimulationCoordinator();
                GameBootstrap.Services.Register(coordinator);
            }

            coordinator.Bind(session);

            if (!GameBootstrap.Services.TryGet<GlobalMarchTickService>(out var tickService))
            {
                tickService = new GlobalMarchTickService(coordinator);
                GameBootstrap.Services.Register(tickService);
            }

            GlobalMarchTickHost.EnsureHost(tickService);
        }

        private void CreateTerrain()
        {
            WorldMapEnvironmentBuilder.Build(transform);
        }

        private void CreateRegions()
        {
            var root = new GameObject("Regions").transform;
            root.SetParent(transform, false);
            foreach (var pair in WorldMapCatalog.All)
            {
                var definition = pair.Value;
                var instance = new RegionInstance(definition.Id, definition.DefaultStatus);

                // Root com escala 1 — evita TextMesh gigante/deformado.
                var regionRoot = new GameObject("Region_" + definition.Id);
                regionRoot.transform.SetParent(root, false);
                regionRoot.transform.position = new Vector3(definition.X, 0f, definition.Z);

                var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                disc.name = "Disc";
                disc.transform.SetParent(regionRoot.transform, false);
                disc.transform.localPosition = Vector3.up * 0.04f;
                disc.transform.localScale = new Vector3(2.0f, 0.035f, 2.0f);
                Destroy(disc.GetComponent<Collider>());

                var hit = regionRoot.AddComponent<BoxCollider>();
                hit.center = new Vector3(0f, 0.1f, 0f);
                hit.size = new Vector3(2.0f, 0.3f, 2.0f);

                var view = regionRoot.AddComponent<RegionNodeView>();
                view.Initialize(instance, definition);
                Controller.AddRegion(instance, definition, view);

                if (Session.Settings.TerritoryOverlayEnabled &&
                    WorldTerritoryCatalog.TryGetByRegion(definition.Id, out var territory) &&
                    Session.TryGetTerritory(territory.Id, out var runtime))
                {
                    var overlay = disc.AddComponent<WorldTerritoryOverlay>();
                    overlay.Initialize(territory.Id, runtime.State, view);
                    Controller.AddTerritoryOverlay(territory.Id, overlay);
                }
            }
        }

        private void CreateNodes()
        {
            var root = new GameObject("WorldNodes").transform;
            root.SetParent(transform, false);
            foreach (var pair in Session.Nodes)
            {
                var instance = pair.Value;
                var definition = Session.GetDefinition(instance.DefinitionId);
                var slot = new GameObject($"Node_{definition.Id}");
                slot.transform.SetParent(root, false);
                slot.transform.position = new Vector3(definition.X, 0f, definition.Z);

                var color = WorldNodeMeshFactory.ColorFor(definition.Kind, instance.Status);
                var bounds = WorldNodeMeshFactory.Build(definition.Kind, slot.transform, color);
                var box = slot.AddComponent<BoxCollider>();
                box.center = bounds.center;
                box.size = Vector3.Max(bounds.size, new Vector3(1.5f, 1.5f, 1.5f));

                var view = slot.AddComponent<WorldNodeView>();
                view.Initialize(instance, definition, labelHeight: bounds.max.y + 0.5f);
                Controller.AddNode(instance, definition, view);
            }
        }

        private void CreateMarchArmy()
        {
            var army = MarchArmyView.Create(transform);
            Controller.BindMarchArmy(army);
        }

        private void CreateHud()
        {
            var hud = GetComponent<WorldMapHudController>() ?? gameObject.AddComponent<WorldMapHudController>();
            ResourceWalletResolver.TryResolve(out var economy);
            if (economy != null)
            {
                Session.BindWallet(economy.Wallet, economy.PersistWallet);
            }

            hud.Initialize(Controller, economy);
        }

        private void ConfigureCamera()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                return;
            }

            var controller = camera.GetComponent<WorldMapCameraController>() ??
                             camera.gameObject.AddComponent<WorldMapCameraController>();
            WorldMapEnvironmentBuilder.ApplyCameraAtmosphere(camera);
            Controller.BindCamera(controller);
        }
    }

    internal static class ResourceWalletResolver
    {
        public static bool TryResolve(out CityEconomy? economy)
        {
            economy = null;
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<CityEconomy>(out var existing))
            {
                economy = existing;
                return true;
            }

            return false;
        }
    }
}
