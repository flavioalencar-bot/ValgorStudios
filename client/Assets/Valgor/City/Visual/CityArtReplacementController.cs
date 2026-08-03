using System.Collections.Generic;
using UnityEngine;
using Valgor.City.Buildings;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Substitui proxy procedural por arte definitiva sob Visual, preservando lógica no root.
    /// Sem asset → mantém proxy (fallback).
    /// </summary>
    public sealed class CityArtReplacementController : MonoBehaviour
    {
        private readonly HashSet<string> _applied = new();

        public static CityArtReplacementController Ensure()
        {
            var existing = FindFirstObjectByType<CityArtReplacementController>();
            if (existing != null)
            {
                return existing;
            }

            var host = new GameObject(nameof(CityArtReplacementController));
            return host.AddComponent<CityArtReplacementController>();
        }

        public void ApplyAll(IEnumerable<BuildingView> views)
        {
            if (!CityArtPipelineConfig.Enabled || views == null)
            {
                return;
            }

            foreach (var view in views)
            {
                ApplyBuilding(view);
            }
        }

        public bool ApplyBuilding(BuildingView view)
        {
            if (!CityArtPipelineConfig.Enabled || view == null || view.Instance == null)
            {
                return false;
            }

            var buildingId = BuildingIdMigration.ToCanonical(view.Instance.DefinitionId);
            var result = CityArtAssetResolver.ResolveBuilding(buildingId, CityAmbientQualitySettings.Current);

            if (result.Mode == CityArtResolveMode.ExistingCastleLoader ||
                result.Mode == CityArtResolveMode.ProceduralFallback ||
                result.UsedFallback)
            {
                return false;
            }

            var catalog = CityArtAssetCatalog.GetOrCreateRuntime();
            var entry = catalog.FindByBuildingId(buildingId);
            var prefab = CityArtAssetResolver.SelectPrefab(entry, result.Mode);
            if (prefab == null)
            {
                return false;
            }

            if (!CityArtPipelineConfig.PrefabMatchesBuilding(buildingId, prefab.name))
            {
                Debug.LogError(
                    $"[Valgor.Art] Mapeamento proibido: BuildingId '{buildingId}' ← prefab '{prefab.name}'. " +
                    "Castle≠DragonTower. Abortando swap (fallback).");
                return false;
            }

            var issues = CityArtPrefabValidation.Validate(prefab);
            if (issues.Count > 0)
            {
                Debug.LogWarning(
                    $"[Valgor.Art] Prefab rejeitado para '{buildingId}': {string.Join("; ", issues)} — fallback.");
                return false;
            }

            var visual = view.VisualRoot;
            if (visual == null)
            {
                visual = view.transform.Find(CityArtPipelineConfig.VisualRootName);
            }

            if (visual == null)
            {
                return false;
            }

            // Evita duplicar no mesmo slot.
            var key = view.GetInstanceID() + ":" + buildingId;
            var artParent = ResolveArtParent(visual, buildingId);
            ClearPreviousArt(visual);
            HideProceduralProxies(visual);

            var instance = Instantiate(prefab, artParent, false);
            instance.name = CityArtPipelineConfig.HighFidelityChildName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            // Preserva escala bakeada do prefab (não forçar 1).
            instance.transform.localScale = prefab.transform.localScale;
            StripGameplayColliders(instance);

            if (string.Equals(buildingId, "dragon-tower", System.StringComparison.Ordinal))
            {
                CityDragonTowerArtDressing.Attach(visual);
                CityDragonTowerArtDressing.PlaceHighFidelityOnPlateau(instance.transform);
            }

            _applied.Add(key);
            view.RecacheRenderersAfterArtSwap();
            Debug.Log(
                $"[Valgor.Art] ArtHF aplicado em '{buildingId}' prefab={prefab.name} " +
                $"parent={artParent.name} scale={instance.transform.localScale} " +
                $"renderers={instance.GetComponentsInChildren<Renderer>(true).Length}");
            return true;
        }

        /// <summary>Muralha usa WallVisualRoot sob Visual; demais usam Visual direto.</summary>
        private static Transform ResolveArtParent(Transform visual, string buildingId)
        {
            if (!string.Equals(buildingId, "wall", System.StringComparison.Ordinal))
            {
                return visual;
            }

            var wallRoot = visual.Find(CityArtPipelineConfig.WallVisualRootName);
            if (wallRoot == null)
            {
                var go = new GameObject(CityArtPipelineConfig.WallVisualRootName);
                wallRoot = go.transform;
                wallRoot.SetParent(visual, false);
                wallRoot.localPosition = Vector3.zero;
                wallRoot.localRotation = Quaternion.identity;
                wallRoot.localScale = Vector3.one;
            }

            return wallRoot;
        }

        private static void ClearPreviousArt(Transform visual)
        {
            var existing = visual.Find(CityArtPipelineConfig.HighFidelityChildName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            var wallRoot = visual.Find(CityArtPipelineConfig.WallVisualRootName);
            if (wallRoot != null)
            {
                var wallArt = wallRoot.Find(CityArtPipelineConfig.HighFidelityChildName);
                if (wallArt != null)
                {
                    Destroy(wallArt.gameObject);
                }
            }
        }

        private static void HideProceduralProxies(Transform visual)
        {
            for (var i = 0; i < visual.childCount; i++)
            {
                var child = visual.GetChild(i);
                var n = child.name;
                if (n == CityArtPipelineConfig.HighFidelityChildName ||
                    n == CityArtPipelineConfig.WallVisualRootName)
                {
                    continue;
                }

                // Preserva dressing e ninho (DragonNestView); tiers/proxy ficam ocultos com ArtHF.
                if (n.StartsWith("CastleDressing", System.StringComparison.Ordinal) ||
                    n.StartsWith("CastleArtDressing", System.StringComparison.Ordinal) ||
                    n.StartsWith("ArtDressing", System.StringComparison.Ordinal) ||
                    string.Equals(n, "NestOccupants", System.StringComparison.Ordinal))
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private static void StripGameplayColliders(GameObject root)
        {
            var cols = root.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < cols.Length; i++)
            {
                Destroy(cols[i]);
            }
        }
    }

    /// <summary>Validação de prefabs artísticos (sem lógica / sem collider de gameplay).</summary>
    public static class CityArtPrefabValidation
    {
        public static List<string> Validate(GameObject prefab)
        {
            var issues = new List<string>();
            if (prefab == null)
            {
                issues.Add("prefab null");
                return issues;
            }

            if (!CityArtPipelineConfig.MatchesNamingConvention(prefab.name) &&
                !prefab.name.StartsWith("Castle_Tier", System.StringComparison.Ordinal))
            {
                // Aviso soft — packs legados de castelo passam; novos devem seguir VAL_Capital_.
                // Não bloqueia castelo Resources existente.
            }

            var behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                var b = behaviours[i];
                if (b == null)
                {
                    continue;
                }

                var typeName = b.GetType().Name;
                if (CityArtPipelineConfig.IsForbiddenScript(typeName))
                {
                    issues.Add("script proibido: " + typeName);
                }
            }

            return issues;
        }
    }
}
