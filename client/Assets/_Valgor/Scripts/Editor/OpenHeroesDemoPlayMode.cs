#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Valgor.EditorTools
{
    /// <summary>
    /// Abre HeroesDemo com o Vortex real (não primitivas da City/WorldMap).
    /// </summary>
    [InitializeOnLoad]
    public static class OpenHeroesDemoPlayMode
    {
        public const string HeroesScene = "Assets/Valgor/Heroes/Scenes/HeroesDemo.unity";
        private const string FlagPath = "Library/ValgorOpenHeroes.flag";
        private static bool _hooked;

        static OpenHeroesDemoPlayMode()
        {
            Hook();
        }

        private static void Hook()
        {
            if (_hooked) return;
            _hooked = true;
            EditorApplication.update += PollFlag;
        }

        [MenuItem("Valgor/Beta/Abrir Heróis — Vortex real (Play)")]
        public static void OpenAndPlay()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EnterHeroesPlay();
        }

        public static void OpenAndPlayFromCommandLine()
        {
            File.WriteAllText(FlagPath, "1");
        }

        private static void PollFlag()
        {
            if (!File.Exists(FlagPath)) return;
            try { File.Delete(FlagPath); } catch { return; }
            EnterHeroesPlay();
        }

        private static void EnterHeroesPlay()
        {
            void Go()
            {
                var scene = EditorSceneManager.OpenScene(HeroesScene, OpenSceneMode.Single);
                Debug.Log($"[Valgor] HeroesDemo (Vortex real) — {scene.path}. Play Mode…");
                EditorApplication.isPlaying = true;
            }

            if (EditorApplication.isPlaying)
            {
                EditorApplication.playModeStateChanged += OnExited;
                EditorApplication.isPlaying = false;

                void OnExited(PlayModeStateChange state)
                {
                    if (state != PlayModeStateChange.EnteredEditMode) return;
                    EditorApplication.playModeStateChanged -= OnExited;
                    EditorApplication.delayCall += Go;
                }

                return;
            }

            Go();
        }
    }
}
#endif
