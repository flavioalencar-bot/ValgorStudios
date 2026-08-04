using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Resolve asset do kit: prefab HF em Resources com encaixe por bounds;
    /// fallback procedural só se o prefab não existir ou HF for inválido (tapete plano).
    /// </summary>
    public static class CityTerrainAssetResolver
    {
        public const string ResourcesRoot = "Valgor/Capital/Environment/";

        public static GameObject Spawn(
            string assetKey,
            Transform parent,
            WorldPropPlacement placement,
            bool collidersEnabled)
        {
            var go = TryInstantiatePrefab(assetKey, parent);
            var highFidelity = go != null;
            if (go == null)
            {
                go = CityTerrainFallbackFactory.Create(assetKey, parent);
            }

            go.name = highFidelity ? assetKey : assetKey + "_Fallback";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(placement.X, placement.Y, placement.Z);
            go.transform.localRotation = Quaternion.Euler(0f, placement.YawDegrees, 0f);
            go.transform.localScale = Vector3.one;

            if (highFidelity)
            {
                if (!TryFitHighFidelity(go, placement))
                {
                    Object.Destroy(go);
                    go = CityTerrainFallbackFactory.Create(assetKey, parent);
                    go.name = assetKey + "_Fallback";
                    go.transform.SetParent(parent, false);
                    go.transform.localPosition = new Vector3(placement.X, placement.Y, placement.Z);
                    go.transform.localRotation = Quaternion.Euler(0f, placement.YawDegrees, 0f);
                    go.transform.localScale = new Vector3(placement.ScaleX, placement.ScaleY, placement.ScaleZ);
                    highFidelity = false;
                }
                else
                {
                    NormalizeTerrainMaterials(go, placement.AssetKey);
                }
            }
            else
            {
                go.transform.localScale = new Vector3(placement.ScaleX, placement.ScaleY, placement.ScaleZ);
            }

            if (!collidersEnabled)
            {
                foreach (var col in go.GetComponentsInChildren<Collider>(true))
                {
                    Object.Destroy(col);
                }
            }

            return go;
        }

        private static bool TryFitHighFidelity(GameObject go, WorldPropPlacement placement)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                go.transform.localScale = new Vector3(placement.ScaleX, placement.ScaleY, placement.ScaleZ);
                return true;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var size = bounds.size;
            var srcW = Mathf.Max(0.05f, Mathf.Max(size.x, size.z));
            var srcH = Mathf.Max(0.05f, size.y);
            var targetW = Mathf.Max(0.5f, Mathf.Max(placement.ScaleX, placement.ScaleZ));
            var targetH = Mathf.Max(0.5f, placement.ScaleY);

            var isTerrainPad = placement.AssetKey.StartsWith("Plateau_") ||
                               placement.AssetKey.StartsWith("Valley_") ||
                               placement.AssetKey.StartsWith("River") ||
                               placement.AssetKey.StartsWith("RiverBank") ||
                               placement.AssetKey is "Stream" or "Lake_A" or "Slope_A" or "Slope_B" or
                                   "Embankment_A" or "Embankment_B";

            // Heightmap/plano sem volume — rejeita montanha/penhasco HF.
            var isVolumeProp = placement.AssetKey.StartsWith("Mountain") ||
                               placement.AssetKey.StartsWith("Distant") ||
                               placement.AssetKey.StartsWith("Cliff");
            if (isVolumeProp && size.y < Mathf.Max(size.x, size.z) * 0.08f)
            {
                return false;
            }

            float s;
            if (isTerrainPad)
            {
                s = Mathf.Min(targetW / srcW, targetH / Mathf.Max(0.2f, srcH * 0.35f));
            }
            else
            {
                s = Mathf.Min(targetW / srcW, targetH / srcH);
            }

            s = Mathf.Clamp(s, 0.15f, 80f);
            go.transform.localScale = Vector3.one * s;

            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (isVolumeProp && bounds.size.y < Mathf.Max(bounds.size.x, bounds.size.z) * 0.07f)
            {
                return false;
            }

            var bottomY = bounds.min.y;
            var delta = placement.Y - bottomY;
            var p = go.transform.localPosition;
            go.transform.localPosition = new Vector3(p.x, p.y + delta, p.z);
            return true;
        }

        /// <summary>Tripo GLB às vezes chega branco/preto — força leitura de terreno.</summary>
        private static void NormalizeTerrainMaterials(GameObject go, string assetKey)
        {
            var isWaterish = assetKey.StartsWith("River") || assetKey is "Stream" or "Lake_A" ||
                             assetKey.StartsWith("Waterfall");
            var target = isWaterish
                ? new Color(0.14f, 0.32f, 0.42f)
                : assetKey.StartsWith("Distant")
                    ? Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Vegetation, 0.25f)
                    : CityVisualMaterials.StoneDark;

            foreach (var rend in go.GetComponentsInChildren<Renderer>(true))
            {
                if (rend == null)
                {
                    continue;
                }

                Color c;
                if (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_BaseColor"))
                {
                    c = rend.sharedMaterial.GetColor("_BaseColor");
                }
                else if (rend.sharedMaterial != null)
                {
                    c = rend.sharedMaterial.color;
                }
                else
                {
                    c = Color.magenta;
                }

                var lum = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                var broken = lum > 0.82f || lum < 0.07f || c.a < 0.15f;
                if (broken || isWaterish)
                {
                    CityVisualMaterials.Apply(rend, target);
                }
            }
        }

        private static GameObject? TryInstantiatePrefab(string assetKey, Transform parent)
        {
            if (!CityTerrainAssetCatalog.TryGet(assetKey, out var def))
            {
                return null;
            }

            var prefab = Resources.Load<GameObject>(ResourcesRoot + def.AssetId);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(ResourcesRoot + assetKey);
            }

            if (prefab == null)
            {
                return null;
            }

            def.Status = CityTerrainAssetStatus.Approved;
            def.SourcePath = ResourcesRoot + def.AssetId;
            return Object.Instantiate(prefab, parent, false);
        }
    }
}
