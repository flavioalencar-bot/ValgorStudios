using UnityEngine;
using UnityEngine.InputSystem;

namespace Valgor.Input
{
    [CreateAssetMenu(menuName = "Valgor/Input Reader", fileName = "ValgorInputReader")]
    public sealed class InputReader : ScriptableObject
    {
        [SerializeField] private InputActionAsset actions;

        public InputAction GetAction(string mapName, string actionName)
        {
            return actions == null ? null : actions.FindActionMap(mapName, true)?.FindAction(actionName, true);
        }

        private void OnEnable() => actions?.Enable();
        private void OnDisable() => actions?.Disable();
    }
}
