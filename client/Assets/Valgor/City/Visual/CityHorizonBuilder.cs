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

            // Anel de névoa / solo distante.
            var mist = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mist.name = "HorizonMistRing";
            mist.transform.SetParent(root, false);
            mist.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            mist.transform.localScale = new Vector3(CityCapitalWorldConfig.DistantHorizonRadius * 2.1f, 0.05f,
                CityCapitalWorldConfig.DistantHorizonRadius * 2.1f);
            Object.Destroy(mist.GetComponent<Collider>());
            CityVisualMaterials.Apply(
                mist.GetComponent<Renderer>(),
                Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.45f, 0.5f, 0.48f), 0.45f));
        }
    }
}
