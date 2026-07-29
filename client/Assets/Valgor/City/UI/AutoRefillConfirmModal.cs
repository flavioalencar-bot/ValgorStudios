using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Data;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Confirmação de reabastecimento automático (sessão: não mostrar de novo).</summary>
    public sealed class AutoRefillConfirmModal
    {
        public const string RootName = "auto-refill-confirm-modal";

        private readonly VisualElement _backdrop;
        private readonly VisualElement _root;
        private Toggle? _dontShowAgain;

        private Action? _onCancel;
        private Action<AutoRefillPlan, bool>? _onConfirm;
        private AutoRefillPlan? _plan;
        private bool _busy;

        /// <summary>Escolha da sessão (não persiste).</summary>
        public static bool SkipConfirmThisSession { get; set; }

        public AutoRefillConfirmModal(VisualElement parent)
        {
            _backdrop = BuildingUpgradeUxTheme.CreateBackdrop("auto-refill-backdrop", Cancel);
            parent.Add(_backdrop);

            _root = new VisualElement { name = RootName };
            BuildingUpgradeUxTheme.ApplyModalShell(_root, 480f);
            _root.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.style.display == DisplayStyle.Flex;

        public void Bind(Action onCancel, Action<AutoRefillPlan, bool> onConfirm)
        {
            _onCancel = onCancel;
            _onConfirm = onConfirm;
        }

        public void Show(AutoRefillPlan plan)
        {
            _plan = plan;
            _busy = false;
            Rebuild(plan);
            _backdrop.style.display = DisplayStyle.Flex;
            _backdrop.BringToFront();
            _root.style.display = DisplayStyle.Flex;
            _root.BringToFront();
        }

        public void Hide()
        {
            _backdrop.style.display = DisplayStyle.None;
            _root.style.display = DisplayStyle.None;
            _root.Clear();
            _plan = null;
            _busy = false;
        }

        private void Cancel()
        {
            Hide();
            _onCancel?.Invoke();
        }

        private void Confirm()
        {
            if (_busy || _plan == null)
            {
                return;
            }

            _busy = true;
            var skip = _dontShowAgain?.value == true;
            if (skip)
            {
                SkipConfirmThisSession = true;
            }

            var plan = _plan;
            Hide();
            _onConfirm?.Invoke(plan, skip);
        }

        private void Rebuild(AutoRefillPlan plan)
        {
            _root.Clear();
            var title = new Label("Reabastecimento automático");
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = 17;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.flexShrink = 0;
            _root.Add(title);

            var subtitle = new Label(
                $"Completar {BuildingUpgradePresentationBuilder.FriendlyResource(plan.ResourceId)} " +
                $"até {BuildingUpgradePresentationBuilder.FormatAmount(plan.RequiredAmount)} necessário");
            subtitle.style.color = BetaVisualTheme.TextMuted;
            subtitle.style.fontSize = 12;
            subtitle.style.marginTop = 4;
            subtitle.style.whiteSpace = WhiteSpace.Normal;
            subtitle.style.flexShrink = 0;
            _root.Add(subtitle);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 80;
            scroll.style.marginTop = 10;

            var listTitle = new Label("Itens que serão consumidos");
            listTitle.style.color = BetaVisualTheme.AgedGold;
            listTitle.style.fontSize = 13;
            listTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            scroll.Add(listTitle);

            foreach (var line in plan.Lines)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.marginTop = 4;
                row.style.paddingLeft = 8;
                row.style.paddingRight = 8;
                row.style.paddingTop = 5;
                row.style.paddingBottom = 5;
                row.style.backgroundColor = BuildingUpgradeUxTheme.RowBg;
                var l = new Label($"{line.DisplayName} ×{line.Quantity}");
                l.style.color = BetaVisualTheme.TextPrimary;
                l.style.fontSize = 12;
                l.style.flexGrow = 1;
                row.Add(l);
                var v = new Label($"+{BuildingUpgradePresentationBuilder.FormatAmount(line.TotalValue)}");
                v.style.color = BetaVisualTheme.Success;
                v.style.fontSize = 12;
                row.Add(v);
                scroll.Add(row);
            }

            _root.Add(scroll);

            var summary = new VisualElement();
            summary.style.flexShrink = 0;
            summary.style.marginTop = 10;
            summary.style.paddingLeft = 10;
            summary.style.paddingRight = 10;
            summary.style.paddingTop = 8;
            summary.style.paddingBottom = 8;
            summary.style.backgroundColor = BuildingUpgradeUxTheme.ScrollInner;

            summary.Add(SummaryLine(
                "Total obtido",
                $"+{BuildingUpgradePresentationBuilder.FormatAmount(plan.TotalObtained)}",
                BetaVisualTheme.Success));
            summary.Add(SummaryLine(
                "Antes",
                BuildingUpgradePresentationBuilder.FormatAmount(plan.BeforeAmount),
                BetaVisualTheme.TextMuted));
            summary.Add(SummaryLine(
                "Depois",
                BuildingUpgradePresentationBuilder.FormatAmount(plan.AfterAmount),
                BetaVisualTheme.TextPrimary));
            if (!plan.CompletesRequirement)
            {
                var warn = new Label("Atenção: inventário insuficiente para completar o requisito.");
                warn.style.color = BetaVisualTheme.Danger;
                warn.style.fontSize = 11;
                warn.style.marginTop = 4;
                warn.style.whiteSpace = WhiteSpace.Normal;
                summary.Add(warn);
            }

            _root.Add(summary);

            _dontShowAgain = new Toggle("Não mostrar novamente nesta sessão");
            _dontShowAgain.style.marginTop = 8;
            _dontShowAgain.style.color = BetaVisualTheme.TextMuted;
            _dontShowAgain.style.flexShrink = 0;
            _root.Add(_dontShowAgain);

            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.justifyContent = Justify.FlexEnd;
            actions.style.flexShrink = 0;
            actions.style.marginTop = 8;
            actions.Add(BuildingUpgradeUxTheme.CreateButton("Cancelar", Cancel, BuildingUpgradeUxTheme.ButtonKind.Ghost));
            actions.Add(BuildingUpgradeUxTheme.CreateButton(
                "Confirmar",
                Confirm,
                BuildingUpgradeUxTheme.ButtonKind.Primary,
                enabled: plan.Lines.Length > 0));
            _root.Add(actions);
        }

        private static VisualElement SummaryLine(string label, string value, Color valueColor)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginTop = 2;
            var l = new Label(label);
            l.style.color = BetaVisualTheme.TextMuted;
            l.style.fontSize = 12;
            row.Add(l);
            var v = new Label(value);
            v.style.color = valueColor;
            v.style.fontSize = 12;
            v.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(v);
            return row;
        }
    }
}
