using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.Scenes
{
    /// <summary>
    /// Anexa UIDocument + LoadingScreenController na cena Loading.
    /// </summary>
    public sealed class LoadingSceneHost : MonoBehaviour
    {
        private void Awake()
        {
            var document = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            BetaUiPanels.ApplyTo(document);
            if (gameObject.GetComponent<LoadingScreenController>() == null)
            {
                gameObject.AddComponent<LoadingScreenController>();
            }
        }
    }
}
