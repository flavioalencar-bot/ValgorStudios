using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Data;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Modal central de Detalhes do edifício.</summary>
    public sealed class BuildingDetailsModal
    {
        public const string RootName = "building-details-modal";

        private readonly VisualElement _backdrop;
        private readonly VisualElement _root;
        private Action? _onClose;
        private Action? _onUpgrade;

        public BuildingDetailsModal(VisualElement parent)
        {
            _backdrop = BuildingUpgradeUxTheme.CreateBackdrop("building-details-backdrop", Hide);
            parent.Add(_backdrop);

            _root = new VisualElement { name = RootName };
            BuildingUpgradeUxTheme.ApplyModalShell(_root, 520f);
            _root.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.style.display == DisplayStyle.Flex;

        public void Bind(Action onClose, Action? onUpgrade = null)
        {
            _onClose = onClose;
            _onUpgrade = onUpgrade;
        }

        public void Show(BuildingDetailsPresentation model)
        {
            _root.Clear();

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.FlexStart;
            header.style.flexShrink = 0;
            header.Add(BuildingUpgradeUxTheme.CreatePreviewHost(
                model.PreviewLabel,
                model.BuildingId,
                model.Level));

            var titles = new VisualElement();
            titles.style.flexGrow = 1;
            var name = new Label(model.DisplayName);
            name.style.color = BetaVisualTheme.AgedGoldBright;
            name.style.fontSize = 18;
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            titles.Add(name);
            var level = new Label($"Nível {model.Level} / {model.MaxLevel}");
            level.style.color = BetaVisualTheme.TextPrimary;
            level.style.fontSize = 14;
            level.style.marginTop = 2;
            titles.Add(level);
            if (!string.IsNullOrEmpty(model.PowerText))
            {
                var power = new Label(model.PowerText);
                power.style.color = BuildingUpgradeUxTheme.InstantGold;
                power.style.fontSize = 12;
                power.style.marginTop = 4;
                titles.Add(power);
            }

            header.Add(titles);
            header.Add(BuildingUpgradeUxTheme.CreateButton("✕", Hide, BuildingUpgradeUxTheme.ButtonKind.Ghost));
            _root.Add(header);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 100;
            scroll.style.marginTop = 10;

            if (!string.IsNullOrEmpty(model.Function))
            {
                scroll.Add(BuildingUpgradeUxTheme.SectionTitle("Função"));
                var fn = new Label(model.Function);
                fn.style.color = BetaVisualTheme.TextPrimary;
                fn.style.fontSize = 13;
                fn.style.whiteSpace = WhiteSpace.Normal;
                fn.style.marginBottom = 8;
                scroll.Add(fn);
            }

            scroll.Add(BuildingUpgradeUxTheme.SectionTitle("Atributos"));

            foreach (var attr in model.Attributes)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.marginTop = 4;
                row.style.paddingLeft = 10;
                row.style.paddingRight = 10;
                row.style.paddingTop = 6;
                row.style.paddingBottom = 6;
                row.style.backgroundColor = BuildingUpgradeUxTheme.RowBg;
                row.style.borderLeftWidth = 2;
                row.style.borderLeftColor = BuildingUpgradeUxTheme.FrameOuter;
                var l = new Label(attr.Label);
                l.style.color = BetaVisualTheme.TextMuted;
                l.style.fontSize = 12;
                l.style.flexGrow = 1;
                row.Add(l);
                var v = new Label(attr.Value);
                v.style.color = BetaVisualTheme.TextPrimary;
                v.style.fontSize = 13;
                v.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(v);
                scroll.Add(row);
            }

            if (!string.IsNullOrEmpty(model.Narrative))
            {
                scroll.Add(BuildingUpgradeUxTheme.SectionTitle("Descrição"));
                var nar = new Label(model.Narrative);
                nar.style.color = BetaVisualTheme.TextPrimary;
                nar.style.fontSize = 12;
                nar.style.whiteSpace = WhiteSpace.Normal;
                nar.style.marginTop = 4;
                scroll.Add(nar);
            }

            _root.Add(scroll);

            var footer = new VisualElement();
            footer.style.flexShrink = 0;
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.FlexEnd;
            footer.style.marginTop = 10;
            if (_onUpgrade != null)
            {
                footer.Add(BuildingUpgradeUxTheme.CreateButton(
                    "Atualizar",
                    () =>
                    {
                        Hide();
                        _onUpgrade.Invoke();
                    },
                    BuildingUpgradeUxTheme.ButtonKind.Primary));
            }

            footer.Add(BuildingUpgradeUxTheme.CreateButton("Fechar", Hide, BuildingUpgradeUxTheme.ButtonKind.Ghost));
            _root.Add(footer);

            _backdrop.style.display = DisplayStyle.Flex;
            _root.style.display = DisplayStyle.Flex;
            _root.BringToFront();
            BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: false);
        }

        public void Hide()
        {
            BuildingUpgradeUxTheme.StopPreview();
            _backdrop.style.display = DisplayStyle.None;
            _root.style.display = DisplayStyle.None;
            _root.Clear();
            _onClose?.Invoke();
        }

        public void HideWithoutCallback()
        {
            BuildingUpgradeUxTheme.StopPreview();
            _backdrop.style.display = DisplayStyle.None;
            _root.style.display = DisplayStyle.None;
            _root.Clear();
        }
    }
}
