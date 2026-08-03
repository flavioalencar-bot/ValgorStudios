using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using Valgor.Core;

namespace Valgor.City.Visual
{
    /// <summary>Amostra FPS no boot QA Art Pass 2 e grava evidência de profiler leve.</summary>
    public sealed class CityArtPass2PerfProbe : MonoBehaviour
    {
        public static void Ensure()
        {
            if (!CityProgressionQa.IsActive)
            {
                return;
            }

            if (FindFirstObjectByType<CityArtPass2PerfProbe>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(CityArtPass2PerfProbe));
            DontDestroyOnLoad(host);
            host.AddComponent<CityArtPass2PerfProbe>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(2f);
            var frames = 0;
            var sum = 0f;
            var min = float.MaxValue;
            var window = 4f;
            var t = 0f;
            while (t < window)
            {
                var dt = Time.unscaledDeltaTime;
                if (dt > 0.0001f)
                {
                    var fps = 1f / dt;
                    sum += fps;
                    frames++;
                    if (fps < min)
                    {
                        min = fps;
                    }
                }

                t += dt;
                yield return null;
            }

            var avg = frames > 0 ? sum / frames : 0f;
            var tris = 0;
            var batches = 0;
#if UNITY_2022_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            // Métricas aproximadas via contagem de renderers ativos.
#endif
            var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            batches = renderers.Length;
            for (var i = 0; i < renderers.Length; i++)
            {
                var mf = renderers[i].GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    tris += mf.sharedMesh.triangles.Length / 3;
                }
            }

            var dir = Path.Combine(Application.dataPath, "..", "evidence", "capital-artpass2");
            Directory.CreateDirectory(dir);
            var sb = new StringBuilder();
            sb.AppendLine("# Art Pass 2.0 — Profiler leve (runtime QA)");
            sb.AppendLine($"quality={CityAmbientQualitySettings.Current}");
            sb.AppendLine($"fps_avg={avg:0.0}");
            sb.AppendLine($"fps_min={min:0.0}");
            sb.AppendLine($"drawish_renderers={batches}");
            sb.AppendLine($"triangles_est={tris}");
            sb.AppendLine($"mem_mono_mb={System.GC.GetTotalMemory(false) / (1024f * 1024f):0.0}");
            sb.AppendLine("note=Amostra runtime; Unity Profiler Editor recomendado para batches/draw calls oficiais.");
            File.WriteAllText(Path.Combine(dir, "profiler-runtime.md"), sb.ToString());
            Debug.Log($"[Valgor.QA] ArtPass2 perf avg={avg:0.0} min={min:0.0} tris~{tris}");
        }
    }
}
