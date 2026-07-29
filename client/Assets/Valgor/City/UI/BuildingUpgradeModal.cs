using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Data;
using Valgor.Core;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Modal central premium de Atualizar edifício.</summary>
    public sealed class BuildingUpgradeModal
    {
        public const string RootName = "building-upgrade-modal";

        private readonly VisualElement _backdrop;
        private readonly VisualElement _root;
        private readonly VisualElement _header;
        private readonly VisualElement _body;
        private readonly VisualElement _footer;
        private readonly Label _feedback;

        private Action? _onClose;
        private Action? _onUpgrade;
        private Action? _onInstant;
        private Action<BuildingRequirementView>? _onGo;
        private Action<BuildingRequirementView>? _onSatisfyQa;
        private Action<ResourceRequirementView>? _onObtain;
        private Action? _onInfo;
        private Action? _onReturn;

        public BuildingUpgradeModal(VisualElement parent)
        {
            _backdrop = BuildingUpgradeUxTheme.CreateBackdrop("building-upgrade-backdrop", Hide);
            parent.Add(_backdrop);

            _root = new VisualElement { name = RootName };
            BuildingUpgradeUxTheme.ApplyModalShell(_root, 580f);
            _root.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());

            _header = new VisualElement { name = "upgrade-header" };
            _header.style.flexShrink = 0;
            _header.style.marginBottom = 8;
            _root.Add(_header);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            ValgorResponsiveUi.TightenScrollBody(scroll, 120f);
            _body = new VisualElement { name = "upgrade-body" };
            scroll.Add(_body);
            _root.Add(scroll);

            _feedback = new Label();
            _feedback.style.color = BetaVisualTheme.AgedGoldBright;
            _feedback.style.fontSize = 12;
            _feedback.style.marginTop = 4;
            _feedback.style.whiteSpace = WhiteSpace.Normal;
            _feedback.style.flexShrink = 0;
            _root.Add(_feedback);

            _footer = new VisualElement { name = "upgrade-footer" };
            _footer.style.flexShrink = 0;
            _footer.style.marginTop = 8;
            _footer.style.flexDirection = FlexDirection.Column;
            _root.Add(_footer);

            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.style.display == DisplayStyle.Flex;

        public void Bind(
            Action onClose,
            Action onUpgrade,
            Action onInstant,
            Action<BuildingRequirementView> onGo,
            Action<ResourceRequirementView> onObtain,
            Action onInfo,
            Action<BuildingRequirementView>? onSatisfyQa = null,
            Action? onReturn = null)
        {
            _onClose = onClose;
            _onUpgrade = onUpgrade;
            _onInstant = onInstant;
            _onGo = onGo;
            _onObtain = onObtain;
            _onInfo = onInfo;
            _onSatisfyQa = onSatisfyQa;
            _onReturn = onReturn;
        }

        public void Show(BuildingUpgradePresentation model, string? feedback = null, bool showReturn = false)
        {
            Rebuild(model, feedback, showReturn);
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
            _header.Clear();
            _body.Clear();
            _footer.Clear();
            _onClose?.Invoke();
        }

        public void HideWithoutCallback()
        {
            BuildingUpgradeUxTheme.StopPreview();
            _backdrop.style.display = DisplayStyle.None;
            _root.style.display = DisplayStyle.None;
            _header.Clear();
            _body.Clear();
            _footer.Clear();
        }

        public void SetFeedback(string text) => _feedback.text = text ?? string.Empty;

        private void Rebuild(BuildingUpgradePresentation model, string? feedback, bool showReturn)
        {
            _header.Clear();
            _body.Clear();
            _footer.Clear();
            _feedback.text = feedback ?? string.Empty;

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.alignItems = Align.FlexStart;

            top.Add(BuildingUpgradeUxTheme.CreatePreviewHost(
                model.PreviewLabel,
                model.BuildingId,
                model.CurrentLevel));

            var titles = new VisualElement();
            titles.style.flexGrow = 1;
            titles.style.flexShrink = 1;

            var name = new Label(model.DisplayName);
            name.style.color = BetaVisualTheme.AgedGoldBright;
            name.style.fontSize = 18;
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            titles.Add(name);

            var levelLine = model.IsMaxLevel
                ? new Label($"Nv.{model.CurrentLevel}  ·  Nível Máximo")
                : new Label($"Nv.{model.CurrentLevel}  →  Nv.{model.NextLevel}");
            levelLine.style.color = BetaVisualTheme.TextPrimary;
            levelLine.style.fontSize = 14;
            levelLine.style.marginTop = 2;
            titles.Add(levelLine);

            if (model.IsUpgrading)
            {
                var upgrading = new Label($"Em andamento — resta {model.RemainingUpgradeText}");
                upgrading.style.color = BuildingUpgradeUxTheme.InstantGold;
                upgrading.style.fontSize = 12;
                upgrading.style.marginTop = 2;
                titles.Add(upgrading);
            }

            top.Add(titles);

            var headerButtons = new VisualElement();
            headerButtons.style.flexDirection = FlexDirection.Column;
            headerButtons.Add(BuildingUpgradeUxTheme.CreateButton("ℹ", () => _onInfo?.Invoke(), BuildingUpgradeUxTheme.ButtonKind.Ghost));
            headerButtons.Add(BuildingUpgradeUxTheme.CreateButton("✕", () => Hide(), BuildingUpgradeUxTheme.ButtonKind.Ghost));
            top.Add(headerButtons);
            _header.Add(top);

            if (showReturn && _onReturn != null)
            {
                _header.Add(BuildingUpgradeUxTheme.CreateButton(
                    "← Voltar ao edifício anterior",
                    () => _onReturn.Invoke(),
                    BuildingUpgradeUxTheme.ButtonKind.Go));
            }

            if (!model.IsMaxLevel)
            {
                var benefitBox = BuildingUpgradeUxTheme.CreateBenefitBox();

                var benefitTitle = new Label(model.BenefitTitle);
                benefitTitle.style.color = BetaVisualTheme.AgedGold;
                benefitTitle.style.fontSize = 13;
                benefitTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                benefitBox.Add(benefitTitle);

                var values = new VisualElement();
                values.style.flexDirection = FlexDirection.Row;
                values.style.marginTop = 4;
                var cur = new Label(model.CurrentBenefit);
                cur.style.color = BetaVisualTheme.TextPrimary;
                cur.style.fontSize = 15;
                values.Add(cur);
                if (!string.IsNullOrEmpty(model.BenefitIncrease))
                {
                    var inc = new Label($"  {model.BenefitIncrease}");
                    inc.style.color = BetaVisualTheme.Success;
                    inc.style.fontSize = 15;
                    inc.style.unityFontStyleAndWeight = FontStyle.Bold;
                    values.Add(inc);
                }

                benefitBox.Add(values);

                var desc = new Label(model.BenefitDescription);
                desc.style.color = BetaVisualTheme.TextMuted;
                desc.style.fontSize = 12;
                desc.style.marginTop = 4;
                desc.style.whiteSpace = WhiteSpace.Normal;
                benefitBox.Add(desc);
                _body.Add(benefitBox);
            }

            _body.Add(BuildingUpgradeUxTheme.SectionTitle("Pré-requisitos"));
            if (model.Requirements.Count == 0)
            {
                _body.Add(Muted("Nenhum pré-requisito adicional."));
            }
            else
            {
                foreach (var req in model.Requirements)
                {
                    _body.Add(BuildRequirementRow(req));
                }
            }

            _body.Add(BuildingUpgradeUxTheme.SectionTitle("Recursos"));
            if (model.IsMaxLevel)
            {
                _body.Add(Muted("Nível máximo atingido."));
            }
            else if (model.ResourceCosts.Count == 0)
            {
                _body.Add(Muted("Sem custo de recursos."));
            }
            else
            {
                foreach (var res in model.ResourceCosts)
                {
                    _body.Add(BuildResourceRow(res));
                }
            }

            var builder = new Label(
                $"Construtor: {model.ConstructionUsed}/{model.ConstructionSlots}");
            builder.style.color = BetaVisualTheme.TextMuted;
            builder.style.fontSize = 12;
            builder.style.marginTop = 8;
            _body.Add(builder);

            if (!string.IsNullOrEmpty(model.BlockReason) && !model.IsMaxLevel)
            {
                var block = new Label(model.BlockReason);
                block.style.color = BetaVisualTheme.Danger;
                block.style.fontSize = 12;
                block.style.marginTop = 4;
                block.style.whiteSpace = WhiteSpace.Normal;
                _body.Add(block);
            }

            // Footer
            if (model.IsMaxLevel)
            {
                var max = new Label("Nível Máximo");
                max.style.color = BetaVisualTheme.AgedGoldBright;
                max.style.fontSize = 15;
                max.style.unityFontStyleAndWeight = FontStyle.Bold;
                max.style.unityTextAlign = TextAnchor.MiddleCenter;
                max.style.marginBottom = 6;
                _footer.Add(max);
                _footer.Add(BuildingUpgradeUxTheme.CreateButton("Fechar", Hide, BuildingUpgradeUxTheme.ButtonKind.Ghost));
                return;
            }

            var durationRow = new Label(
                $"Duração: {(int)model.Duration.TotalSeconds}s" +
                (CityProgressionQa.IsActive
                    ? $"  ·  efetiva QA {(int)model.EffectiveDuration.TotalSeconds}s"
                    : string.Empty));
            durationRow.style.color = BetaVisualTheme.TextMuted;
            durationRow.style.fontSize = 12;
            _footer.Add(durationRow);

            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.flexWrap = Wrap.Wrap;
            actions.style.justifyContent = Justify.FlexEnd;
            actions.style.marginTop = 4;

            if (model.CanInstantFinish)
            {
                actions.Add(BuildingUpgradeUxTheme.CreateButton(
                    $"Concluir agora ({model.InstantFinishCost} ◆)",
                    () => _onInstant?.Invoke(),
                    BuildingUpgradeUxTheme.ButtonKind.Instant));
            }

            var upgradeLabel = model.CurrentLevel <= 0 ? "Construir" : "Atualizar";
            actions.Add(BuildingUpgradeUxTheme.CreateButton(
                upgradeLabel,
                () => _onUpgrade?.Invoke(),
                BuildingUpgradeUxTheme.ButtonKind.Primary,
                enabled: model.CanUpgrade));

            _footer.Add(actions);
        }

        private VisualElement BuildRequirementRow(BuildingRequirementView req)
        {
            var row = BuildingUpgradeUxTheme.CreateStatusRow(req.IsSatisfied);
            row.Add(ValgorUiIcons.CreateIconElement(ValgorUiIcons.ForBuildingRequirement(), 24f));

            var texts = new VisualElement();
            texts.style.flexGrow = 1;
            texts.style.flexShrink = 1;
            var name = new Label(req.DisplayName);
            name.style.color = BetaVisualTheme.TextPrimary;
            name.style.fontSize = 13;
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            texts.Add(name);
            var detail = new Label(
                req.RequiredLevel > 0
                    ? $"Exige Nv.{req.RequiredLevel} · atual Nv.{req.CurrentLevel}"
                    : req.Detail);
            detail.style.color = req.IsSatisfied ? BetaVisualTheme.Success : BetaVisualTheme.Danger;
            detail.style.fontSize = 11;
            texts.Add(detail);
            row.Add(texts);

            var mark = new Label(req.IsSatisfied ? "✓" : "✗");
            mark.style.color = req.IsSatisfied ? BetaVisualTheme.Success : BetaVisualTheme.Danger;
            mark.style.fontSize = 15;
            mark.style.unityFontStyleAndWeight = FontStyle.Bold;
            mark.style.marginLeft = 6;
            row.Add(mark);

            if (!req.IsSatisfied && !string.IsNullOrEmpty(req.TargetBuildingId))
            {
                var captured = req;
                row.Add(BuildingUpgradeUxTheme.CreateButton(
                    "Ir",
                    () => _onGo?.Invoke(captured),
                    BuildingUpgradeUxTheme.ButtonKind.Go));

                if (CityProgressionQa.IsActive && req.RequiredLevel > 0 && _onSatisfyQa != null)
                {
                    row.Add(BuildingUpgradeUxTheme.CreateButton(
                        "Atender requisito",
                        () => _onSatisfyQa.Invoke(captured),
                        BuildingUpgradeUxTheme.ButtonKind.Ghost));
                }
            }

            return row;
        }

        private VisualElement BuildResourceRow(ResourceRequirementView res)
        {
            var row = BuildingUpgradeUxTheme.CreateStatusRow(res.IsSatisfied);
            row.Add(ValgorUiIcons.CreateResourceChip(res.ResourceId, 24f));

            var name = new Label(res.DisplayName);
            name.style.color = BetaVisualTheme.TextPrimary;
            name.style.fontSize = 13;
            name.style.flexGrow = 1;
            row.Add(name);

            var amounts = new Label(
                res.IsSatisfied
                    ? $"{BuildingUpgradePresentationBuilder.FormatAmount(res.Available)} / {BuildingUpgradePresentationBuilder.FormatAmount(res.Required)} ✓"
                    : $"{BuildingUpgradePresentationBuilder.FormatAmount(res.Available)} / {BuildingUpgradePresentationBuilder.FormatAmount(res.Required)}");
            amounts.style.color = res.IsSatisfied ? BetaVisualTheme.Success : BetaVisualTheme.Danger;
            amounts.style.fontSize = 12;
            amounts.style.marginRight = 8;
            amounts.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(amounts);

            if (!res.IsSatisfied)
            {
                var captured = res;
                var obtain = BuildingUpgradeUxTheme.CreateButton(
                    "Obter",
                    () => _onObtain?.Invoke(captured),
                    BuildingUpgradeUxTheme.ButtonKind.Go);
                row.Add(obtain);
                row.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.target == row || evt.target == amounts || evt.target == name)
                    {
                        _onObtain?.Invoke(captured);
                    }
                });
            }

            return row;
        }

        private static Label Muted(string text)
        {
            var label = new Label(text);
            label.style.color = BetaVisualTheme.TextMuted;
            label.style.fontSize = 12;
            label.style.marginTop = 2;
            return label;
        }
    }
}
