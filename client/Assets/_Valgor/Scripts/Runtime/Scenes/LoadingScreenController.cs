using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.Scenes
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        private ProgressBar progressBar;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
            progressBar = document.rootVisualElement.Q<ProgressBar>("loading-progress");
        }

        public void SetProgress(float value)
        {
            if (progressBar != null)
                progressBar.value = Mathf.Clamp01(value) * 100f;
        }
    }
}
