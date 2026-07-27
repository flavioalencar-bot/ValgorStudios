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
            BetaUiPanels.ApplyTo(document);
            if (gameObject.GetComponent<MainMenuController>() == null)
            {
                gameObject.AddComponent<MainMenuController>();
            }
        }
    }
}
