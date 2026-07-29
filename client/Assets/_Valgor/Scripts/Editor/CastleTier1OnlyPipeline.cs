using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valgor.Editor
{
    /// <summary>
    /// Pipeline Tier1 ONLY: importa GLB Tripo via glTFast sem rematerializar.
    /// Escala/pivô só no Transform. Sem ApplyColors / City.
    /// </summary>
    public static class CastleTier1OnlyPipeline
    {
        public const string SourceGlb =
            @"C:\Valgor_Studio\production\City\Castle\source\Castle_Tier1.glb";

        public const string ModelPath = "Assets/Valgor/City/Art/Castle/Models/Castle_Tier1.glb";
        public const string PrefabPath = "Assets/Valgor/City/Art/Castle/Prefabs/Castle_Tier1_Visual.prefab";
        public const string ResourcesPrefabPath = "Assets/Valgor/City/Art/Castle/Resources/Valgor/Castle_Tier1.prefab";
        public const string ResourcesKey = "Valgor/Castle_Tier1";
        public const string ScenePath = "Assets/Valgor/City/Scenes/CastleImportValidation.unity";

        public const string EvidenceAbs =
            @"C:\Valgor_Studio\docs\releases\beta-0.2.4-tier1-evidence";

        public const float TargetFootprint = 7.5f;

        [MenuItem("Valgor/City/Castle/Tier1 Only/Build Prefab + Validation Scene + Capture")]
        public static void BuildFromMenu()
        {
            var ok = RunAll(out var msg);
            EditorUtility.DisplayDialog("Castle Tier1 Only", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        public static void BuildCli()
        {
            var code = RunAll(out var msg) ? 0 : 1;
            if (code == 0)
            {
                Debug.Log($"[Valgor] Castle Tier1-only OK: {msg}");
            }
            else
            {
                Debug.LogError($"[Valgor] Castle Tier1-only FAIL: {msg}");
            }

            EditorApplication.Exit(code);
        }

        public static bool RunAll(out string message)
        {
            Directory.CreateDirectory(EvidenceAbs);
            var log = new StringBuilder();

            if (!ImportAndBuildPrefab(out var prefabMsg))
            {
                message = prefabMsg;
                return false;
            }

            log.AppendLine(prefabMsg);

            if (!BuildValidationScene(out var sceneMsg))
            {
                message = sceneMsg;
                return false;
            }

            log.AppendLine(sceneMsg);

            if (!CaptureIsolated(out var capMsg))
            {
                message = capMsg;
                return false;
            }

            log.AppendLine(capMsg);
            File.WriteAllText(Path.Combine(EvidenceAbs, "tier1-pipeline-log.txt"), log.ToString(), Encoding.UTF8);
            message = log.ToString();
            return true;
        }

        public static bool ImportAndBuildPrefab(out string message)
        {
            EnsureFolder("Assets/Valgor/City/Art");
            EnsureFolder("Assets/Valgor/City/Art/Castle");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Models");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Prefabs");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Resources");
            EnsureFolder("Assets/Valgor/City/Art/Castle/Resources/Valgor");

            if (!File.Exists(SourceGlb))
            {
                message = $"Missing source: {SourceGlb}";
                return false;
            }

            // Remover FBX legado
            var fbx = "Assets/Valgor/City/Art/Castle/Models/Castle_Tier1.fbx";
            if (AssetDatabase.LoadAssetAtPath<Object>(fbx) != null)
            {
                AssetDatabase.DeleteAsset(fbx);
            }

            var destAbs = Path.GetFullPath(Path.Combine(Application.dataPath, "Valgor/City/Art/Castle/Models/Castle_Tier1.glb"));
            Directory.CreateDirectory(Path.GetDirectoryName(destAbs)!);
            File.Copy(SourceGlb, destAbs, overwrite: true);
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceUpdate);

            // Prefabs antigos com nested FBX
            if (AssetDatabase.LoadAssetAtPath<Object>(PrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(PrefabPath);
            }

            if (AssetDatabase.LoadAssetAtPath<Object>(ResourcesPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(ResourcesPrefabPath);
            }

            AssetDatabase.Refresh();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                message = $"glTFast failed to import {ModelPath}";
                return false;
            }

            var assetPath = AssetDatabase.GetAssetPath(model);
            if (!assetPath.EndsWith(".glb", System.StringComparison.OrdinalIgnoreCase))
            {
                message = $"Loaded wrong asset type: {assetPath}";
                return false;
            }

            var root = new GameObject("Castle_Tier1_Visual");
            var instance = Object.Instantiate(model);
            instance.name = "Model";
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(col);
            }

            // NÃO rematerializar — preservar materiais/UVs do GLB (glTFast).
            NormalizeTransformOnly(root.transform, TargetFootprint, out var scale, out var footprint, out var height);

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var slotCount = 0;
            var submeshes = 0;
            var matNames = new StringBuilder();
            foreach (var r in renderers)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    submeshes += mf.sharedMesh.subMeshCount;
                }

                var mats = r.sharedMaterials;
                slotCount += mats != null ? mats.Length : 0;
                if (mats != null)
                {
                    foreach (var m in mats)
                    {
                        if (m != null)
                        {
                            matNames.Append(m.name).Append(" [").Append(m.shader != null ? m.shader.name : "?").Append("]; ");
                        }
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, ResourcesPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loaded = Resources.Load<GameObject>(ResourcesKey);
            message =
                $"Prefab OK path={ModelPath} scale={scale:0.###} fp={footprint:0.##}m h={height:0.##}m " +
                $"renderers={renderers.Length} slots={slotCount} submeshes={submeshes} " +
                $"Resources={(loaded != null)} mats={matNames}";
            WriteInspectJson(scale, footprint, height, slotCount, submeshes, matNames.ToString());
            return loaded != null || AssetDatabase.LoadAssetAtPath<GameObject>(ResourcesPrefabPath) != null;
        }

        public static bool BuildValidationScene(out string message)
        {
            EnsureFolder("Assets/Valgor/City/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "CastleImportValidation";

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.58f, 0.58f, 0.6f);

            var camGo = new GameObject("ValidationCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            cam.fieldOfView = 30f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.tag = "MainCamera";

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = Color.white;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(42f, -40f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = Vector3.one * 1.1f;
            var unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var gmat = new Material(unlit ?? Shader.Find("Standard"));
            if (gmat.HasProperty("_BaseColor"))
            {
                gmat.SetColor("_BaseColor", new Color(0.4f, 0.41f, 0.43f));
            }
            else
            {
                gmat.color = new Color(0.4f, 0.41f, 0.43f);
            }

            ground.GetComponent<MeshRenderer>().sharedMaterial = gmat;

            var holder = new GameObject("Tier1");
            var prefab = Resources.Load<GameObject>(ResourcesKey)
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(ResourcesPrefabPath)
                         ?? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                message = "Tier1 prefab missing for validation scene";
                return false;
            }

            var inst = Object.Instantiate(prefab);
            inst.name = "Castle_Tier1_Isolated";
            // Sem BuildingView / seleção / City
            foreach (var mb in inst.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb != null && mb.GetType().Name.Contains("Building"))
                {
                    Object.DestroyImmediate(mb);
                }
            }

            inst.transform.SetParent(holder.transform, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            // Mantém escala do prefab

            FrameCamera(cam, holder.transform);
            EditorSceneManager.SaveScene(scene, ScenePath);
            message = $"Scene OK {ScenePath} (Tier1 only, no City scripts)";
            return true;
        }

        public static bool CaptureIsolated(out string message)
        {
            Directory.CreateDirectory(EvidenceAbs);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            var holder = GameObject.Find("Tier1");
            if (cam == null || holder == null)
            {
                message = "Camera or Tier1 missing in validation scene";
                return false;
            }

            FrameCamera(cam, holder.transform);
            var path = Path.Combine(EvidenceAbs, "tier1-isolated.png");
            if (!RenderToPng(cam, path, 1600, 900))
            {
                message = $"Capture failed (precisa sessão COM gráfico): {path}";
                return false;
            }

            message = $"Capture OK {path} bytes={new FileInfo(path).Length}";
            return true;
        }

        private static void NormalizeTransformOnly(
            Transform root,
            float targetFootprint,
            out float appliedScale,
            out float footprint,
            out float height)
        {
            appliedScale = 1f;
            footprint = 0f;
            height = 0f;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            var rawFp = Mathf.Max(b.size.x, b.size.z);
            if (rawFp < 1e-4f)
            {
                return;
            }

            appliedScale = targetFootprint / rawFp;
            // Escala no filho Model (root fica identity) — Resources.Instantiate preserva.
            foreach (Transform child in root)
            {
                child.localScale = Vector3.one * appliedScale;
            }

            b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            footprint = Mathf.Max(b.size.x, b.size.z);
            height = b.size.y;
            var deltaY = -b.min.y;
            if (Mathf.Abs(deltaY) > 0.0001f)
            {
                foreach (Transform child in root)
                {
                    child.localPosition += new Vector3(0f, deltaY / Mathf.Max(appliedScale, 1e-6f), 0f);
                }
            }
        }

        private static void FrameCamera(Camera cam, Transform target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                cam.transform.position = new Vector3(8f, 7f, -8f);
                cam.transform.LookAt(Vector3.up * 2f);
                return;
            }

            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            var center = b.center;
            var radius = Mathf.Max(b.extents.magnitude, 3f);
            var dist = Mathf.Max(6.5f, radius * 1.55f);
            cam.fieldOfView = 28f;
            cam.transform.position = center + new Vector3(dist * 0.9f, dist * 0.7f, -dist * 0.9f);
            cam.transform.LookAt(center + Vector3.up * (b.size.y * 0.12f));
        }

        private static bool RenderToPng(Camera cam, string absPath, int width, int height)
        {
            RenderTexture rt = null;
            try
            {
                rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                if (!rt.Create())
                {
                    return false;
                }

                var prev = cam.targetTexture;
                var prevRt = RenderTexture.active;
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                cam.targetTexture = prev;
                RenderTexture.active = prevRt;
                File.WriteAllBytes(absPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                return File.Exists(absPath) && new FileInfo(absPath).Length > 2000;
            }
            finally
            {
                if (rt != null)
                {
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }
            }
        }

        private static void WriteInspectJson(
            float scale,
            float footprint,
            float height,
            int slots,
            int submeshes,
            string mats)
        {
            var path = Path.Combine(EvidenceAbs, "tier1-unity-inspect.json");
            File.WriteAllText(
                path,
                "{\n" +
                $"  \"importer\": \"com.unity.cloud.gltfast\",\n" +
                $"  \"source\": \"{SourceGlb.Replace("\\", "\\\\")}\",\n" +
                $"  \"modelPath\": \"{ModelPath}\",\n" +
                $"  \"scale\": {scale.ToString(System.Globalization.CultureInfo.InvariantCulture)},\n" +
                $"  \"footprint_m\": {footprint.ToString(System.Globalization.CultureInfo.InvariantCulture)},\n" +
                $"  \"height_m\": {height.ToString(System.Globalization.CultureInfo.InvariantCulture)},\n" +
                $"  \"material_slots\": {slots},\n" +
                $"  \"submeshes\": {submeshes},\n" +
                $"  \"materials\": \"{mats.Replace("\\", "/").Replace("\"", "'")}\",\n" +
                $"  \"rematerialized\": false,\n" +
                $"  \"applyColors\": false\n" +
                "}\n",
                Encoding.UTF8);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var name = path.Substring(path.LastIndexOf('/') + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
