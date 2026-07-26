using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    /// <summary>
    /// Anexa UIDocument + MainMenuController na cena MainMenu.
    /// </summary>
    public sealed class MainMenuSceneHost : MonoBehaviour
    {
        private void Awake()
        {
            var document = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            var controller = gameObject.GetComponent<MainMenuController>() ?? gameObject.AddComponent<MainMenuController>();
            // Force serialized reference via reflection-free path: controller finds document in its Awake.
            _ = document;
            _ = controller;
        }
    }
}
