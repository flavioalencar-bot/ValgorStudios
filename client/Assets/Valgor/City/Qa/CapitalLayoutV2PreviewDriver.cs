using System.Collections;
using System.IO;
using UnityEngine;
using Valgor.City.Visual;
using Valgor.Core;

namespace Valgor.City.Qa
{
    /// <summary>
    /// Preview do Capital Layout V2: anchors + captura de evidência.
    /// Não migra save; não promove V2 a oficial.
    /// </summary>
    public sealed class CapitalLayoutV2PreviewDriver : MonoBehaviour
    {
        public static void EnsureAndBegin()
        {
            if (!CityProgressionQa.IsLayoutV2Preview)
            {
                return;
            }

            var host = FindFirstObjectByType<CapitalLayoutV2PreviewDriver>();
            if (host == null)
            {
                var go = new GameObject(nameof(CapitalLayoutV2PreviewDriver));
                host = go.AddComponent<CapitalLayoutV2PreviewDriver>();
            }

            host.Begin();
        }

        public void Begin() => StartCoroutine(Run());

        private IEnumerator Run()
        {
            Directory.CreateDirectory(CityProgressionQa.LayoutV2EvidenceDir);
            yield return null;
            yield return new WaitForSecondsRealtime(1.2f);

            var cam = FindFirstObjectByType<Valgor.City.Camera.CityCameraController>();
            cam?.ReleaseOverviewHold();
            cam?.ResetToOverview();
            yield return new WaitForSecondsRealtime(0.6f);

            // Overview limpo (sem markers) antes dos anchors.
            Capture("02-v2-overview.png");
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.4f);

            BuildAnchorOverlay();
            yield return new WaitForSecondsRealtime(0.35f);
            Capture("03-v2-anchors.png");
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.35f);

            if (cam != null)
            {
                var castle = CityLayout.WorldPosition("castle");
                cam.FocusOn(castle, 0.4f, orthographicSize: 14f);
                yield return new WaitForSecondsRealtime(0.7f);
                Capture("04-castle-axis.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(0.35f);

                var tower = CityLayout.WorldPosition("dragon-tower");
                cam.FocusOn(tower, 0.4f, orthographicSize: 13f);
                yield return new WaitForSecondsRealtime(0.7f);
                Capture("05-dragon-tower-ne.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(0.35f);

                cam.FocusOn(CityLayout.WorldPosition("market"), 0.35f, orthographicSize: 12f);
                yield return new WaitForSecondsRealtime(0.6f);
                Capture("06-market-south-center.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(0.35f);

                cam.FocusOn(
                    (CityLayout.WorldPosition("arena") + CityLayout.WorldPosition("training-ground")) * 0.5f,
                    0.4f,
                    orthographicSize: 16f);
                yield return new WaitForSecondsRealtime(0.6f);
                Capture("07-military-sector.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(0.35f);

                cam.FocusOn(
                    (CityLayout.WorldPosition("temple") + CityLayout.WorldPosition("hospital")) * 0.5f,
                    0.4f,
                    orthographicSize: 15f);
                yield return new WaitForSecondsRealtime(0.6f);
                Capture("08-sacred-sector.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(0.35f);

                cam.FocusOn(CityLayout.WorldPosition("wall"), 0.35f, orthographicSize: 14f);
                yield return new WaitForSecondsRealtime(0.6f);
                Capture("09-wall-organic.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(0.35f);

                cam.ResetToOverview();
                yield return new WaitForSecondsRealtime(0.5f);
            }

            Screen.SetResolution(1080, 640, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.4f);
            Capture("10-overview-1080x640.png");
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.4f);

            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.4f);
            Capture("11-overview-1920x1080.png");
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.5f);

            Debug.Log(
                "[Valgor.QA] Capital Layout V2 Preview capturas OK → " +
                CityProgressionQa.LayoutV2EvidenceDir);
            Debug.Log("[Valgor.QA] Status: Capital Layout V2 disponível para aprovação visual.");
        }

        private static void Capture(string fileName)
        {
            var path = Path.Combine(CityProgressionQa.LayoutV2EvidenceDir, fileName);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[Valgor.QA] Screenshot {path}");
        }

        private void BuildAnchorOverlay()
        {
            var root = new GameObject("LayoutV2Anchors").transform;
            root.SetParent(transform, false);
            foreach (var pair in CityCapitalLayoutConfig.All)
            {
                var p = pair.Value.Position;
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = $"Anchor_{pair.Key}";
                Destroy(marker.GetComponent<Collider>());
                marker.transform.SetParent(root, false);
                marker.transform.position = new Vector3(p.X, p.Y + 1.2f, p.Z);
                marker.transform.localScale = Vector3.one * 0.85f;
                CityVisualMaterials.Apply(
                    marker.GetComponent<Renderer>(),
                    new Color(1f, 0.85f, 0.2f, 0.95f));

                var labelGo = new GameObject("Id");
                labelGo.transform.SetParent(marker.transform, false);
                labelGo.transform.localPosition = Vector3.up * 1.4f;
                var tm = labelGo.AddComponent<TextMesh>();
                tm.text = pair.Key;
                tm.characterSize = 0.18f;
                tm.fontSize = 48;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.color = Color.white;
            }
        }
    }
}
