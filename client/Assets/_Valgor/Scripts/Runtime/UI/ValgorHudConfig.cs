namespace Valgor.UI
{
    /// <summary>Layout HUD Capital — margens e zonas (1920×1080 de referência). Sem Unity UI.</summary>
    public static class ValgorHudLayoutConfig
    {
        public const int ReferenceWidth = 1920;
        public const int ReferenceHeight = 1080;
        public const float PlayerPanelWidth = 210f;
        public const float PlayerPanelHeight = 92f;
        public const float TopBarHeight = 44f;
        public const float QueuePanelWidth = 168f;
        public const float ChatWidth = 280f;
        public const float ChatHeight = 120f;
        public const float BottomBarHeight = 64f;
        public const float SideShortcutWidth = 48f;
        public const float Margin = 10f;
        public const float BuildingLabelMaxOrtho = 24f;
        public const float BuildingLabelMinOrtho = 9.5f;

        public static readonly int[] SupportedWidths = { 1920, 1600, 1366, 1280, 1080 };
        public static readonly int[] SupportedHeights = { 1080, 900, 768, 720, 640 };

        public static bool IsSupportedResolution(int width, int height)
        {
            for (var i = 0; i < SupportedWidths.Length; i++)
            {
                if (SupportedWidths[i] == width && SupportedHeights[i] == height)
                {
                    return true;
                }
            }

            return width >= 1080 && height >= 640 && width <= 3840 && height <= 2160;
        }
    }

    /// <summary>Formatação de HUD (abreviados) — não altera persistência.</summary>
    public static class ValgorHudFormat
    {
        public static string Abbreviate(long amount)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            if (amount >= 1_000_000_000)
            {
                return (amount / 1_000_000_000d).ToString("0.##", culture) + "B";
            }

            if (amount >= 1_000_000)
            {
                return (amount / 1_000_000d).ToString("0.##", culture) + "M";
            }

            if (amount >= 10_000)
            {
                return (amount / 1_000d).ToString("0.##", culture) + "K";
            }

            return amount.ToString("N0", culture);
        }

        public static string Full(long amount) =>
            amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

        public static string FormatDuration(System.TimeSpan remaining)
        {
            if (remaining < System.TimeSpan.Zero)
            {
                remaining = System.TimeSpan.Zero;
            }

            if (remaining.TotalHours >= 1)
            {
                return $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            }

            return $"{remaining.Minutes:00}:{remaining.Seconds:00}";
        }
    }

    /// <summary>Catálogo de ícones lógicos (chaves) — sprites procedurais em runtime.</summary>
    public static class ValgorIconCatalog
    {
        public const string Food = "res.food";
        public const string Wood = "res.wood";
        public const string Stone = "res.stone";
        public const string Ore = "res.ore";
        public const string Gold = "res.gold";
        public const string Diamonds = "res.diamonds";
        public const string Player = "hud.player";
        public const string QueueBuild = "queue.build";
        public const string QueueTrain = "queue.train";
        public const string QueueResearch = "queue.research";
        public const string QueueHeal = "queue.heal";
        public const string QueueMarch = "queue.march";

        public static readonly string[] ResourceKeys =
        {
            Food, Wood, Stone, Ore, Gold, Diamonds
        };

        public static readonly string[] TopMenuKeys =
        {
            "menu.battlepass", "menu.events", "menu.benefits", "menu.shop"
        };

        public static readonly string[] BottomMenuKeys =
        {
            "nav.items", "nav.mail", "nav.alliance", "nav.heroes", "nav.dragon", "nav.world"
        };
    }

    /// <summary>Tipografia lógica (tamanhos) — fontes do projeto Unity.</summary>
    public static class ValgorTypographyConfig
    {
        public const int TitleSize = 15;
        public const int BodySize = 12;
        public const int SmallSize = 11;
        public const int ChipSize = 12;
        public const int LabelWorldSize = 42;
    }

    /// <summary>Flags Capital Final — o que é real vs shell disabled.</summary>
    public static class CapitalFinalHudConfig
    {
        public const bool Enabled = true;
        public const int OfficialBuildingLabelCount = 17;
        public const bool VipSystemAvailable = false;
        public const bool PlayerPowerSystemAvailable = false;
        public const bool TroopTrainingQueueAvailable = false;
        public const bool HealQueueAvailable = false;
        public const bool BattlePassAvailable = false;
        public const bool EventsHubAvailable = false;
        public const bool BenefitsCenterAvailable = false;
        public const bool ShopAvailable = false;
        public const bool MailAvailable = false;
        public const bool AllianceAvailable = false;
        public const bool ItemsInventoryAvailable = false;
        public const bool RemoteChatAvailable = false;

        public static readonly string[] DisabledTopMenus =
        {
            "Passe de Batalha", "Eventos", "Benefícios", "Loja"
        };

        public static readonly string[] DisabledBottomMenus =
        {
            "Itens", "Correio", "Aliança"
        };

        public static readonly string[] EnabledBottomMenus =
        {
            "Heróis", "Dragão", "Mundo"
        };

        public const string ComingSoon = "Em breve";
    }
}
