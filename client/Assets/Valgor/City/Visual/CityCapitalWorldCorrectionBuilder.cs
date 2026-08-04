using System.Collections.Generic;
using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>
    /// Composição curada da Capital — sem vitrine de catálogo, sem Tripo.
    /// </summary>
    public static class CityCapitalWorldCorrectionBuilder
    {
        public static void Build(Transform cityRoot)
        {
            CityCapitalWorldNoTripo.NeverCallTripoApi("composition-build");

            var existing = cityRoot.Find(CityCapitalWorldConfig.WorldRootName);
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }

            var world = new GameObject(CityCapitalWorldConfig.WorldRootName).transform;
            world.SetParent(cityRoot, false);

            var terrain = new GameObject("TerrainModules").transform;
            terrain.SetParent(world, false);
            var water = new GameObject("WaterModules").transform;
            water.SetParent(world, false);
            var props = new GameObject("NatureProps").transform;
            props.SetParent(world, false);

            BuildContinuousTerrain(terrain);
            BuildHorizonRing(terrain);
            BuildDragonTowerShelf(terrain);
            BuildCurvedRivers(water);
            BuildEdgeVegetation(props);
            ApplyAtmosphere();

            Debug.Log(
                $"[Valgor.City] {CityCapitalWorldNoTripo.ModeName} montada " +
                $"(previewV2={CityCapitalLayoutRuntime.IsPreviewV2}, " +
                $"tripoCalls={CityCapitalWorldNoTripo.TripoApiInvocationCount}, " +
                $"debug={CityCapitalWorldNoTripo.DebugOverlays}).");
        }

        /// <summary>Base contínua — discos empilhados escondem bordas no overview.</summary>
        private static void BuildContinuousTerrain(Transform parent)
        {
            var root = new GameObject("ContinuousTerrain").transform;
            root.SetParent(parent, false);

            // Disco externo quase fora do overview (fog esconde borda).
            SpawnDisk(root, "OuterBasin", 0f, -0.22f, 0f, 62f, 0.12f,
                new Color(0.22f, 0.24f, 0.2f));
            SpawnDisk(root, "MidEarth", 0f, -0.12f, 0f, 44f, 0.11f,
                new Color(0.28f, 0.32f, 0.23f));
            SpawnDisk(root, "InnerMeadow", 0.4f, -0.05f, 0.3f, 34f, 0.1f,
                new Color(0.3f, 0.38f, 0.25f));

            // Platô central sob a cidade (ligeiramente elevado).
            SpawnDisk(root, "CapitalPlateau", 0.3f, 0.06f, 0.8f, 24f, 0.2f,
                new Color(0.4f, 0.36f, 0.3f));
            SpawnDisk(root, "CapitalRim", 0.2f, 0.14f, 1f, 27f, 0.14f,
                new Color(0.35f, 0.32f, 0.27f));

            SpawnDisk(root, "NorthRise", 2f, 0.32f, 12f, 14f, 0.5f,
                new Color(0.33f, 0.3f, 0.26f));
            SpawnDisk(root, "SouthApproach", 0f, 0.1f, -12f, 12f, 0.26f,
                new Color(0.37f, 0.34f, 0.28f));

            // Colinas de transição — base das montanhas, dentro do overview.
            SpawnDisk(root, "HillN", 0f, 1.1f, 34f, 16f, 1.8f,
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.35f));
            SpawnDisk(root, "HillNW", -24f, 0.9f, 28f, 13f, 1.4f,
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Vegetation, 0.2f));
            SpawnDisk(root, "HillNE", 24f, 0.9f, 26f, 13f, 1.4f,
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneDark, 0.4f));
            SpawnDisk(root, "HillSW", -22f, 0.5f, -22f, 11f, 1.0f, CityVisualMaterials.Dirt);
            SpawnDisk(root, "HillSE", 22f, 0.5f, -20f, 11f, 1.0f,
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.StoneLight, 0.2f));
        }

        /// <summary>Horizonte: poucas montanhas locais, longe, atrás da Capital.</summary>
        private static void BuildHorizonRing(Transform parent)
        {
            var root = new GameObject("Horizon").transform;
            root.SetParent(parent, false);
            var quality = CityAmbientQualitySettings.Current;

            var placements = new List<WorldPropPlacement>();
            if (quality >= CityAmbientQuality.Low)
            {
                placements.Add(new WorldPropPlacement("DistantMountain_A", 0f, 0.5f, 36f, 12f, 18f, 14f, 14f));
                placements.Add(new WorldPropPlacement("Mountain_B", -30f, 0.4f, 30f, 40f, 14f, 12f, 11f));
                placements.Add(new WorldPropPlacement("Mountain_C", 30f, 0.4f, 28f, -35f, 13f, 12f, 10f));
            }

            if (quality >= CityAmbientQuality.Medium)
            {
                placements.Add(new WorldPropPlacement("DistantMountain_B", 16f, 0.45f, 38f, -20f, 15f, 12f, 11f));
                placements.Add(new WorldPropPlacement("Mountain_D", -16f, 0.4f, 38f, 55f, 14f, 11f, 10f));
            }

            if (quality >= CityAmbientQuality.High)
            {
                placements.Add(new WorldPropPlacement("Mountain_A", 34f, 0.3f, 12f, -90f, 11f, 10f, 9f));
                placements.Add(new WorldPropPlacement("Mountain_A", -34f, 0.3f, 10f, 100f, 11f, 10f, 9f));
            }

            foreach (var p in placements)
            {
                var dist = Mathf.Sqrt(p.X * p.X + p.Z * p.Z);
                if (dist < CityCapitalWorldConfig.PlayableRadius + 6f)
                {
                    continue;
                }

                if (CityCapitalWorldConfig.OverlapsReserved(p.X, p.Z, Mathf.Max(p.ScaleX, p.ScaleZ) * 0.3f))
                {
                    continue;
                }

                var go = CityTerrainAssetResolver.Spawn(p.AssetKey, root, p, collidersEnabled: false);
                go.name = "Horizon_" + p.AssetKey;
                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                {
                    r.shadowCastingMode = CityCapitalWorldConfig.AllowDistantMountainShadows(quality)
                        ? UnityEngine.Rendering.ShadowCastingMode.On
                        : UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = false;
                    ForceStoneIfBroken(r);
                }
            }
        }

        private static void BuildDragonTowerShelf(Transform parent)
        {
            var tower = CityCapitalLayoutConfig.GetPosition("dragon-tower");
            var root = new GameObject("DragonShelf").transform;
            root.SetParent(parent, false);

            // Base integrada (pequena — não pad marrom dominante).
            SpawnDisk(root, "TowerPad", tower.X, Mathf.Max(0.25f, tower.Y * 0.35f), tower.Z,
                5.5f, 0.45f, Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.25f));
            SpawnDisk(root, "TowerApproach", tower.X - 1.2f, 0.12f, tower.Z - 3.2f,
                4.5f, 0.22f, Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.5f));

            var cliff = new WorldPropPlacement(
                "Cliff_DragonTower",
                tower.X + 0.4f,
                Mathf.Max(0.12f, tower.Y - 0.45f),
                tower.Z + 1.2f,
                8f,
                5.5f,
                3.4f,
                5.2f);
            var go = CityTerrainAssetResolver.Spawn(cliff.AssetKey, root, cliff, false);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                ForceStoneIfBroken(r);
            }
        }

        /// <summary>Rio principal curvo + braço — segmentos curtos, sem faixa reta única.</summary>
        private static void BuildCurvedRivers(Transform parent)
        {
            var root = new GameObject("CurvedRivers").transform;
            root.SetParent(parent, false);

            var west = new (float x, float z, float yaw, float w, float len)[]
            {
                (-24f, -14f, 22f, 1.9f, 5.5f),
                (-25.2f, -7f, 10f, 2.0f, 5.5f),
                (-25.8f, 0f, -4f, 2.1f, 5.5f),
                (-25f, 7f, -16f, 2.0f, 5.5f),
                (-23.2f, 13f, -26f, 1.9f, 5.2f),
                (-20.5f, 18f, -38f, 2.2f, 4.8f)
            };
            for (var i = 0; i < west.Length; i++)
            {
                var s = west[i];
                SpawnRiverSegment(root, $"West_{i}", s.x, s.z, s.yaw, s.w, s.len);
            }

            var south = new (float x, float z, float yaw, float w, float len)[]
            {
                (-5f, -23.5f, 72f, 2.4f, 5f),
                (1.5f, -25f, 90f, 2.8f, 5.5f),
                (8f, -23.5f, 108f, 2.3f, 5f)
            };
            for (var i = 0; i < south.Length; i++)
            {
                var s = south[i];
                SpawnRiverSegment(root, $"South_{i}", s.x, s.z, s.yaw, s.w, s.len);
            }

            if (CityAmbientQualitySettings.Current >= CityAmbientQuality.Medium)
            {
                var east = new (float x, float z, float yaw, float w, float len)[]
                {
                    (24f, -8f, -10f, 1.8f, 5f),
                    (25f, -1f, 6f, 1.9f, 5.2f),
                    (24.2f, 6f, 16f, 1.8f, 5f)
                };
                for (var i = 0; i < east.Length; i++)
                {
                    var s = east[i];
                    SpawnRiverSegment(root, $"East_{i}", s.x, s.z, s.yaw, s.w, s.len);
                }
            }

            SpawnBridge(root, 0.4f, -21.5f, 90f);
        }

        private static void SpawnRiverSegment(
            Transform parent,
            string name,
            float x,
            float z,
            float yaw,
            float width,
            float length)
        {
            var seg = new GameObject(name).transform;
            seg.SetParent(parent, false);
            seg.localPosition = new Vector3(x, -0.12f, z);
            seg.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "Bed";
            bed.transform.SetParent(seg, false);
            bed.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            bed.transform.localScale = new Vector3(width + 1.1f, 0.22f, length);
            Object.Destroy(bed.GetComponent<Collider>());
            CityVisualMaterials.Apply(bed.GetComponent<Renderer>(),
                Color.Lerp(CityVisualMaterials.StoneDark, CityVisualMaterials.Dirt, 0.4f));

            var bankL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bankL.name = "BankL";
            bankL.transform.SetParent(seg, false);
            bankL.transform.localPosition = new Vector3(-(width * 0.55f + 0.35f), 0.06f, 0f);
            bankL.transform.localScale = new Vector3(0.7f, 0.28f, length * 0.95f);
            Object.Destroy(bankL.GetComponent<Collider>());
            CityVisualMaterials.Apply(bankL.GetComponent<Renderer>(),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.45f));

            var bankR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bankR.name = "BankR";
            bankR.transform.SetParent(seg, false);
            bankR.transform.localPosition = new Vector3(width * 0.55f + 0.35f, 0.06f, 0f);
            bankR.transform.localScale = new Vector3(0.7f, 0.28f, length * 0.95f);
            Object.Destroy(bankR.GetComponent<Collider>());
            CityVisualMaterials.Apply(bankR.GetComponent<Renderer>(),
                Color.Lerp(CityVisualMaterials.Dirt, CityVisualMaterials.Vegetation, 0.4f));

            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "Water";
            water.transform.SetParent(seg, false);
            water.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            water.transform.localScale = new Vector3(width * 0.85f, 0.04f, length * 0.92f);
            Object.Destroy(water.GetComponent<Collider>());
            water.GetComponent<Renderer>().sharedMaterial =
                RuntimeSafeMaterials.Create(new Color(0.07f, 0.2f, 0.28f), "CorrectionRiverWater");
        }

        private static void SpawnBridge(Transform parent, float x, float z, float yaw)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "GateBridge";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, 0.12f, z);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = new Vector3(5.2f, 0.35f, 2.4f);
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), CityVisualMaterials.StoneLight);

            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Rail";
            rail.transform.SetParent(go.transform, false);
            rail.transform.localPosition = new Vector3(0f, 0.8f, 0.45f);
            rail.transform.localScale = new Vector3(1.02f, 0.45f, 0.12f);
            Object.Destroy(rail.GetComponent<Collider>());
            CityVisualMaterials.Apply(rail.GetComponent<Renderer>(), CityVisualMaterials.StoneDark);
        }

        private static void BuildEdgeVegetation(Transform parent)
        {
            if (CityAmbientQualitySettings.Current == CityAmbientQuality.Low)
            {
                return;
            }

            var root = new GameObject("EdgeVegetation").transform;
            root.SetParent(parent, false);
            var count = CityAmbientQualitySettings.Current == CityAmbientQuality.High ? 18 : 12;
            for (var i = 0; i < count; i++)
            {
                var angle = i * (Mathf.PI * 2f / count) + 0.2f;
                var radius = 31f + (i % 3) * 1.8f;
                var x = Mathf.Sin(angle) * radius;
                var z = Mathf.Cos(angle) * radius;
                if (CityCapitalWorldConfig.OverlapsReserved(x, z, 1.2f))
                {
                    continue;
                }

                // Silhueta vertical (não esfera).
                var canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                canopy.name = $"EdgeTree_{i}";
                canopy.transform.SetParent(root, false);
                canopy.transform.localPosition = new Vector3(x, 1.1f + (i % 3) * 0.25f, z);
                canopy.transform.localScale = new Vector3(1.6f + (i % 3) * 0.4f, 2.0f + (i % 4) * 0.35f, 0.4f);
                canopy.transform.LookAt(new Vector3(0f, canopy.transform.position.y, 0f));
                Object.Destroy(canopy.GetComponent<Collider>());
                CityVisualMaterials.Apply(canopy.GetComponent<Renderer>(),
                    Color.Lerp(CityVisualMaterials.Vegetation, new Color(0.1f, 0.18f, 0.08f), 0.4f));
            }
        }

        private static void ApplyAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = CityAmbientQualitySettings.Current switch
            {
                CityAmbientQuality.Low => 0.018f,
                CityAmbientQuality.High => 0.022f,
                _ => 0.02f
            };
            RenderSettings.fogColor = new Color(0.55f, 0.6f, 0.68f);
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.68f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.48f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.2f, 0.16f);
        }

        private static void SpawnDisk(
            Transform parent,
            string name,
            float x,
            float y,
            float z,
            float diameter,
            float height,
            Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, y, z);
            go.transform.localScale = new Vector3(diameter, height * 0.5f, diameter);
            Object.Destroy(go.GetComponent<Collider>());
            CityVisualMaterials.Apply(go.GetComponent<Renderer>(), color);
        }

        private static void ForceStoneIfBroken(Renderer rend)
        {
            if (rend == null || rend.sharedMaterial == null)
            {
                CityVisualMaterials.Apply(rend, CityVisualMaterials.StoneDark);
                return;
            }

            Color c;
            if (rend.sharedMaterial.HasProperty("_BaseColor"))
            {
                c = rend.sharedMaterial.GetColor("_BaseColor");
            }
            else
            {
                c = rend.sharedMaterial.color;
            }

            var lum = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
            if (lum > 0.82f || lum < 0.07f || c.a < 0.15f)
            {
                CityVisualMaterials.Apply(rend, CityVisualMaterials.StoneDark);
            }
        }
    }
}
