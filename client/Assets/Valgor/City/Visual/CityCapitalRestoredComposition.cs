using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Base ambiental mínima pós-remoção do World Foundation quebrado.
    /// Não instancia catálogo Tripo; não cria geometria de teste.
    /// Referência estável: commit 8cd4d78 (pai de 4d49b24).
    /// </summary>
    public static class CityCapitalRestoredComposition
    {
        public const string RootName = "CapitalRestoredEnvironment";

        public static void Build(Transform terrainParent, Transform waterParent)
        {
            CityCapitalWorldNoTripo.NeverCallTripoApi("restored-composition");

            BuildTerrainBase(terrainParent);
            BuildDistantBackdrop(terrainParent);
            BuildRiverBase(waterParent);
            ApplyAtmosphere();

            Debug.Log(
                $"[Valgor.City] CapitalRestoredComposition OK " +
                $"(tripoCalls={CityCapitalWorldNoTripo.TripoApiInvocationCount}).");
        }

        /// <summary>Solo contínuo — sem círculos / pads de demonstração.</summary>
        private static void BuildTerrainBase(Transform parent)
        {
            var root = new GameObject("TerrainBase").transform;
            root.SetParent(parent, false);

            // Placa única grande (bordas fora do overview ~16–17).
            SpawnFlat(root, "GrassFar", Vector3.zero, new Vector3(90f, 0.06f, 90f),
                CityVisualMaterials.Vegetation, -0.06f);
            SpawnFlat(root, "GrassMid", Vector3.zero, new Vector3(52f, 0.07f, 52f),
                Color.Lerp(CityVisualMaterials.Vegetation, CityVisualMaterials.Dirt, 0.14f), -0.03f);
        }

        private static void SpawnFlat(
            Transform parent,
            string name,
            Vector3 pos,
            Vector3 scale,
            Color color,
            float y)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(pos.x, y, pos.z);
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }

        /// <summary>2–3 silhuetas atrás da cidade — procedural, sem catálogo.</summary>
        private static void BuildDistantBackdrop(Transform parent)
        {
            var root = new GameObject("DistantBackdrop").transform;
            root.SetParent(parent, false);

            SpawnSilhouette(root, "Backdrop_N", new Vector3(0f, 1.2f, 34f), new Vector3(32f, 6f, 10f), 6f);
            SpawnSilhouette(root, "Backdrop_NW", new Vector3(-22f, 0.9f, 31f), new Vector3(18f, 4.5f, 8f), 22f);
            SpawnSilhouette(root, "Backdrop_NE", new Vector3(22f, 0.9f, 30f), new Vector3(18f, 4.5f, 8f), -18f);
        }

        private static void SpawnSilhouette(
            Transform parent,
            string name,
            Vector3 pos,
            Vector3 scale,
            float yaw)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var rend = go.GetComponent<Renderer>();
            // Escuro + matte — some no fog; sem sombra.
            CityVisualMaterials.Apply(
                rend,
                Color.Lerp(CityVisualMaterials.StoneDark, new Color(0.22f, 0.26f, 0.24f), 0.55f));
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }

        /// <summary>
        /// Rio estável simplificado: dois canais laterais contínuos (sem bends / sem World Foundation).
        /// </summary>
        private static void BuildRiverBase(Transform waterParent)
        {
            var root = new GameObject("RiverBase").transform;
            root.SetParent(waterParent, false);

            var water = new Color(0.2f, 0.34f, 0.42f);
            var bank = Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.4f);

            // Fora do overview ~13.5 (half-width ~24 @16:9) — presente no mundo, sem dominante na tela.
            SpawnFlat(root, "RiverWest", new Vector3(-40f, 0f, 0f), new Vector3(2.2f, 0.1f, 24f), water, -0.1f);
            SpawnFlat(root, "RiverEast", new Vector3(40f, 0f, 0f), new Vector3(2.2f, 0.1f, 24f), water, -0.1f);
            SpawnFlat(root, "BankWest", new Vector3(-38.8f, 0f, 0f), new Vector3(0.7f, 0.06f, 22f), bank, -0.02f);
            SpawnFlat(root, "BankEast", new Vector3(38.8f, 0f, 0f), new Vector3(0.7f, 0.06f, 22f), bank, -0.02f);
        }

        private static void ApplyAtmosphere()
        {
            var sky = new Color(0.48f, 0.64f, 0.82f);
            var cam = global::UnityEngine.Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = sky;
                cam.farClipPlane = 140f;
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.022f;
            RenderSettings.fogColor = new Color(0.55f, 0.64f, 0.74f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.64f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.48f, 0.5f, 0.44f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.22f, 0.18f);
        }
    }
}
