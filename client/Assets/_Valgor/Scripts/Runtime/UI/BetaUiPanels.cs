using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    /// <summary>
    /// Garante PanelSettings com Theme Style Sheet (sem isso a UI Toolkit avisa e quebra estilos).
    /// </summary>
    public static class BetaUiPanels
    {
        public const string ResourcesPanelSettings = "BetaPanelSettings";
        public const string ResourcesTheme = "UnityDefaultRuntimeTheme";

        private static ThemeStyleSheet? _theme;
        private static PanelSettings? _template;

        public static PanelSettings Resolve(int sortingOrder = 0)
        {
            var settings = CloneOrCreate();
            settings.sortingOrder = sortingOrder;
            ValgorResponsiveUi.ApplyPanelScaleDefaults(settings);
            EnsureTheme(settings);
            return settings;
        }

        /// <summary>
        /// Chamar imediatamente após AddComponent&lt;UIDocument&gt; (antes de acessar rootVisualElement).
        /// </summary>
        public static void ApplyTo(UIDocument document, int sortingOrder = 0)
        {
            if (document == null) return;

            var existing = document.panelSettings;
            if (existing != null && existing.themeStyleSheet != null)
            {
                existing.sortingOrder = Mathf.Max(existing.sortingOrder, sortingOrder);
                ValgorResponsiveUi.ApplyPanelScaleDefaults(existing);
                return;
            }

            // Substitui settings sem tema (AddComponent<UIDocument> costuma criar assim).
            document.panelSettings = Resolve(sortingOrder);
        }

        private static PanelSettings CloneOrCreate()
        {
            _template ??= Resources.Load<PanelSettings>(ResourcesPanelSettings);
            if (_template != null)
            {
                var clone = Object.Instantiate(_template);
                clone.name = "BetaPanelSettings_Runtime";
                return clone;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "BetaPanelSettings_Runtime";
            return settings;
        }

        private static void EnsureTheme(PanelSettings settings)
        {
            if (settings == null || settings.themeStyleSheet != null) return;

            _theme ??= ResolveTheme();
            if (_theme != null)
            {
                settings.themeStyleSheet = _theme;
                return;
            }

            Debug.LogWarning(
                "[Valgor] Theme Style Sheet ausente. Esperado Resources/BetaPanelSettings ou UnityDefaultRuntimeTheme.");
        }

        private static ThemeStyleSheet? ResolveTheme()
        {
            if (_template == null)
            {
                _template = Resources.Load<PanelSettings>(ResourcesPanelSettings);
            }

            if (_template != null && _template.themeStyleSheet != null)
            {
                return _template.themeStyleSheet;
            }

            var fromResources = Resources.Load<ThemeStyleSheet>(ResourcesTheme);
            if (fromResources != null) return fromResources;

            return null;
        }
    }
}
