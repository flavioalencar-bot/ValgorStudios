using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Densidade urbana não-lógica (casas, barracas, muros internos) — não cria edifícios de gameplay.</summary>
    public static class CityUrbanDensityBuilder
    {
        public static void Build(Transform decorations)
        {
            if (decorations == null || !CityCapitalFinalConfig.UrbanDensityEnabled)
            {
                return;
            }

            // Art Pass 3 — habitação vernacular assume a densidade; evita cubos coloridos.
            if (CityArtPass3Config.Enabled && CityArtPass3Config.VernacularHousingEnabled)
            {
                var lean = new GameObject("UrbanDensity_Lean").transform;
                lean.SetParent(decorations, false);
                var wall = CityVisualMaterials.StoneDark;
                var garden = CityVisualMaterials.Vegetation;
                Prop(lean, "GardenWest", new Vector3(-7.5f, 0.05f, 2.5f), new Vector3(2.2f, 0.1f, 1.6f), garden);
                Prop(lean, "GardenEast", new Vector3(8.2f, 0.05f, -1.8f), new Vector3(2.0f, 0.1f, 1.4f), garden);
                Prop(lean, "InnerWallN", new Vector3(0f, 0.4f, 6.5f), new Vector3(8f, 0.8f, 0.25f), wall);
                Prop(lean, "LampPostA", new Vector3(-2.2f, 0.9f, -4.5f), new Vector3(0.15f, 1.8f, 0.15f), wall);
                Prop(lean, "LampPostB", new Vector3(3.1f, 0.9f, -5.2f), new Vector3(0.15f, 1.8f, 0.15f), wall);
                return;
            }

            var root = new GameObject("UrbanDensity").transform;
            root.SetParent(decorations, false);

            var cottage = Color.Lerp(CityVisualMaterials.Wood, CityVisualMaterials.StoneLight, 0.35f);
            var stall = Color.Lerp(CityVisualMaterials.Wood, new Color(0.55f, 0.22f, 0.18f), 0.35f);
            var wallLegacy = CityVisualMaterials.StoneDark;
            var gardenLegacy = CityVisualMaterials.Vegetation;

            PlaceRing(root, "CottageRing", cottage, radius: 11.5f, count: 10, scale: new Vector3(1.1f, 0.9f, 1.1f));
            PlaceRing(root, "WorkshopRing", stall, radius: 14.2f, count: 8, scale: new Vector3(0.9f, 0.7f, 0.9f));
            PlaceRing(root, "TowerHint", wallLegacy, radius: 16.8f, count: 6, scale: new Vector3(0.55f, 1.6f, 0.55f));

            Prop(root, "GardenWest", new Vector3(-7.5f, 0.05f, 2.5f), new Vector3(2.2f, 0.1f, 1.6f), gardenLegacy);
            Prop(root, "GardenEast", new Vector3(8.2f, 0.05f, -1.8f), new Vector3(2.0f, 0.1f, 1.4f), gardenLegacy);
            Prop(root, "InnerWallN", new Vector3(0f, 0.4f, 6.5f), new Vector3(8f, 0.8f, 0.25f), wallLegacy);
            Prop(root, "CartA", new Vector3(4.5f, 0.25f, -8.5f), new Vector3(1.2f, 0.5f, 0.7f), cottage);
            Prop(root, "CartB", new Vector3(-5.2f, 0.25f, -7.8f), new Vector3(1.0f, 0.45f, 0.65f), stall);
            Prop(root, "LampPostA", new Vector3(-2.2f, 0.9f, -4.5f), new Vector3(0.15f, 1.8f, 0.15f), wallLegacy);
            Prop(root, "LampPostB", new Vector3(3.1f, 0.9f, -5.2f), new Vector3(0.15f, 1.8f, 0.15f), wallLegacy);
        }

        private static void PlaceRing(
            Transform root,
            string prefix,
            Color color,
            float radius,
            int count,
            Vector3 scale)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = (i / (float)count) * Mathf.PI * 2f + 0.17f;
                var pos = new Vector3(Mathf.Cos(angle) * radius, scale.y * 0.5f, Mathf.Sin(angle) * radius);
                if (pos.z < -15f && Mathf.Abs(pos.x) < 3.5f)
                {
                    continue;
                }

                Prop(root, $"{prefix}_{i}", pos, scale, color);
            }
        }

        private static void Prop(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.layer = 2;
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }
    }
}
