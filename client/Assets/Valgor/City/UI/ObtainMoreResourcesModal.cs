using System;
using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City.Data;
using Valgor.City.Economy;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Modal Obter mais — inventário de pacotes/baús (sem compra real).</summary>
    public sealed class ObtainMoreResourcesModal
    {
        public const string RootName = "obtain-more-modal";

        private readonly VisualElement _backdrop;
        private readonly VisualElement _root;

        private Action? _onClose;
        private Action<string, int>? _onUse;
        private Action? _onAutoRefill;
        private Action? _onSimulateShortageQa;
        private Action? _onRestoreResourcesQa;

        private ResourceType _resource;
        private long _available;
        private long _required;

        public ObtainMoreResourcesModal(VisualElement parent)
        {
            _backdrop = BuildingUpgradeUxTheme.CreateBackdrop("obtain-more-backdrop", Hide);
            parent.Add(_backdrop);

            _root = new VisualElement { name = RootName };
            BuildingUpgradeUxTheme.ApplyModalShell(_root, 540f);
            _root.RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            parent.Add(_root);
        }

        public VisualElement Root => _root;
        public bool IsVisible => _root.style.display == DisplayStyle.Flex;
        public ResourceType CurrentResource => _resource;

        public void Bind(
            Action onClose,
            Action<string, int> onUse,
            Action onAutoRefill,
            Action? onSimulateShortageQa = null,
            Action? onRestoreResourcesQa = null)
        {
            _onClose = onClose;
            _onUse = onUse;
            _onAutoRefill = onAutoRefill;
            _onSimulateShortageQa = onSimulateShortageQa;
            _onRestoreResourcesQa = onRestoreResourcesQa;
        }

        public void Show(
            ResourceType resource,
            long available,
            long required,
            ResourceItemInventory inventory)
        {
            _resource = resource;
            _available = available;
            _required = required;
            Rebuild(inventory);
            _backdrop.style.display = DisplayStyle.Flex;
            _root.style.display = DisplayStyle.Flex;
            _root.BringToFront();
            BetaJourneyGuide.NotifyCityModalOpen(true, panelOnRight: false);
        }

        public void Refresh(ResourceItemInventory inventory, long available, long required)
        {
            _available = available;
            _required = required;
            if (IsVisible)
            {
                Rebuild(inventory);
            }
        }

        public void Hide()
        {
            _backdrop.style.display = DisplayStyle.None;
            _root.style.display = DisplayStyle.None;
            _root.Clear();
            _onClose?.Invoke();
        }

        public void HideWithoutCallback()
        {
            _backdrop.style.display = DisplayStyle.None;
            _root.style.display = DisplayStyle.None;
            _root.Clear();
        }

        private void Rebuild(ResourceItemInventory inventory)
        {
            _root.Clear();
            var name = BuildingUpgradePresentationBuilder.FriendlyResource(_resource);
            var missing = Math.Max(0, _required - _available);
            var progress = _required <= 0 ? 1f : Mathf.Clamp01((float)_available / _required);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.flexShrink = 0;

            var titles = new VisualElement();
            titles.style.flexGrow = 1;
            var title = new Label($"Obter mais — {name}");
            title.style.color = BetaVisualTheme.AgedGoldBright;
            title.style.fontSize = 17;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            titles.Add(title);

            var amounts = new Label(
                $"{BuildingUpgradePresentationBuilder.FormatAmount(_available)} / {BuildingUpgradePresentationBuilder.FormatAmount(_required)}");
            amounts.style.color = missing > 0 ? BetaVisualTheme.Danger : BetaVisualTheme.Success;
            amounts.style.fontSize = 14;
            amounts.style.marginTop = 2;
            titles.Add(amounts);
            header.Add(titles);
            header.Add(BuildingUpgradeUxTheme.CreateButton("✕", Hide, BuildingUpgradeUxTheme.ButtonKind.Ghost));
            _root.Add(header);

            var barBg = new VisualElement();
            barBg.style.height = 10;
            barBg.style.marginTop = 8;
            barBg.style.backgroundColor = new Color(0.15f, 0.15f, 0.18f);
            barBg.style.flexShrink = 0;
            var barFill = new VisualElement();
            barFill.style.height = 10;
            barFill.style.width = new Length(progress * 100f, LengthUnit.Percent);
            barFill.style.backgroundColor = missing > 0
                ? BuildingUpgradeUxTheme.DeepBlueButton
                : BetaVisualTheme.Success;
            barBg.Add(barFill);
            _root.Add(barBg);

            if (missing > 0)
            {
                var miss = new Label($"Faltam {BuildingUpgradePresentationBuilder.FormatAmount(missing)}");
                miss.style.color = BetaVisualTheme.TextMuted;
                miss.style.fontSize = 12;
                miss.style.marginTop = 4;
                miss.style.flexShrink = 0;
                _root.Add(miss);
            }

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.flexShrink = 1;
            scroll.style.minHeight = 120;
            scroll.style.marginTop = 10;

            var sourcesTitle = new Label("Fontes no inventário");
            sourcesTitle.style.color = BetaVisualTheme.AgedGold;
            sourcesTitle.style.fontSize = 13;
            sourcesTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            scroll.Add(sourcesTitle);

            var stacks = inventory.GetStacksFor(_resource);
            if (stacks.Count == 0)
            {
                var empty = new Label("Nenhum item deste recurso no inventário.");
                empty.style.color = BetaVisualTheme.TextMuted;
                empty.style.fontSize = 12;
                empty.style.marginTop = 6;
                scroll.Add(empty);
            }
            else
            {
                foreach (var stack in stacks)
                {
                    scroll.Add(BuildItemRow(stack));
                }
            }

            var shop = new VisualElement();
            shop.style.marginTop = 14;
            shop.style.paddingLeft = 10;
            shop.style.paddingRight = 10;
            shop.style.paddingTop = 10;
            shop.style.paddingBottom = 10;
            shop.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.9f);
            shop.style.opacity = 0.55f;
            var shopTitle = new Label("Loja de recursos — em breve");
            shopTitle.style.color = BetaVisualTheme.TextMuted;
            shopTitle.style.fontSize = 13;
            shopTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            shop.Add(shopTitle);
            var shopHint = new Label("Ofertas e skins ficam preparadas aqui. Sem compra nesta sprint.");
            shopHint.style.color = BetaVisualTheme.TextMuted;
            shopHint.style.fontSize = 11;
            shopHint.style.marginTop = 4;
            shopHint.style.whiteSpace = WhiteSpace.Normal;
            shop.Add(shopHint);
            scroll.Add(shop);

            _root.Add(scroll);

            var footer = new VisualElement();
            footer.style.flexShrink = 0;
            footer.style.marginTop = 8;
            footer.style.flexDirection = FlexDirection.Column;

            footer.Add(BuildingUpgradeUxTheme.CreateButton(
                "Reabastecimento automático",
                () => _onAutoRefill?.Invoke(),
                BuildingUpgradeUxTheme.ButtonKind.Primary,
                enabled: missing > 0 && inventory.CanAutoRefill(_resource, missing)));

            if (CityProgressionQa.IsActive)
            {
                var qaRow = new VisualElement();
                qaRow.style.flexDirection = FlexDirection.Row;
                qaRow.style.flexWrap = Wrap.Wrap;
                qaRow.style.marginTop = 4;
                if (_onSimulateShortageQa != null)
                {
                    qaRow.Add(BuildingUpgradeUxTheme.CreateButton(
                        "QA: Simular falta",
                        () => _onSimulateShortageQa.Invoke(),
                        BuildingUpgradeUxTheme.ButtonKind.Danger));
                }

                if (_onRestoreResourcesQa != null)
                {
                    qaRow.Add(BuildingUpgradeUxTheme.CreateButton(
                        "QA: Restaurar recursos",
                        () => _onRestoreResourcesQa.Invoke(),
                        BuildingUpgradeUxTheme.ButtonKind.Ghost));
                }

                footer.Add(qaRow);
            }

            footer.Add(BuildingUpgradeUxTheme.CreateButton("Fechar", Hide, BuildingUpgradeUxTheme.ButtonKind.Ghost));
            _root.Add(footer);
        }

        private VisualElement BuildItemRow(ResourceItemStack stack)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 5;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;
            row.style.backgroundColor = BuildingUpgradeUxTheme.RowBg;

            var texts = new VisualElement();
            texts.style.flexGrow = 1;
            texts.style.flexShrink = 1;
            var name = new Label(stack.Definition.DisplayName);
            name.style.color = BetaVisualTheme.TextPrimary;
            name.style.fontSize = 12;
            texts.Add(name);
            var meta = new Label(
                $"×{stack.Quantity}  ·  +{BuildingUpgradePresentationBuilder.FormatAmount(stack.Value)} cada");
            meta.style.color = BetaVisualTheme.TextMuted;
            meta.style.fontSize = 11;
            texts.Add(meta);
            row.Add(texts);

            var itemId = stack.ItemId;
            row.Add(BuildingUpgradeUxTheme.CreateButton(
                "Usar",
                () => _onUse?.Invoke(itemId, 1),
                BuildingUpgradeUxTheme.ButtonKind.Go));

            if (stack.Quantity > 1)
            {
                var multi = Math.Min(stack.Quantity, 5);
                row.Add(BuildingUpgradeUxTheme.CreateButton(
                    $"Usar ×{multi}",
                    () => _onUse?.Invoke(itemId, multi),
                    BuildingUpgradeUxTheme.ButtonKind.Ghost));
            }

            return row;
        }
    }
}
