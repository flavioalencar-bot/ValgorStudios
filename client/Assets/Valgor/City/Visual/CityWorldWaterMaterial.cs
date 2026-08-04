using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Material de água + scroll UV leve (Low/Medium/High).</summary>
    public static class CityWorldWaterMaterial
    {
        private static Material? _shared;

        public static Material Shared
        {
            get
            {
                if (_shared != null)
                {
                    return _shared;
                }

                var shader = Shader.Find("Universal Render Pipeline/Lit") ??
                             Shader.Find("URP/Lit") ??
                             Shader.Find("Standard");
                _shared = new Material(shader)
                {
                    name = "M_CapitalWorldWater",
                    color = new Color(0.12f, 0.42f, 0.58f, 0.82f)
                };
                if (_shared.HasProperty("_Surface"))
                {
                    _shared.SetFloat("_Surface", 1f); // Transparent
                }

                if (_shared.HasProperty("_BaseColor"))
                {
                    _shared.SetColor("_BaseColor", new Color(0.12f, 0.42f, 0.58f, 0.82f));
                }

                if (_shared.HasProperty("_Smoothness"))
                {
                    _shared.SetFloat("_Smoothness", 0.85f);
                }

                _shared.renderQueue = 3000;
                return _shared;
            }
        }

        public static void ApplyTo(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = Shared;
        }
    }

    /// <summary>Scroll simples da água no overview.</summary>
    public sealed class CityWorldWaterScroll : MonoBehaviour
    {
        [SerializeField] private float speed = 0.035f;
        private Renderer? _renderer;
        private Material? _instance;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _instance = _renderer.material;
            }
        }

        private void Update()
        {
            if (_instance == null || CityAmbientQualitySettings.Current == CityAmbientQuality.Low)
            {
                return;
            }

            var o = Time.time * speed;
            if (_instance.HasProperty("_BaseMap"))
            {
                _instance.SetTextureOffset("_BaseMap", new Vector2(o * 0.2f, o));
            }
            else
            {
                _instance.mainTextureOffset = new Vector2(o * 0.2f, o);
            }
        }
    }
}
