namespace Valgor.City.UI
{
    /// <summary>ViewModel do painel do jogador — sem acesso a save na View.</summary>
    public sealed class PlayerHudViewModel
    {
        public string DisplayName { get; set; } = "Visitante";
        public int Level { get; set; }
        public string LevelLabel => $"Nível {Level}";
        public string PowerLabel { get; set; } = string.Empty;
        public bool PowerAvailable { get; set; }
        public string VipLabel { get; set; } = string.Empty;
        public bool VipAvailable { get; set; }
        public string OnlineStatus { get; set; } = "Online";
        public float XpNormalized { get; set; }
        public string PortraitKey { get; set; } = "hud.player";
    }

    public interface IPlayerHudView
    {
        void Bind(PlayerHudViewModel model);
    }
}
