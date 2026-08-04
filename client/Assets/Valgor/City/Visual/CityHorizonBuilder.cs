using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Camada 1 — horizonte cinematográfico (sem collider, LOD baixo).</summary>
    public static class CityHorizonBuilder
    {
        public static void Build(Transform parent)
        {
            var quality = CityAmbientQualitySettings.Current;
            var root = new GameObject("Horizon").transform;
            root.SetParent(parent, false);

            var i = 0;
            foreach (var p in CityCapitalWorldConfig.HorizonMountains())
            {
                var go = CityTerrainAssetResolver.Spawn(p.AssetKey, root, p, collidersEnabled: false);
                go.name = $"Horizon_{i}_{p.AssetKey}";
                var renderers = go.GetComponentsInChildren<Renderer>(true);
                for (var r = 0; r < renderers.Length; r++)
                {
                    renderers[r].shadowCastingMode = CityCapitalWorldConfig.AllowDistantMountainShadows(quality)
                        ? UnityEngine.Rendering.ShadowCastingMode.On
                        : UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderers[r].receiveShadows = false;
                }

                i++;
            }
        }
    }
}
