using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valgor.Editor
{
    /// <summary>
    /// Cena técnica isolada CastleImportValidation — sem City/BuildingView/recolor.
    /// </summary>
    public static class CastleImportValidationSceneBuilder
    {
        public const string ScenePath = "Assets/Valgor/City/Art/Castle/Scenes/CastleImportValidation.unity";
        public const string CaptureFolderAbs =
            @"C:\Valgor_Studio\docs\releases\beta-0.2.4-evidence\isolated-tiers";

        [MenuItem("Valgor/City/Castle/Build Import Validation Scene")]
        public static void BuildFromMenu()
        {
            var ok = Build(out var msg);
            EditorUtility.DisplayDialog("Castle Import Validation", msg, "OK");
            if (!ok)
            {
                Debug.LogError(msg);
            }
        }

        /// <summary>CLI: cria cena + captura tiers isolados (requer gráfico; sem -nographics).</summary>
        public static void BuildAndCaptureCli()
        {
            var code = 0;
            if (!Build(out var msg))
            {
                Debug.LogError(msg);
                code = 1;
            }
            else if (!CaptureAllTiers(out var capMsg))
            {
                Debug.LogError(capMsg);
                code = 1;
            }
            else
            {
                Debug.Log($"[Valgor] CastleImportValidation OK: {msg} | {capMsg}");
            }

            EditorApplication.Exit(code);
        }

        public static bool Build(out string message)
        {
            EnsureFolder("Assets/Valgor/City/Art/Castle/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "CastleImportValidation";

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);

            var camGo = new GameObject("ValidationCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.22f, 0.23f, 0.25f);
            cam.orthographic = false;
            cam.fieldOfView = 35f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.transform.position = new Vector3(10f, 9f, -10f);
            cam.transform.LookAt(new Vector3(0f, 3f, 0f));
            cam.tag = "MainCamera";

            var lightGo = new GameObject("ValidationLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = Color.white;
            lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            var lit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var groundMat = new Material(lit);
            if (groundMat.HasProperty("_BaseColor"))
            {
                groundMat.SetColor("_BaseColor", new Color(0.38f, 0.39f, 0.41f));
            }
            else
            {
                groundMat.color = new Color(0.38f, 0.39f, 0.41f);
            }

            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

            var root = new GameObject("Tiers");
            var missing = 0;
            for (var tier = 1; tier <= 6; tier++)
            {
                var key = $"Valgor/Castle_Tier{tier}";
                var prefab = Resources.Load<GameObject>(key);
                var holder = new GameObject($"Tier{tier}");
                holder.transform.SetParent(root.transform, false);
                holder.transform.localPosition = new Vector3((tier - 1) * 18f, 0f, 0f);
                if (prefab != null)
                {
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    if (inst == null)
                    {
                        inst = Object.Instantiate(prefab);
                    }

                    inst.name = $"Castle_Tier{tier}_Isolated";
                    inst.transform.SetParent(holder.transform, false);
                    inst.transform.localPosition = Vector3.zero;
                    inst.transform.localRotation = Quaternion.identity;
                    inst.transform.localScale = Vector3.one;
                }
                else
                {
                    missing++;
                    Debug.LogError($"[Valgor] Resources missing {key}");
                }
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            message = missing == 0
                ? $"Scene saved: {ScenePath} (6 holders, no City scripts)"
                : $"Scene saved with {missing} missing Resources prefabs";
            return missing == 0;
        }

        public static bool CaptureAllTiers(out string message)
        {
            Directory.CreateDirectory(CaptureFolderAbs);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                message = "Validation camera missing";
                return false;
            }

            var tiersRoot = GameObject.Find("Tiers");
            if (tiersRoot == null)
            {
                message = "Tiers root missing";
                return false;
            }

            var captured = 0;
            for (var tier = 1; tier <= 6; tier++)
            {
                var holderTf = tiersRoot.transform.Find($"Tier{tier}");
                if (holderTf == null)
                {
                    message = $"Tier{tier} holder missing under Tiers";
                    return false;
                }

                for (var i = 0; i < tiersRoot.transform.childCount; i++)
                {
                    var child = tiersRoot.transform.GetChild(i);
                    child.gameObject.SetActive(child == holderTf);
                }

                var home = new Vector3((tier - 1) * 18f, 0f, 0f);
                holderTf.localPosition = Vector3.zero;
                FrameCamera(cam, holderTf);
                var path = Path.Combine(CaptureFolderAbs, $"tier-{tier}-isolated.png");
                if (!RenderToPng(cam, path, 1280, 720))
                {
                    message = $"Capture failed: {path}";
                    return false;
                }

                holderTf.localPosition = home;
                captured++;
            }

            for (var tier = 1; tier <= 6; tier++)
            {
                var child = tiersRoot.transform.Find($"Tier{tier}");
                if (child != null)
                {
                    child.gameObject.SetActive(true);
                    child.localPosition = new Vector3((tier - 1) * 18f, 0f, 0f);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            message = $"Captured {captured}/6 → {CaptureFolderAbs}";
            return captured == 6;
        }

        private static void FrameCamera(Camera cam, Transform target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                cam.transform.position = new Vector3(10f, 9f, -10f);
                cam.transform.LookAt(Vector3.up * 3f);
                return;
            }

            var b = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            var center = b.center;
            var radius = Mathf.Max(b.extents.magnitude, 3.5f);
            var dist = Mathf.Max(7.5f, radius * 1.65f);
            cam.fieldOfView = 32f;
            cam.transform.position = center + new Vector3(dist * 0.85f, dist * 0.62f, -dist * 0.85f);
            cam.transform.LookAt(center + Vector3.up * (b.size.y * 0.15f));
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = Mathf.Max(200f, dist * 8f);
        }

        private static bool RenderToPng(Camera cam, string absPath, int width, int height)
        {
            // batchmode -nographics: URP Camera.Render falha. Preferir Graphics.Blit fallback
            // ou exigir sessão COM gráfico. Tentamos RT simples sem HDR.
            RenderTexture rt = null;
            try
            {
                rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                rt.antiAliasing = 1;
                if (!rt.Create())
                {
                    Debug.LogError("RenderTexture.Create failed — rode captura SEM -nographics");
                    return false;
                }

                var prev = cam.targetTexture;
                var prevActive = RenderTexture.active;
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                cam.targetTexture = prev;
                RenderTexture.active = prevActive;
                var bytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);
                File.WriteAllBytes(absPath, bytes);
                return File.Exists(absPath) && new FileInfo(absPath).Length > 1000;
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
