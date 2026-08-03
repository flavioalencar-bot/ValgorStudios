using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Atmosfera leve: bandeiras, fumaça, névoa, água — gated por qualidade.</summary>
    public sealed class CityAtmosphereFx : MonoBehaviour
    {
        public static void Attach(Transform cityRoot)
        {
            if (cityRoot == null)
            {
                return;
            }

            var host = cityRoot.Find("AmbientFX") ?? cityRoot;
            var fx = host.gameObject.GetComponent<CityAtmosphereFx>() ??
                     host.gameObject.AddComponent<CityAtmosphereFx>();
            fx.Bootstrap(cityRoot);
        }

        private Transform[] _flags = System.Array.Empty<Transform>();
        private Transform[] _smokes = System.Array.Empty<Transform>();
        private Transform[] _waters = System.Array.Empty<Transform>();
        private float _fogDensity;

        private void Bootstrap(Transform cityRoot)
        {
            _flags = Collect(cityRoot, "FlagCloth", "BannerCloth");
            _smokes = Collect(cityRoot, "Smoke", "FoundrySmoke", "MillSmoke", "SmokeHint");
            _waters = Collect(cityRoot, "FountainWater", "RiverWest", "RiverEast", "FallWest", "FallEast");

            if (CityAmbientQualitySettings.AllowLightFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                _fogDensity = CityAmbientQualitySettings.Current == CityAmbientQuality.High ? 0.012f : 0.008f;
                RenderSettings.fogDensity = _fogDensity;
                RenderSettings.fogColor = new Color(0.55f, 0.58f, 0.52f);
            }

            if (CityAmbientQualitySettings.AllowDustLeaves)
            {
                var leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leaf.name = "LeafWindHint";
                leaf.transform.SetParent(transform, false);
                leaf.transform.localPosition = new Vector3(0f, 1.2f, -6f);
                leaf.transform.localScale = new Vector3(0.15f, 0.05f, 0.2f);
                Object.Destroy(leaf.GetComponent<Collider>());
                CityArtMaterialLibrary.ApplyVariant(leaf.GetComponent<Renderer>(), CityArtMaterialCategory.Grass,
                    new Color(0.55f, 0.4f, 0.2f), 0.7f);
            }
        }

        private void Update()
        {
            var t = Time.unscaledTime;
            if (CityAmbientQualitySettings.AllowFlagMotion)
            {
                for (var i = 0; i < _flags.Length; i++)
                {
                    var f = _flags[i];
                    if (f == null)
                    {
                        continue;
                    }

                    var yaw = Mathf.Sin(t * 2.4f + i) * 12f;
                    f.localRotation = Quaternion.Euler(0f, yaw, Mathf.Sin(t * 3.1f + i) * 6f);
                }
            }

            if (CityAmbientQualitySettings.AllowSmoke)
            {
                for (var i = 0; i < _smokes.Length; i++)
                {
                    var s = _smokes[i];
                    if (s == null)
                    {
                        continue;
                    }

                    var p = s.localPosition;
                    s.localPosition = new Vector3(p.x, p.y + Mathf.Sin(t * 1.3f + i) * 0.01f, p.z);
                }
            }

            if (CityAmbientQualitySettings.AllowWaterFx)
            {
                for (var i = 0; i < _waters.Length; i++)
                {
                    var w = _waters[i];
                    if (w == null)
                    {
                        continue;
                    }

                    var p = w.localPosition;
                    w.localPosition = new Vector3(p.x, p.y + Mathf.Sin(t * 2.2f + i) * 0.004f, p.z);
                }
            }
        }

        private static Transform[] Collect(Transform root, params string[] nameContains)
        {
            var list = new System.Collections.Generic.List<Transform>(32);
            var all = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < all.Length; i++)
            {
                var n = all[i].name;
                for (var j = 0; j < nameContains.Length; j++)
                {
                    if (n.IndexOf(nameContains[j], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        list.Add(all[i]);
                        break;
                    }
                }
            }

            return list.ToArray();
        }
    }
}
