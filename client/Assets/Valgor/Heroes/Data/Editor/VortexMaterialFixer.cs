#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Valgor.Heroes.Characters.Vortex;

namespace Valgor.Heroes.EditorTools
{
    /// <summary>
    /// Fixes magenta/missing materials on Tripo FBX by assigning URP Lit + embedded albedo.
    /// </summary>
    public static class VortexMaterialFixer
    {
        public const string BodyMatPath = VortexAssetPaths.Materials + "/MAT_Vortex_Body.mat";
        public const string BodyTexPath = VortexAssetPaths.Textures + "/Vortex_Body_BaseColor.jpg";

        public static Material EnsureBodyMaterial()
        {
            EnsureBodyTexture();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(BodyMatPath);
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                         ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(shader) { name = "MAT_Vortex_Body" };
                AssetDatabase.CreateAsset(mat, BodyMatPath);
            }
            else if (shader != null && mat.shader != shader)
            {
                mat.shader = shader;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(BodyTexPath);
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            }
            else
            {
                // Fallback tint so we never stay magenta.
                var tint = new Color(0.12f, 0.13f, 0.16f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static void ApplyToHierarchy(GameObject root)
        {
            if (root == null) return;
            var body = EnsureBodyMaterial();
            var sword = AssetDatabase.LoadAssetAtPath<Material>(VortexAssetPaths.Materials + "/MAT_Vortex_Sword.mat")
                        ?? body;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                if (renderer is ParticleSystemRenderer) continue;
                if (renderer.GetComponent<ParticleSystem>() != null) continue;

                var isSword = renderer.name.IndexOf("Sword", System.StringComparison.OrdinalIgnoreCase) >= 0
                              || renderer.transform.root.name.IndexOf("Sword", System.StringComparison.OrdinalIgnoreCase) >= 0
                              || IsUnderNamedAncestor(renderer.transform, "Vortex_DragonSword");
                var mats = renderer.sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    renderer.sharedMaterial = isSword ? sword : body;
                    continue;
                }

                for (var i = 0; i < mats.Length; i++)
                {
                    mats[i] = isSword ? sword : body;
                }

                renderer.sharedMaterials = mats;
            }
        }

        private static bool IsUnderNamedAncestor(Transform t, string name)
        {
            while (t != null)
            {
                if (t.name == name) return true;
                t = t.parent;
            }

            return false;
        }

        public static string EnsureBodyTexture()
        {
            var texturesDir = Path.Combine(Application.dataPath, "Valgor/Heroes/Characters/Vortex/Textures");
            Directory.CreateDirectory(texturesDir);
            var texAbs = Path.Combine(texturesDir, "Vortex_Body_BaseColor.jpg");

            if (!File.Exists(texAbs))
            {
                var fbmDir = Path.Combine(Application.dataPath, "Valgor/Heroes/Characters/Vortex/Models/Vortex_LOD0.fbm");
                string source = null;
                if (Directory.Exists(fbmDir))
                {
                    source = Directory.GetFiles(fbmDir)
                        .FirstOrDefault(f => !f.EndsWith(".meta") &&
                                             (f.IndexOf("basecolor", System.StringComparison.OrdinalIgnoreCase) >= 0
                                              || f.EndsWith(".jpg")
                                              || f.EndsWith(".png")
                                              || f.EndsWith(".jpeg")));
                }

                if (source != null && File.Exists(source))
                {
                    File.Copy(source, texAbs, true);
                }
            }

            if (File.Exists(texAbs))
            {
                AssetDatabase.ImportAsset(BodyTexPath, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(BodyTexPath) as TextureImporter;
                if (importer != null)
                {
                    var dirty = false;
                    if (importer.textureType != TextureImporterType.Default)
                    {
                        importer.textureType = TextureImporterType.Default;
                        dirty = true;
                    }

                    if (!importer.sRGBTexture)
                    {
                        importer.sRGBTexture = true;
                        dirty = true;
                    }

                    if (importer.maxTextureSize > VortexAssetPaths.MaxBodyTextureSize)
                    {
                        importer.maxTextureSize = VortexAssetPaths.MaxBodyTextureSize;
                        dirty = true;
                    }

                    if (dirty) importer.SaveAndReimport();
                }
            }

            return BodyTexPath;
        }
    }
}
#endif
