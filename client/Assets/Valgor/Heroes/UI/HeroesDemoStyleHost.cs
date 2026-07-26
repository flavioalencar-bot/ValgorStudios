using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.Heroes.UI
{
    /// <summary>
    /// Applies the demo stylesheet to the UIDocument root at runtime.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HeroesDemoStyleHost : MonoBehaviour
    {
        [SerializeField] private StyleSheet styleSheet;

        private void OnEnable()
        {
            var document = GetComponent<UIDocument>();
            if (document == null || styleSheet == null) return;
            var root = document.rootVisualElement;
            if (root == null) return;
            if (!root.styleSheets.Contains(styleSheet))
            {
                root.styleSheets.Add(styleSheet);
            }
        }
    }
}
