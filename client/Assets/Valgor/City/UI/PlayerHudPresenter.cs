using Valgor.City.Core;
using Valgor.Core;
using Valgor.UI;

namespace Valgor.City.UI
{
    /// <summary>Monta PlayerHudViewModel a partir de perfil e cidade reais.</summary>
    public sealed class PlayerHudPresenter
    {
        private readonly CityController _city;
        private readonly IPlayerHudView _view;
        private readonly PlayerHudViewModel _model = new();

        public PlayerHudPresenter(CityController city, IPlayerHudView view)
        {
            _city = city;
            _view = view;
        }

        public PlayerHudViewModel Model => _model;

        public void Refresh()
        {
            _model.DisplayName = LocalPlayerProfile.HasProfile
                ? LocalPlayerProfile.DisplayName
                : "Visitante";
            _model.Level = _city.GetCastleLevel();
            _model.OnlineStatus = "Online";
            _model.XpNormalized = 0f;
            _model.VipAvailable = CapitalFinalHudConfig.VipSystemAvailable;
            _model.VipLabel = string.Empty;
            _model.PowerAvailable = CapitalFinalHudConfig.PlayerPowerSystemAvailable;
            _model.PowerLabel = string.Empty;
            _model.PortraitKey = ValgorIconCatalog.Player;
            _view.Bind(_model);
        }
    }
}
