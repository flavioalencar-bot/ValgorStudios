using UnityEngine;

namespace Valgor.Core
{
    /// <summary>
    /// Materiais runtime seguros. Sprites/Default é sempre incluído (GraphicsSettings)
    /// e funciona em qualquer câmera/RT — evita magenta do URP Lit no preview.
    /// </summary>
    public static class RuntimeSafeMaterials
    {
        private static Material? _template;

        public static Material Create(Color color, string name = "RuntimeSafe")
        {
            EnsureTemplate();
            var mat = new Material(_template!) { name = name };
            ApplyColor(mat, color);
            ClearMaps(mat);
            return mat;
        }

        public static void Apply(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            var mat = renderer.material;
            if (mat == null || IsBroken(mat))
            {
                EnsureTemplate();
                mat = new Material(_template!);
                renderer.material = mat;
            }

            ApplyColor(mat, color);
            ClearMaps(mat);
        }

        public static void ApplyColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            material.color = color;
        }

        public static bool IsBroken(Material? material)
        {
            if (material == null)
            {
                return true;
            }

            var shader = material.shader;
            if (shader == null)
            {
                return true;
            }

            var name = shader.name ?? string.Empty;
            if (name.IndexOf("InternalError", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (name is "Standard" or "Diffuse" or "Legacy Shaders/Diffuse")
            {
                return true;
            }

            return false;
        }

        private static void ClearMaps(Material mat)
        {
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", null);
            }

            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", null);
            }
        }

        private static void EnsureTemplate()
        {
            if (_template != null && !IsBroken(_template))
            {
                return;
            }

            // 1) Sprites/Default — sempre na build, estável em preview RT.
            var sprites = Shader.Find("Sprites/Default");
            if (sprites != null)
            {
                _template = new Material(sprites) { name = "RuntimeSafe_Template" };
                ApplyColor(_template, Color.white);
                return;
            }

            // 2) Clone da primitiva URP.
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var src = temp.GetComponent<Renderer>().sharedMaterial;
            if (src != null && !IsBroken(src))
            {
                _template = new Material(src) { name = "RuntimeSafe_Template" };
                Object.Destroy(temp);
                ApplyColor(_template, Color.white);
                ClearMaps(_template);
                return;
            }

            Object.Destroy(temp);

            var dummy = Shader.Find("Valgor/Heroes/DummyUnlit");
            if (dummy != null)
            {
                _template = new Material(dummy) { name = "RuntimeSafe_Template" };
                ApplyColor(_template, Color.white);
                return;
            }

            Debug.LogError("[RuntimeSafeMaterials] Nenhum shader seguro disponível.");
            _template = new Material(Shader.Find("UI/Default") ?? Shader.Find("Unlit/Color"));
            if (_template != null)
            {
                ApplyColor(_template, Color.gray);
            }
        }
    }
}
