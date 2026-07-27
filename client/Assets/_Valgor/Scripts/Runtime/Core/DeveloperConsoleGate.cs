using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.Core
{
    /// <summary>
    /// Esconde o Development Console na experiência do jogador.
    /// Só reabre com -showDevConsole/-debug ou F10 (builds de desenvolvimento).
    /// </summary>
    public sealed class DeveloperConsoleGate : MonoBehaviour
    {
        private const Key ToggleKey = Key.F10;
        private bool _playerAllowed;

        public static void Install()
        {
            if (Object.FindFirstObjectByType<DeveloperConsoleGate>() != null)
            {
                return;
            }

            var go = new GameObject(nameof(DeveloperConsoleGate));
            DontDestroyOnLoad(go);
            go.AddComponent<DeveloperConsoleGate>();
        }

        private void Awake()
        {
            _playerAllowed = HasExplicitDebugFlag();
            Apply(_playerAllowed);
        }

        private void Update()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[ToggleKey].wasPressedThisFrame)
            {
                _playerAllowed = !_playerAllowed;
                Apply(_playerAllowed);
                Debug.Log($"[Valgor.DevConsole] {(_playerAllowed ? "visível" : "oculto")} (F10).");
            }
#endif
        }

        private static bool HasExplicitDebugFlag()
        {
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (string.Equals(arg, "-showDevConsole", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "-debug", System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Apply(bool visible)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.developerConsoleEnabled = visible;
            Debug.developerConsoleVisible = visible;
#else
            Debug.developerConsoleEnabled = false;
            Debug.developerConsoleVisible = false;
#endif
            if (!visible)
            {
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            }
        }
    }
}
