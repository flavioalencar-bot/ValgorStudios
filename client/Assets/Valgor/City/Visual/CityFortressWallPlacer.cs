using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Sprint 1 — coloca o kit modular em anel fechado.
    /// Portão HF (BuildingId=wall) permanece; ambiente só conecta asas.
    /// Fallback por trecho se Create falhar — nunca buraco.
    /// </summary>
    public static class CityFortressWallPlacer
    {
        public static bool TryBuild(Transform fortRoot, int wallLevel)
        {
            if (!CityFortressWallConfig.Enabled || fortRoot == null)
            {
                return false;
            }

            try
            {
                BuildRing(fortRoot, wallLevel);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Valgor.Fortress] Kit falhou — fallback procedural: {ex.Message}");
                return false;
            }
        }

        private static void BuildRing(Transform root, int wallLevel)
        {
            _ = wallLevel;
            var radius = CityCapitalLayoutConfig.WallRadius;
            var segments = CityFortressWallConfig.RingSegments;
            var gateHalf = CityFortressWallConfig.GateOpeningHalfAngleDeg;
            var towerCycle = CityFortressWallConfig.TowerCycle;
            var towerIndex = 0;

            // Asas conectando ao Portão HF (sul).
            PlaceGateWings(root, radius);

            // Anel modular.
            for (var i = 0; i < segments; i++)
            {
                var deg = i * (360f / segments);
                // 0° no layout = sul após +180° (mesmo convenção do EnvironmentBuilder).
                var angle = (deg + 180f) * Mathf.Deg2Rad;

                // Abertura sul para BuildingId=wall (WallGate HF).
                var southAngle = 180f; // após offset, sul
                var delta = Mathf.DeltaAngle(deg + 180f, southAngle);
                if (Mathf.Abs(delta) < gateHalf)
                {
                    continue;
                }

                var pos = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                var look = Quaternion.LookRotation(new Vector3(-pos.x, 0f, -pos.z));

                // Cantos ~45/135/225/315 → Corner90 + bastion.
                var isCorner = IsNearCorner(deg + 180f);
                GameObject module;
                if (isCorner)
                {
                    module = SafeCreate("WallCorner90", root);
                    TryAttachStairs(root, pos, look);
                }
                else if ((i + 1) % 8 == 0)
                {
                    module = SafeCreate("WallBastion", root);
                }
                else if ((i + 3) % 5 == 0)
                {
                    module = SafeCreate("WallStraight8m", root);
                }
                else
                {
                    module = SafeCreate("WallStraight4m", root);
                }

                module.transform.SetParent(root, false);
                module.transform.localPosition = pos;
                module.transform.localRotation = look;

                // Torres em ciclo irregular (nunca o mesmo padrão repetido linear).
                if (!isCorner && i % 5 == 2)
                {
                    var towerId = towerCycle[towerIndex % towerCycle.Length];
                    towerIndex++;
                    if (i % 11 == 0)
                    {
                        towerIndex++;
                    }

                    var tower = SafeCreate(towerId, root);
                    tower.transform.SetParent(root, false);
                    tower.transform.localPosition = pos;
                    tower.transform.localRotation = look;
                }
            }

            // Torres de canto dedicadas (4) — tamanhos misturados.
            PlaceCornerTowers(root, radius);

            // Ponte / approach do portão (visual).
            PlaceGateApproach(root);

            // LOD hints (LOD0 ativo; pastas LOD1–3 preparadas no kit editor).
            var lod = root.gameObject.AddComponent<CityFortressWallLodHint>();
            lod.Configure(CityFortressWallConfig.LodLevels);
        }

        private static void PlaceGateWings(Transform root, float radius)
        {
            var z = CityCapitalLayoutConfig.GetPosition("wall").Z;
            // Asas L/R alinhadas ao slot do portão.
            var wingL = SafeCreate("WallEnd", root);
            wingL.transform.SetParent(root, false);
            wingL.transform.localPosition = new Vector3(-5.2f, 0f, z + 0.35f);
            wingL.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            var wingR = SafeCreate("WallEnd", root);
            wingR.transform.SetParent(root, false);
            wingR.transform.localPosition = new Vector3(5.2f, 0f, z + 0.35f);
            wingR.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // Segmentos curtos bridging ring → wing.
            var bridgeAngleL = 180f + CityFortressWallConfig.GateOpeningHalfAngleDeg + 4f;
            var bridgeAngleR = 180f - CityFortressWallConfig.GateOpeningHalfAngleDeg - 4f;
            PlaceStraightAtAngle(root, radius, bridgeAngleL, "WallStraight4m");
            PlaceStraightAtAngle(root, radius, bridgeAngleR, "WallStraight4m");
        }

        private static void PlaceStraightAtAngle(Transform root, float radius, float degFromNorth, string moduleId)
        {
            var angle = degFromNorth * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            var go = SafeCreate(moduleId, root);
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.LookRotation(new Vector3(-pos.x, 0f, -pos.z));
        }

        private static void PlaceCornerTowers(Transform root, float radius)
        {
            float[] corners = { 45f, 135f, 225f, 315f };
            string[] sizes = { "WallTowerLarge", "WallTowerMedium", "WallTowerLarge", "WallTowerMedium" };
            for (var i = 0; i < corners.Length; i++)
            {
                var angle = (corners[i] + 180f) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                var tower = SafeCreate(sizes[i], root);
                tower.transform.SetParent(root, false);
                tower.transform.localPosition = pos;
                tower.transform.localRotation = Quaternion.LookRotation(new Vector3(-pos.x, 0f, -pos.z));
            }
        }

        private static void TryAttachStairs(Transform root, Vector3 wallPos, Quaternion look)
        {
            var stairs = SafeCreate("WallStairs", root);
            stairs.transform.SetParent(root, false);
            // Escada pelo lado interno da muralha.
            var inward = look * Vector3.forward;
            stairs.transform.localPosition = wallPos + inward * 1.6f;
            stairs.transform.localRotation = look;
        }

        private static void PlaceGateApproach(Transform root)
        {
            var z = CityCapitalLayoutConfig.MainGateZ;
            var bridge = SafeCreate("WallWalk", root);
            bridge.name = "GateApproachWalk";
            bridge.transform.SetParent(root, false);
            bridge.transform.localPosition = new Vector3(0f, 0.05f, z - 2.4f);
            bridge.transform.localScale = new Vector3(0.85f, 1f, 1.2f);

            // Pedras / rampa visual na fundação do portão.
            for (var i = 0; i < 5; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"GateFoundationRock_{i}";
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(-2f + i * 1.0f, 0.12f, z - 1.1f);
                go.transform.localScale = new Vector3(0.85f, 0.35f, 0.7f);
                UnityEngine.Object.Destroy(go.GetComponent<Collider>());
                CityFortressWallMaterials.ApplyStone(go.GetComponent<Renderer>());
            }
        }

        private static void TrimToFoundationOnly(Transform root)
        {
            _ = root;
        }

        private static bool IsNearCorner(float deg)
        {
            float[] corners = { 45f, 135f, 225f, 315f };
            for (var i = 0; i < corners.Length; i++)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(deg, corners[i])) < 8f)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject SafeCreate(string moduleId, Transform parent)
        {
            try
            {
                // Prefere fábrica runtime (sempre sincronizada). Prefabs Resources = cache opcional.
                return CityFortressWallModuleFactory.Create(moduleId, parent);
            }
            catch
            {
                // Fallback mínimo — segmento reto (nunca buraco).
                return CityFortressWallModuleFactory.Create("WallStraight4m", parent);
            }
        }

        private static void ApplyStoneRecursive(Transform t)
        {
            foreach (var renderer in t.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var n = renderer.gameObject.name;
                if (n.IndexOf("Walk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("Deck", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = CityFortressWallMaterials.Walk;
                }
                else if (n.IndexOf("Flag", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         n.IndexOf("Banner", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = CityFortressWallMaterials.Banner;
                }
                else if (n.IndexOf("Flame", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         n.IndexOf("Fire", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = CityFortressWallMaterials.Fire;
                }
                else if (n.IndexOf("Pole", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         n.IndexOf("Wood", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = CityFortressWallMaterials.Wood;
                }
                else
                {
                    CityFortressWallMaterials.ApplyStone(renderer);
                }
            }
        }
    }

    /// <summary>Hint LOD — Sprint 1 prepara 4 níveis; runtime usa LOD0.</summary>
    public sealed class CityFortressWallLodHint : MonoBehaviour
    {
        public int LodLevels { get; private set; } = 4;
        public int ActiveLod { get; private set; }

        public void Configure(int levels)
        {
            LodLevels = Mathf.Clamp(levels, 1, 4);
            ActiveLod = 0;
            gameObject.name = $"{gameObject.name}_LOD0";
        }
    }
}
