#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Valgor.EditorTools
{
    /// <summary>
    /// Abre a Beta Técnica 0.1 (Bootstrap → Loading → MainMenu) em Play Mode.
    /// </summary>
    [InitializeOnLoad]
    public static class OpenBetaPlayMode
    {
        public const string BootstrapScene = "Assets/_Valgor/Scenes/Bootstrap.unity";
        private const string FlagPath = "Library/ValgorOpenBeta.flag";

        static OpenBetaPlayMode()
        {
            EditorApplication.delayCall += TryConsumeOpenFlag;
        }

        [MenuItem("Valgor/Beta/Abrir Beta Técnica 0.1 (Play)")]
        public static void OpenAndPlay()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.Log("Beta já está em Play Mode.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnterBetaPlay();
        }

        /// <summary>Entry para Unity CLI (sem -batchmode / sem -quit).</summary>
        public static void OpenAndPlayFromCommandLine()
        {
            File.WriteAllText(FlagPath, "1");
            EditorApplication.delayCall += EnterBetaPlay;
        }

        private static void TryConsumeOpenFlag()
        {
            if (!File.Exists(FlagPath)) return;
            File.Delete(FlagPath);
            if (EditorApplication.isPlaying) return;
            EnterBetaPlay();
        }

        private static void EnterBetaPlay()
        {
            if (EditorApplication.isPlaying) return;

            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);
            Debug.Log($"[Valgor] Beta Técnica 0.1 — {scene.path}. Entrando em Play Mode…");
            EditorApplication.isPlaying = true;
        }
    }
}
#endif
