using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Constrói andaimes medievais próprios do Valgor (madeira, cordas, plataformas, escadas).
    /// Usado em runtime (fallback) e pelo bake de prefabs no Editor.
    /// </summary>
    public static class ConstructionScaffoldBuilder
    {
        private static Color Wood => CityVisualMaterials.Wood;
        private static Color WoodDark => Color.Lerp(CityVisualMaterials.Wood, Color.black, 0.28f);
        private static Color Rope => new(0.45f, 0.38f, 0.28f);
        private static Color Plank => Color.Lerp(CityVisualMaterials.Wood, new Color(0.55f, 0.42f, 0.25f), 0.35f);

        public static GameObject Build(ConstructionScaffoldSize size, Transform? parent = null)
        {
            var root = new GameObject(ConstructionScaffoldCatalog.PrefabAssetName(size));
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            switch (size)
            {
                case ConstructionScaffoldSize.Small:
                    BuildFrame(root.transform, width: 2.4f, depth: 2.2f, height: 2.6f, levels: 2, ladders: 1);
                    break;
                case ConstructionScaffoldSize.Medium:
                    BuildFrame(root.transform, width: 3.4f, depth: 3.0f, height: 3.6f, levels: 3, ladders: 2);
                    break;
                case ConstructionScaffoldSize.Large:
                    BuildFrame(root.transform, width: 4.2f, depth: 3.6f, height: 4.8f, levels: 4, ladders: 2);
                    break;
                case ConstructionScaffoldSize.Castle:
                    BuildFrame(root.transform, width: 5.6f, depth: 5.2f, height: 6.4f, levels: 5, ladders: 3);
                    AddCornerTowers(root.transform, 5.6f, 5.2f, 6.4f);
                    break;
                case ConstructionScaffoldSize.Wall:
                    BuildWallScaffold(root.transform);
                    break;
            }

            StripColliders(root);
            return root;
        }

        private static void BuildFrame(
            Transform root,
            float width,
            float depth,
            float height,
            int levels,
            int ladders)
        {
            var hw = width * 0.5f;
            var hd = depth * 0.5f;
            var posts = new[]
            {
                new Vector3(-hw, 0f, -hd),
                new Vector3(hw, 0f, -hd),
                new Vector3(-hw, 0f, hd),
                new Vector3(hw, 0f, hd)
            };

            foreach (var p in posts)
            {
                Beam(root, p + Vector3.up * (height * 0.5f), new Vector3(0.14f, height, 0.14f), WoodDark);
            }

            // Travessas horizontais por nível.
            for (var i = 1; i <= levels; i++)
            {
                var y = height * (i / (float)(levels + 0.35f));
                Beam(root, new Vector3(0f, y, -hd), new Vector3(width, 0.1f, 0.1f), Wood);
                Beam(root, new Vector3(0f, y, hd), new Vector3(width, 0.1f, 0.1f), Wood);
                Beam(root, new Vector3(-hw, y, 0f), new Vector3(0.1f, 0.1f, depth), Wood);
                Beam(root, new Vector3(hw, y, 0f), new Vector3(0.1f, 0.1f, depth), Wood);

                // Plataforma (tábuas).
                Beam(root, new Vector3(0f, y + 0.06f, 0f), new Vector3(width * 0.92f, 0.05f, depth * 0.92f), Plank);

                // Cordas diagonais leves.
                if (i % 2 == 0)
                {
                    RopeLine(root, new Vector3(-hw, y - 0.35f, -hd), new Vector3(hw, y + 0.1f, -hd));
                    RopeLine(root, new Vector3(hw, y - 0.35f, hd), new Vector3(-hw, y + 0.1f, hd));
                }
            }

            // Escadas.
            for (var l = 0; l < ladders; l++)
            {
                var side = l % 2 == 0 ? -1f : 1f;
                var z = Mathf.Lerp(-hd, hd, (l + 1f) / (ladders + 1f));
                BuildLadder(root, new Vector3(side * hw, 0f, z), height * 0.92f, side);
            }

            // Suportes inclinados na base.
            Beam(root, new Vector3(-hw * 0.7f, height * 0.22f, -hd * 0.7f), new Vector3(0.1f, height * 0.45f, 0.1f), Wood);
            Beam(root, new Vector3(hw * 0.7f, height * 0.22f, hd * 0.7f), new Vector3(0.1f, height * 0.45f, 0.1f), Wood);
        }

        private static void AddCornerTowers(Transform root, float width, float depth, float height)
        {
            var hw = width * 0.5f;
            var hd = depth * 0.5f;
            var corners = new[]
            {
                new Vector3(-hw, 0f, -hd),
                new Vector3(hw, 0f, -hd),
                new Vector3(-hw, 0f, hd),
                new Vector3(hw, 0f, hd)
            };
            foreach (var c in corners)
            {
                Beam(root, c + Vector3.up * (height * 0.55f), new Vector3(0.35f, height * 1.05f, 0.35f), WoodDark);
                Beam(root, c + Vector3.up * (height * 1.05f), new Vector3(0.55f, 0.12f, 0.55f), Plank);
            }
        }

        private static void BuildWallScaffold(Transform root)
        {
            // Andaime alongado (portão / muralha).
            const float width = 7.5f;
            const float depth = 1.8f;
            const float height = 3.8f;
            BuildFrame(root, width, depth, height, levels: 3, ladders: 2);
            // Plataforma frontal extra.
            Beam(root, new Vector3(0f, height * 0.55f, depth * 0.55f), new Vector3(width * 0.9f, 0.06f, 0.7f), Plank);
            Beam(root, new Vector3(-width * 0.35f, height * 0.3f, depth * 0.55f), new Vector3(0.12f, height * 0.55f, 0.12f), WoodDark);
            Beam(root, new Vector3(width * 0.35f, height * 0.3f, depth * 0.55f), new Vector3(0.12f, height * 0.55f, 0.12f), WoodDark);
        }

        private static void BuildLadder(Transform root, Vector3 basePos, float height, float sideSign)
        {
            var x = basePos.x + sideSign * 0.18f;
            var z = basePos.z;
            Beam(root, new Vector3(x, height * 0.5f, z - 0.18f), new Vector3(0.08f, height, 0.08f), WoodDark);
            Beam(root, new Vector3(x, height * 0.5f, z + 0.18f), new Vector3(0.08f, height, 0.08f), WoodDark);
            var steps = Mathf.Max(4, Mathf.RoundToInt(height / 0.45f));
            for (var i = 1; i < steps; i++)
            {
                var y = height * (i / (float)steps);
                Beam(root, new Vector3(x, y, z), new Vector3(0.08f, 0.06f, 0.42f), Plank);
            }
        }

        private static void RopeLine(Transform root, Vector3 a, Vector3 b)
        {
            var mid = (a + b) * 0.5f;
            var dir = b - a;
            var len = dir.magnitude;
            if (len < 0.05f)
            {
                return;
            }

            var go = Beam(root, mid, new Vector3(0.035f, len, 0.035f), Rope);
            go.transform.up = dir.normalized;
        }

        private static GameObject Beam(Transform root, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Beam";
            go.transform.SetParent(root, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(col);
                }
                else
                {
                    Object.DestroyImmediate(col);
                }
            }

            CityVisualMaterials.ApplySurface(go.GetComponent<Renderer>(), color, SurfaceKind.Wood);
            return go;
        }

        private static void StripColliders(GameObject root)
        {
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(col);
                }
                else
                {
                    Object.DestroyImmediate(col);
                }
            }
        }
    }
}
