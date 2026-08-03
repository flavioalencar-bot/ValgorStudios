using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Resolve asset do kit: prefab HF em Resources, senão fallback procedural.
    /// Sem lógica de gameplay.
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
            if (go == null)
            {
                go = CityTerrainFallbackFactory.Create(assetKey, parent);
            }

            go.name = assetKey;
            go.transform.localPosition = new Vector3(placement.X, placement.Y, placement.Z);
            go.transform.localRotation = Quaternion.Euler(0f, placement.YawDegrees, 0f);
            go.transform.localScale = new Vector3(placement.ScaleX, placement.ScaleY, placement.ScaleZ);

            if (!collidersEnabled)
            {
                foreach (var col in go.GetComponentsInChildren<Collider>(true))
                {
                    Object.Destroy(col);
                }
            }

            return go;
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

            return Object.Instantiate(prefab, parent, false);
        }
    }
}
