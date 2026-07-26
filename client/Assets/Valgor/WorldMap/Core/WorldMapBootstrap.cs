using UnityEngine;
using UnityEngine.UIElements;
using Valgor.Bootstrap;
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
        public WorldMapController Controller { get; private set; } = null!;

        private void Awake()
        {
            Controller = new WorldMapController(new RegionSelectionService());
            GameBootstrap.Services?.Register<IWorldMapModule>(this);
            CreateTerrain();
            CreateRegions();
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
                var node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                node.transform.SetParent(root);
                node.transform.position = new Vector3(definition.X, 0.6f, definition.Z);
                node.transform.localScale = new Vector3(2.2f, 0.6f, 2.2f);
                var view = node.AddComponent<RegionNodeView>();
                view.Initialize(instance, definition);
                Controller.Add(instance, definition, view);
            }
        }

        private void CreateHud()
        {
            var hud = GetComponent<WorldMapHudController>() ?? gameObject.AddComponent<WorldMapHudController>();
            hud.Initialize(Controller);
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
    }
}
