using UnityEngine;
using Valgor.Core;

namespace Valgor.Heroes.Preview360
{
    /// <summary>
    /// Garante materiais válidos no preview — nunca magenta.
    /// Vortex: preto/dourado via DummyUnlit (incluso na build).
    /// </summary>
    public static class HeroPreviewMaterialSanitizer
    {
        private static Material? _bodyMat;
        private static Material? _goldMat;
        private static Material? _particleMat;
        private static Material? _fallbackMat;

        public static void Sanitize(GameObject root, bool preferBlackGold)
        {
            if (root == null)
            {
                return;
            }

            EnsureMaterials();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                // Desliga emission/property blocks que podem forçar magenta.
                renderer.SetPropertyBlock(null);

                if (renderer is ParticleSystemRenderer particleRenderer)
                {
                    particleRenderer.sharedMaterial = _particleMat;
                    continue;
                }

                var isSword = renderer.name.IndexOf("Sword", System.StringComparison.OrdinalIgnoreCase) >= 0
                              || IsUnderNamedAncestor(renderer.transform, "Vortex_DragonSword");

                if (preferBlackGold)
                {
                    var count = Mathf.Max(1, renderer.sharedMaterials?.Length ?? 0);
                    var forced = new Material[count];
                    for (var i = 0; i < count; i++)
                    {
                        forced[i] = isSword ? _goldMat : _bodyMat;
                    }

                    renderer.sharedMaterials = forced;
                    renderer.materials = forced;
                    continue;
                }

                var mats = renderer.sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    renderer.sharedMaterial = _fallbackMat;
                    continue;
                }

                var changed = false;
                for (var i = 0; i < mats.Length; i++)
                {
                    if (RuntimeSafeMaterials.IsBroken(mats[i]))
                    {
                        mats[i] = _fallbackMat;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = mats;
                    renderer.materials = mats;
                }
            }
        }

        private static void EnsureMaterials()
        {
            if (_bodyMat != null)
            {
                return;
            }

            _bodyMat = RuntimeSafeMaterials.Create(new Color(0.07f, 0.08f, 0.1f), "Runtime_Vortex_Body");
            _goldMat = RuntimeSafeMaterials.Create(new Color(0.82f, 0.64f, 0.22f), "Runtime_Vortex_Gold");
            _fallbackMat = RuntimeSafeMaterials.Create(new Color(0.32f, 0.36f, 0.42f), "Runtime_Hero_Fallback");
            _particleMat = RuntimeSafeMaterials.Create(new Color(0.9f, 0.72f, 0.28f), "Runtime_Vortex_Vfx");
        }

        private static bool IsUnderNamedAncestor(Transform t, string name)
        {
            while (t != null)
            {
                if (t.name == name)
                {
                    return true;
                }

                t = t.parent;
            }

            return false;
        }
    }
}
