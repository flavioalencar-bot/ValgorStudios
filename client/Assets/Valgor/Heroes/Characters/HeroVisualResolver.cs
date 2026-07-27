using UnityEngine;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.Characters
{
    public readonly struct HeroVisualResolveResult
    {
        public HeroVisualResolveResult(GameObject prefab, bool isTechnicalFallback, string message)
        {
            Prefab = prefab;
            IsTechnicalFallback = isTechnicalFallback;
            Message = message;
        }

        public GameObject Prefab { get; }
        public bool IsTechnicalFallback { get; }
        public string Message { get; }
    }

    /// <summary>
    /// Resolves the visual prefab for a hero id: real Vortex prefab when ready, else technical dummy.
    /// </summary>
    public static class HeroVisualResolver
    {
        public static HeroVisualResolveResult Resolve(
            string heroId,
            GameObject technicalDummyPrefab,
            GameObject vortexHeroPrefab = null)
        {
            if (heroId == VortexAssetPaths.HeroId)
            {
                GameObject vortex = vortexHeroPrefab;
#if UNITY_EDITOR
                if (vortex == null)
                {
                    vortex = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(VortexAssetPaths.HeroPrefab);
                }
#endif
                if (vortex == null)
                {
                    vortex = Resources.Load<GameObject>("Valgor/Vortex_Hero");
                }

                if (vortex != null)
                {
                    var visual = vortex.GetComponent<HeroVisualController>();
                    var fallback = visual == null || visual.UsingTechnicalFallback || !HasVortexSourceModel();
                    var msg = fallback
                        ? "Vortex_Hero shell pronto; FBX real ainda ausente — fallback técnico ativo."
                        : "Vortex_Hero (modelo real) resolvido.";
                    return new HeroVisualResolveResult(vortex, fallback, msg);
                }

                return new HeroVisualResolveResult(
                    technicalDummyPrefab,
                    true,
                    $"Vortex: prefab final ausente. Fallback técnico. Coloque o FBX em {VortexAssetPaths.Models}/");
            }

            return new HeroVisualResolveResult(
                technicalDummyPrefab,
                true,
                $"Hero {heroId}: usando fallback técnico até o modelo final.");
        }

        public static bool HasVortexSourceModel()
        {
#if UNITY_EDITOR
            foreach (var path in VortexAssetPaths.RequiredModelCandidates)
            {
                if (!string.IsNullOrEmpty(UnityEditor.AssetDatabase.AssetPathToGUID(path)))
                {
                    if (System.IO.File.Exists(path))
                        return true;
                }

                if (System.IO.File.Exists(path))
                    return true;
            }

            return false;
#else
            // Player: modelo incluso via referência de cena / Resources.
            return true;
#endif
        }
    }
}
