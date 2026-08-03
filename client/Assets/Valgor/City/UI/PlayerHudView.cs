using UnityEngine;
using UnityEngine.UIElements;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>View UIToolkit do painel do jogador (canto superior esquerdo).</summary>
    public sealed class PlayerHudView : IPlayerHudView
    {
        private readonly VisualElement _root;
        private readonly Label _name;
        private readonly Label _level;
        private readonly Label _status;
        private readonly Label _meta;
        private readonly VisualElement _xpFill;

        public PlayerHudView(VisualElement parent)
        {
            _root = new VisualElement { name = "player-hud" };
            _root.style.position = Position.Absolute;
            _root.style.left = ValgorHudLayoutConfig.Margin;
            _root.style.top = ValgorHudLayoutConfig.Margin;
            _root.style.width = ValgorResponsiveUi.Compact(
                ValgorHudLayoutConfig.PlayerPanelWidth,
                ValgorHudLayoutConfig.PlayerPanelWidth * 0.88f);
            _root.style.minHeight = ValgorResponsiveUi.Compact(
                ValgorHudLayoutConfig.PlayerPanelHeight,
                ValgorHudLayoutConfig.PlayerPanelHeight * 0.9f);
            _root.style.paddingLeft = 10;
            _root.style.paddingRight = 10;
            _root.style.paddingTop = 8;
            _root.style.paddingBottom = 8;
            _root.pickingMode = PickingMode.Ignore;
            ValgorUiTheme.ApplyPanelFrame(_root);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.pickingMode = PickingMode.Ignore;
            _root.Add(row);

            var portrait = new VisualElement { name = "player-portrait" };
            portrait.style.width = 44;
            portrait.style.height = 44;
            portrait.style.marginRight = 10;
            portrait.style.backgroundColor = ValgorUiTheme.StoneDark;
            portrait.style.borderTopWidth = 2;
            portrait.style.borderBottomWidth = 2;
            portrait.style.borderLeftWidth = 2;
            portrait.style.borderRightWidth = 2;
            portrait.style.borderTopColor = ValgorUiTheme.AgedGold;
            portrait.style.borderBottomColor = ValgorUiTheme.AgedGold;
            portrait.style.borderLeftColor = ValgorUiTheme.AgedGold;
            portrait.style.borderRightColor = ValgorUiTheme.AgedGold;
            portrait.pickingMode = PickingMode.Ignore;
            row.Add(portrait);

            var col = new VisualElement();
            col.style.flexGrow = 1;
            col.pickingMode = PickingMode.Ignore;
            row.Add(col);

            _name = new Label("—") { name = "player-name" };
            _name.style.color = ValgorUiTheme.AgedGoldBright;
            _name.style.fontSize = ValgorTypographyConfig.TitleSize;
            _name.style.unityFontStyleAndWeight = FontStyle.Bold;
            _name.pickingMode = PickingMode.Ignore;
            col.Add(_name);

            _level = new Label { name = "player-level" };
            _level.style.color = ValgorUiTheme.TextPrimary;
            _level.style.fontSize = ValgorTypographyConfig.BodySize;
            _level.pickingMode = PickingMode.Ignore;
            col.Add(_level);

            _meta = new Label { name = "player-meta" };
            _meta.style.color = ValgorUiTheme.TextMuted;
            _meta.style.fontSize = ValgorTypographyConfig.SmallSize;
            _meta.pickingMode = PickingMode.Ignore;
            col.Add(_meta);

            _status = new Label { name = "player-status" };
            _status.style.color = ValgorUiTheme.TextSuccess;
            _status.style.fontSize = ValgorTypographyConfig.SmallSize;
            _status.pickingMode = PickingMode.Ignore;
            col.Add(_status);

            var xpTrack = new VisualElement { name = "player-xp-track" };
            xpTrack.style.height = 6;
            xpTrack.style.marginTop = 6;
            xpTrack.style.backgroundColor = new Color(0.05f, 0.05f, 0.06f, 0.9f);
            xpTrack.style.borderTopLeftRadius = 2;
            xpTrack.style.borderTopRightRadius = 2;
            xpTrack.style.borderBottomLeftRadius = 2;
            xpTrack.style.borderBottomRightRadius = 2;
            xpTrack.pickingMode = PickingMode.Ignore;
            _root.Add(xpTrack);

            _xpFill = new VisualElement { name = "player-xp-fill" };
            _xpFill.style.height = 6;
            _xpFill.style.width = Length.Percent(0);
            _xpFill.style.backgroundColor = ValgorUiTheme.AgedGold;
            _xpFill.pickingMode = PickingMode.Ignore;
            xpTrack.Add(_xpFill);

            parent.Add(_root);
        }

        public VisualElement Root => _root;

        public void Bind(PlayerHudViewModel model)
        {
            _name.text = model.DisplayName;
            _level.text = model.LevelLabel;
            _status.text = model.OnlineStatus;
            var meta = string.Empty;
            if (model.PowerAvailable && !string.IsNullOrEmpty(model.PowerLabel))
            {
                meta = model.PowerLabel;
            }

            if (model.VipAvailable && !string.IsNullOrEmpty(model.VipLabel))
            {
                meta = string.IsNullOrEmpty(meta) ? model.VipLabel : meta + " · " + model.VipLabel;
            }

            _meta.text = meta;
            _meta.style.display = string.IsNullOrEmpty(meta) ? DisplayStyle.None : DisplayStyle.Flex;
            _xpFill.style.width = Length.Percent(Mathf.Clamp01(model.XpNormalized) * 100f);
        }
    }
}
