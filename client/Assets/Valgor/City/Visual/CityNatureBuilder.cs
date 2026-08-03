using System;
using UnityEngine;

namespace Valgor.City.Visual
{
    /// <summary>Vegetação orgânica Art Pass 2.0 — distribuição sem grade rígida.</summary>
    public static class CityNatureBuilder
    {
        public static void Build(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var rng = new System.Random(0x27E2026);
            var budget = CityCapitalEnvironmentConfig.VegetationCount(CityAmbientQualitySettings.Current, 2);
            budget = Math.Clamp(budget + 10, 16, 56);

            for (var i = 0; i < budget; i++)
            {
                var ang = (float)(rng.NextDouble() * Math.PI * 2.0);
                var rad = 7.5f + (float)rng.NextDouble() * 14.5f;
                var x = Mathf.Sin(ang) * rad + (float)(rng.NextDouble() * 2.4 - 1.2);
                var z = Mathf.Cos(ang) * rad + (float)(rng.NextDouble() * 2.4 - 1.2);
                // Evitar eixo imperial / praça / castelo.
                if (Mathf.Abs(x) < 3.2f && z > -17f && z < 16f)
                {
                    continue;
                }

                var species = rng.Next(0, CityArtPass2Config.NatureSpeciesCount);
                Place(root, i, species, new Vector3(x, 0f, z), rng);
            }

            // Musgos e gramíneas perto das muralhas.
            for (var i = 0; i < 10; i++)
            {
                var a = i * 36f * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 18.8f, 0.08f, Mathf.Cos(a) * 18.8f);
                Dec(root, PrimitiveType.Cube, $"Moss_{i}", p,
                    new Vector3(0.7f + (i % 3) * 0.15f, 0.08f, 0.55f), CityArtMaterialCategory.Grass);
            }
        }

        private static void Place(Transform root, int i, int species, Vector3 pos, System.Random rng)
        {
            switch (species)
            {
                case 0: // árvore grande
                    Tree(root, $"BigTree_{i}", pos, 1.35f, 1.55f, pine: false);
                    break;
                case 1: // média
                    Tree(root, $"MedTree_{i}", pos, 0.95f, 1.1f, pine: false);
                    break;
                case 2: // pinheiro
                    Tree(root, $"Pine_{i}", pos, 1.15f, 0.85f, pine: true);
                    break;
                case 3: // seca
                    Dec(root, PrimitiveType.Cylinder, $"DryTrunk_{i}", pos + Vector3.up * 0.7f,
                        new Vector3(0.18f, 0.7f, 0.18f), CityArtMaterialCategory.Wood);
                    Dec(root, PrimitiveType.Cube, $"DryBranch_{i}", pos + new Vector3(0.35f, 1.2f, 0f),
                        new Vector3(0.7f, 0.08f, 0.08f), CityArtMaterialCategory.Wood);
                    break;
                case 4: // arbusto
                    Dec(root, PrimitiveType.Sphere, $"Bush_{i}", pos + Vector3.up * 0.35f,
                        new Vector3(0.85f, 0.55f, 0.85f), CityArtMaterialCategory.Grass);
                    break;
                case 5: // flores
                {
                    var flower = Dec(root, PrimitiveType.Sphere, $"Flower_{i}", pos + Vector3.up * 0.2f,
                        Vector3.one * 0.28f, CityArtMaterialCategory.Metal);
                    CityArtMaterialLibrary.ApplyVariant(flower.GetComponent<Renderer>(),
                        CityArtMaterialCategory.Roof, new Color(0.85f, 0.35f, 0.55f), 0.9f);
                    break;
                }
                case 6: // samambaia
                    Dec(root, PrimitiveType.Cube, $"Fern_{i}", pos + Vector3.up * 0.2f,
                        new Vector3(0.55f, 0.35f, 0.2f), CityArtMaterialCategory.Grass);
                    break;
                case 7: // musgo
                    Dec(root, PrimitiveType.Cube, $"MossPatch_{i}", pos + Vector3.up * 0.04f,
                        new Vector3(0.9f, 0.06f, 0.7f), CityArtMaterialCategory.Grass);
                    break;
                case 8: // gramínea
                    Dec(root, PrimitiveType.Cube, $"GrassTuft_{i}", pos + Vector3.up * 0.12f,
                        new Vector3(0.35f, 0.22f, 0.35f), CityArtMaterialCategory.Grass);
                    break;
                case 9: // raiz
                    Dec(root, PrimitiveType.Cube, $"Root_{i}", pos + new Vector3(0.2f, 0.06f, 0f),
                        new Vector3(0.8f, 0.08f, 0.18f), CityArtMaterialCategory.Wood);
                    break;
                default: // pedra natural
                    Dec(root, PrimitiveType.Sphere, $"Rock_{i}", pos + Vector3.up * 0.2f,
                        new Vector3(0.7f + (float)rng.NextDouble() * 0.5f, 0.35f, 0.6f),
                        i % 2 == 0 ? CityArtMaterialCategory.StoneDark : CityArtMaterialCategory.StoneLight);
                    break;
            }
        }

        private static void Tree(Transform root, string name, Vector3 pos, float height, float canopy, bool pine)
        {
            Dec(root, PrimitiveType.Cylinder, name + "_Trunk", pos + Vector3.up * height * 0.5f,
                new Vector3(0.22f * canopy, height * 0.5f, 0.22f * canopy), CityArtMaterialCategory.Wood);
            if (pine)
            {
                if (CityArtPass3Config.Enabled && CityArtPass3Config.LayeredVegetationEnabled)
                {
                    for (var layer = 0; layer < CityArtPass3Config.NatureCanopyLayers; layer++)
                    {
                        var t = layer / (float)Math.Max(1, CityArtPass3Config.NatureCanopyLayers - 1);
                        var y = height * 0.75f + layer * 0.45f * height;
                        var w = canopy * (1.05f - t * 0.55f);
                        Dec(root, PrimitiveType.Cylinder, $"{name}_Cone{layer}", pos + Vector3.up * y,
                            new Vector3(w, height * 0.28f, w), CityArtMaterialCategory.Grass);
                    }
                }
                else
                {
                    Dec(root, PrimitiveType.Cylinder, name + "_Cone", pos + Vector3.up * (height + 0.55f),
                        new Vector3(canopy * 0.9f, height * 0.55f, canopy * 0.9f), CityArtMaterialCategory.Grass);
                }
            }
            else if (CityArtPass3Config.Enabled && CityArtPass3Config.LayeredVegetationEnabled)
            {
                Dec(root, PrimitiveType.Sphere, name + "_CanopyA", pos + Vector3.up * (height * 1.05f),
                    new Vector3(canopy * 1.25f, canopy * 0.75f, canopy * 1.1f), CityArtMaterialCategory.Grass);
                Dec(root, PrimitiveType.Sphere, name + "_CanopyB", pos + Vector3.up * (height * 1.35f),
                    new Vector3(canopy * 0.95f, canopy * 0.7f, canopy * 0.95f), CityArtMaterialCategory.Grass);
                Dec(root, PrimitiveType.Sphere, name + "_CanopyC", pos + Vector3.up * (height * 1.55f) + new Vector3(0.15f, 0f, 0.1f),
                    new Vector3(canopy * 0.65f, canopy * 0.55f, canopy * 0.65f), CityArtMaterialCategory.Grass);
            }
            else
            {
                Dec(root, PrimitiveType.Sphere, name + "_Canopy", pos + Vector3.up * (height * 1.15f),
                    new Vector3(canopy * 1.2f, canopy, canopy * 1.2f), CityArtMaterialCategory.Grass);
            }
        }

        private static GameObject Dec(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 position,
            Vector3 scale,
            CityArtMaterialCategory category)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            var ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                go.layer = ignore;
            }

            CityArtMaterialLibrary.Apply(go.GetComponent<Renderer>(), category);
            return go;
        }
    }
}
