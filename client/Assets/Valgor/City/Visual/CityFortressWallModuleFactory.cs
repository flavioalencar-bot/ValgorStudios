using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Fábrica do kit modular da muralha (Sprint 1).
    /// Peças compostas com material do Portão — não são “cubos provisórios” soltos.
    /// </summary>
    public static class CityFortressWallModuleFactory
    {
        public static GameObject Create(string moduleId, Transform parent)
        {
            return moduleId switch
            {
                "WallStraight4m" => Straight(parent, "WallStraight4m", 4f),
                "WallStraight8m" => Straight(parent, "WallStraight8m", 8f),
                "WallCorner45" => Corner(parent, "WallCorner45", 45f),
                "WallCorner90" => Corner(parent, "WallCorner90", 90f),
                "WallTowerSmall" => Tower(parent, "WallTowerSmall", 1.35f, CityFortressWallConfig.WallHeight + 1.1f),
                "WallTowerMedium" => Tower(parent, "WallTowerMedium", 1.65f, CityFortressWallConfig.WallHeight + 1.85f),
                "WallTowerLarge" => Tower(parent, "WallTowerLarge", 2.05f, CityFortressWallConfig.WallHeight + 2.7f),
                "WallWalk" => WalkOnly(parent, "WallWalk", 4f),
                "WallStairs" => Stairs(parent, "WallStairs"),
                "WallEnd" => End(parent, "WallEnd"),
                "WallBastion" => Bastion(parent, "WallBastion"),
                "WallGate" => End(parent, "WallGate_Connector"), // Gate HF é BuildingId.wall; aqui só conector
                _ => Straight(parent, "WallStraight4m_Fallback", 4f)
            };
        }

        private static GameObject Straight(Transform parent, string name, float length)
        {
            var root = Host(parent, name);
            var h = CityFortressWallConfig.WallHeight;
            var t = CityFortressWallConfig.WallThickness;

            // Corpo principal + reforço externo (leitura de muralha, não cubo solto).
            Part(root, PrimitiveType.Cube, "Body", new Vector3(0f, h * 0.5f, 0f), new Vector3(length, h, t), stone: true);
            Part(root, PrimitiveType.Cube, "ButtressL", new Vector3(-length * 0.35f, h * 0.35f, t * 0.45f),
                new Vector3(0.55f, h * 0.7f, 0.55f), stone: true);
            Part(root, PrimitiveType.Cube, "ButtressR", new Vector3(length * 0.35f, h * 0.35f, t * 0.45f),
                new Vector3(0.55f, h * 0.7f, 0.55f), stone: true);

            // Fundação irregular.
            Part(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, -CityFortressWallConfig.FoundationDepth * 0.35f, 0.05f),
                new Vector3(length + 0.35f, CityFortressWallConfig.FoundationDepth, t + 0.55f), stone: true);
            Part(root, PrimitiveType.Cube, "FoundationRockA", new Vector3(-length * 0.25f, 0.12f, t * 0.7f),
                new Vector3(0.7f, 0.35f, 0.55f), stone: true);
            Part(root, PrimitiveType.Cube, "FoundationRockB", new Vector3(length * 0.2f, 0.1f, t * 0.65f),
                new Vector3(0.55f, 0.28f, 0.45f), stone: true);

            // Caminho de ronda + ameias.
            Part(root, PrimitiveType.Cube, "Walk", new Vector3(0f, h + 0.08f, 0f),
                new Vector3(length * 0.98f, 0.14f, CityFortressWallConfig.WalkWidth), walk: true);
            var merlonCount = Mathf.Max(2, Mathf.RoundToInt(length / 1.8f));
            for (var i = 0; i < merlonCount; i++)
            {
                var x = -length * 0.5f + (i + 0.5f) * (length / merlonCount);
                Part(root, PrimitiveType.Cube, $"Merlon_{i}",
                    new Vector3(x, h + CityFortressWallConfig.MerlonHeight * 0.55f, -t * 0.35f),
                    new Vector3(0.55f, CityFortressWallConfig.MerlonHeight, 0.45f), stone: true);
            }

            Part(root, PrimitiveType.Cube, "ParapetInner",
                new Vector3(0f, h + 0.35f, t * 0.35f),
                new Vector3(length * 0.96f, 0.45f, 0.22f), stone: true);

            return root.gameObject;
        }

        private static GameObject Corner(Transform parent, string name, float degrees)
        {
            var root = Host(parent, name);
            var h = CityFortressWallConfig.WallHeight;
            var t = CityFortressWallConfig.WallThickness;
            Part(root, PrimitiveType.Cube, "ArmA", new Vector3(1.4f, h * 0.5f, 0f),
                new Vector3(3.2f, h, t), stone: true);
            var armB = PartGo(root, PrimitiveType.Cube, "ArmB", new Vector3(0f, h * 0.5f, 1.4f),
                new Vector3(t, h, 3.2f), stone: true);
            armB.transform.localRotation = Quaternion.Euler(0f, degrees - 90f, 0f);
            Part(root, PrimitiveType.Cube, "Keep", new Vector3(0f, h * 0.55f, 0f),
                new Vector3(2.0f, h * 1.1f, 2.0f), stone: true);
            Part(root, PrimitiveType.Cube, "Walk", new Vector3(0f, h + 0.08f, 0f),
                new Vector3(2.4f, 0.14f, 2.4f), walk: true);
            Part(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, -0.15f, 0f),
                new Vector3(3.8f, 0.5f, 3.8f), stone: true);
            return root.gameObject;
        }

        private static GameObject Tower(Transform parent, string name, float radius, float height)
        {
            var root = Host(parent, name);
            Part(root, PrimitiveType.Cylinder, "Shaft", new Vector3(0f, height * 0.5f, 0f),
                new Vector3(radius * 2f, height, radius * 2f), stone: true);
            Part(root, PrimitiveType.Cylinder, "Foundation", new Vector3(0f, 0.18f, 0f),
                new Vector3(radius * 2.35f, 0.5f, radius * 2.35f), stone: true);
            Part(root, PrimitiveType.Cube, "Walk", new Vector3(0f, height + 0.05f, 0f),
                new Vector3(radius * 2.15f, 0.14f, radius * 2.15f), walk: true);
            for (var i = 0; i < 4; i++)
            {
                var ang = i * 90f * Mathf.Deg2Rad;
                Part(root, PrimitiveType.Cube, $"Merlon_{i}",
                    new Vector3(Mathf.Sin(ang) * radius * 0.9f, height + 0.4f, Mathf.Cos(ang) * radius * 0.9f),
                    new Vector3(0.4f, 0.65f, 0.4f), stone: true);
            }

            Part(root, PrimitiveType.Cylinder, "FlagPole", new Vector3(0f, height + 1.0f, 0f),
                new Vector3(0.08f, 1.0f, 0.08f), wood: true);
            Part(root, PrimitiveType.Cube, "FlagCloth", new Vector3(0.4f, height + 1.4f, 0f),
                new Vector3(0.8f, 0.5f, 0.05f), banner: true);
            return root.gameObject;
        }

        private static GameObject WalkOnly(Transform parent, string name, float length)
        {
            var root = Host(parent, name);
            Part(root, PrimitiveType.Cube, "Deck", new Vector3(0f, 0.07f, 0f),
                new Vector3(length, 0.14f, CityFortressWallConfig.WalkWidth), walk: true);
            Part(root, PrimitiveType.Cube, "RailOut", new Vector3(0f, 0.4f, -CityFortressWallConfig.WalkWidth * 0.4f),
                new Vector3(length * 0.98f, 0.45f, 0.16f), stone: true);
            return root.gameObject;
        }

        private static GameObject Stairs(Transform parent, string name)
        {
            var root = Host(parent, name);
            for (var i = 0; i < 7; i++)
            {
                Part(root, PrimitiveType.Cube, $"Step_{i}",
                    new Vector3(0f, 0.18f + i * 0.32f, -i * 0.45f),
                    new Vector3(1.6f, 0.28f, 0.55f), stone: true);
            }

            Part(root, PrimitiveType.Cube, "RailL", new Vector3(-0.95f, 1.3f, -1.4f),
                new Vector3(0.18f, 2.2f, 3.2f), stone: true);
            Part(root, PrimitiveType.Cube, "RailR", new Vector3(0.95f, 1.3f, -1.4f),
                new Vector3(0.18f, 2.2f, 3.2f), stone: true);
            return root.gameObject;
        }

        private static GameObject End(Transform parent, string name)
        {
            var root = Host(parent, name);
            var h = CityFortressWallConfig.WallHeight;
            var t = CityFortressWallConfig.WallThickness;
            // Asa de conexão ao Portão HF.
            Part(root, PrimitiveType.Cube, "Wing", new Vector3(0f, h * 0.5f, 0f),
                new Vector3(3.4f, h * 1.05f, t + 0.25f), stone: true);
            Part(root, PrimitiveType.Cube, "WingCap", new Vector3(0f, h + 0.15f, 0f),
                new Vector3(3.5f, 0.35f, t + 0.4f), stone: true);
            Part(root, PrimitiveType.Cube, "Walk", new Vector3(0f, h + 0.08f, 0f),
                new Vector3(3.2f, 0.14f, CityFortressWallConfig.WalkWidth), walk: true);
            Part(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, -0.15f, 0.15f),
                new Vector3(3.7f, 0.55f, t + 0.7f), stone: true);
            Part(root, PrimitiveType.Cube, "ArchTrim", new Vector3(1.5f, h * 0.65f, 0f),
                new Vector3(0.35f, h * 0.9f, t + 0.5f), stone: true);
            return root.gameObject;
        }

        private static GameObject Bastion(Transform parent, string name)
        {
            var root = Host(parent, name);
            var h = CityFortressWallConfig.WallHeight;
            Part(root, PrimitiveType.Cube, "Mass", new Vector3(0f, h * 0.45f, 1.1f),
                new Vector3(4.2f, h * 0.9f, 3.0f), stone: true);
            Part(root, PrimitiveType.Cube, "Walk", new Vector3(0f, h + 0.05f, 1.0f),
                new Vector3(4.0f, 0.14f, 2.8f), walk: true);
            for (var i = -1; i <= 1; i++)
            {
                Part(root, PrimitiveType.Cube, $"Merlon_{i}",
                    new Vector3(i * 1.2f, h + 0.45f, 2.2f),
                    new Vector3(0.55f, 0.7f, 0.45f), stone: true);
            }

            Part(root, PrimitiveType.Cube, "Foundation", new Vector3(0f, 0.1f, 1.3f),
                new Vector3(4.6f, 0.5f, 3.4f), stone: true);
            return root.gameObject;
        }

        private static Transform Host(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Part(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 pos,
            Vector3 scale,
            bool stone = false,
            bool walk = false,
            bool wood = false,
            bool banner = false,
            bool fire = false)
        {
            PartGo(parent, type, name, pos, scale, stone, walk, wood, banner, fire);
        }

        private static GameObject PartGo(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 pos,
            Vector3 scale,
            bool stone = false,
            bool walk = false,
            bool wood = false,
            bool banner = false,
            bool fire = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            var renderer = go.GetComponent<Renderer>();
            if (stone)
            {
                CityFortressWallMaterials.ApplyStone(renderer);
            }
            else if (walk)
            {
                renderer.sharedMaterial = CityFortressWallMaterials.Walk;
            }
            else if (wood)
            {
                renderer.sharedMaterial = CityFortressWallMaterials.Wood;
            }
            else if (banner)
            {
                renderer.sharedMaterial = CityFortressWallMaterials.Banner;
            }
            else if (fire)
            {
                renderer.sharedMaterial = CityFortressWallMaterials.Fire;
            }

            return go;
        }
    }
}
