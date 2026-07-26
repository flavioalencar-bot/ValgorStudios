using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
using Valgor.City.Core;
using Valgor.Core.Modules;
using Valgor.WorldMap.Camera;
using Valgor.WorldMap.Core;
using Valgor.WorldMap.Data;
using Valgor.WorldMap.Nodes;
using Valgor.WorldMap.UI;

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
                Exit();
            }
        }

        public void Enter() => IsLoaded = true;

        public void Exit() => IsLoaded = false;

        private static WorldMapSession ResolveSession()
        {
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<WorldMapSession>(out var existing))
            {
                return existing;
            }

            IHeroesGateway heroes = GameBootstrap.Services != null &&
                                    GameBootstrap.Services.TryGet<IHeroesGateway>(out var gateway)
                ? gateway
                : new ProvisionalHeroesGateway();

            var session = WorldMapSession.Create(heroes);
            if (GameBootstrap.Services != null && GameBootstrap.Services.TryGet<CityEconomy>(out var economy))
            {
                session.BindWallet(economy.Wallet);
            }

            GameBootstrap.Services?.Register(session);
            return session;
        }

        private void CreateTerrain()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "WorldTerrain";
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<Renderer>().material.color = new Color(0.12f, 0.2f, 0.14f);
        }

        private void CreateRegions()
        {
            var root = new GameObject("Regions").transform;
            foreach (var pair in WorldMapCatalog.All)
            {
                var definition = pair.Value;
                var instance = new RegionInstance(definition.Id, definition.DefaultStatus);
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.transform.SetParent(root);
                marker.transform.position = new Vector3(definition.X, 0.15f, definition.Z);
                marker.transform.localScale = new Vector3(5.5f, 0.12f, 5.5f);
                var view = marker.AddComponent<RegionNodeView>();
                view.Initialize(instance, definition);
                Controller.AddRegion(instance, definition, view);
            }
        }

        private void CreateNodes()
        {
            var root = new GameObject("WorldNodes").transform;
            foreach (var pair in Session.Nodes)
            {
                var instance = pair.Value;
                var definition = Session.GetDefinition(instance.DefinitionId);
                var primitive = GameObject.CreatePrimitive(PrimitiveFor(definition.Kind));
                primitive.transform.SetParent(root);
                primitive.transform.position = new Vector3(definition.X, 0.7f, definition.Z);
                primitive.transform.localScale = ScaleFor(definition.Kind);
                var view = primitive.AddComponent<WorldNodeView>();
                view.Initialize(instance, definition);
                Controller.AddNode(instance, definition, view);
            }
        }

        private void CreateHud()
        {
            var hud = GetComponent<WorldMapHudController>() ?? gameObject.AddComponent<WorldMapHudController>();
            ResourceWalletResolver.TryResolve(out var economy);
            if (economy != null)
            {
                Session.BindWallet(economy.Wallet);
            }

            hud.Initialize(Controller, economy);
        }

        private static void ConfigureCamera()
        {
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                return;
            }

            if (camera.GetComponent<WorldMapCameraController>() == null)
            {
                camera.gameObject.AddComponent<WorldMapCameraController>();
            }
        }

        private static PrimitiveType PrimitiveFor(WorldNodeKind kind) => kind switch
        {
            WorldNodeKind.City => PrimitiveType.Cube,
            WorldNodeKind.Village => PrimitiveType.Capsule,
            WorldNodeKind.Resource => PrimitiveType.Cylinder,
            WorldNodeKind.Creature => PrimitiveType.Sphere,
            WorldNodeKind.Dragon => PrimitiveType.Cube,
            WorldNodeKind.Landmark => PrimitiveType.Cylinder,
            _ => PrimitiveType.Cube
        };

        private static Vector3 ScaleFor(WorldNodeKind kind) => kind switch
        {
            WorldNodeKind.City => new Vector3(2.4f, 2.2f, 2.4f),
            WorldNodeKind.Dragon => new Vector3(2.6f, 1.8f, 2.6f),
            WorldNodeKind.Landmark => new Vector3(1.4f, 2.4f, 1.4f),
            _ => new Vector3(1.8f, 1.2f, 1.8f)
        };
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
