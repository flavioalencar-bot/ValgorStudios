using Valgor.City.Buildings;
using Valgor.City.Visual;
using Valgor.UI;
using Valgor.UI.Chat;
using Xunit;

namespace Valgor.GameLogic.Tests;

public sealed class CapitalFinalHudTests
{
    [Fact]
    public void Config_IsEnabled_AndOfficialLabelCountIs17()
    {
        Assert.True(CapitalFinalHudConfig.Enabled);
        Assert.Equal(17, CapitalFinalHudConfig.OfficialBuildingLabelCount);
        Assert.Equal(17, CityCapitalFinalConfig.OfficialLabelCount);
        Assert.Equal(BuildingIdMigration.OfficialCount, CapitalFinalHudConfig.OfficialBuildingLabelCount);
    }

    [Fact]
    public void MissingModules_AreDocumentedAsDisabled()
    {
        Assert.False(CapitalFinalHudConfig.VipSystemAvailable);
        Assert.False(CapitalFinalHudConfig.PlayerPowerSystemAvailable);
        Assert.False(CapitalFinalHudConfig.TroopTrainingQueueAvailable);
        Assert.False(CapitalFinalHudConfig.MailAvailable);
        Assert.False(CapitalFinalHudConfig.AllianceAvailable);
        Assert.False(CapitalFinalHudConfig.RemoteChatAvailable);
        Assert.Contains("Passe de Batalha", CapitalFinalHudConfig.DisabledTopMenus);
        Assert.Contains("Correio", CapitalFinalHudConfig.DisabledBottomMenus);
        Assert.Equal("Em breve", CapitalFinalHudConfig.ComingSoon);
    }

    [Fact]
    public void ResourceChips_KeysCoverSixOfficialResources()
    {
        Assert.Equal(6, ValgorIconCatalog.ResourceKeys.Length);
        Assert.Contains(ValgorIconCatalog.Food, ValgorIconCatalog.ResourceKeys);
        Assert.Contains(ValgorIconCatalog.Diamonds, ValgorIconCatalog.ResourceKeys);
    }
}

public sealed class HudDataBindingTests
{
    [Fact]
    public void HudFormat_AbbreviatesWithoutChangingFullPrecisionString()
    {
        Assert.Equal("1.250", ValgorHudFormat.Full(1250));
        Assert.Equal("1.250", ValgorHudFormat.Abbreviate(1250));
        Assert.Equal("12,5K", ValgorHudFormat.Abbreviate(12_500));
        Assert.Equal("1,25M", ValgorHudFormat.Abbreviate(1_250_000));
        Assert.Equal("1.250.000", ValgorHudFormat.Full(1_250_000));
    }

    [Fact]
    public void LocalChat_UsesSystemOnly_NoFakePlayers()
    {
        var chat = new LocalSystemChatProvider();
        Assert.Contains("Sistema", chat.Channels);
        Assert.Contains("Aliança", chat.Channels);
        chat.PushSystem("Evento local: construção concluída.");
        var msgs = chat.GetMessages("Sistema", 10);
        Assert.NotEmpty(msgs);
        Assert.All(msgs, m => Assert.Equal("Sistema", m.Author));
    }
}

public sealed class CityBuildingWorldLabelTests
{
    [Fact]
    public void OfficialBuildings_RemainExactly17_ForLabels()
    {
        Assert.Equal(17, BuildingIdMigration.OfficialCount);
        Assert.Equal(17, BuildingCatalog.All.Count);
        Assert.Equal(CapitalFinalHudConfig.OfficialBuildingLabelCount, BuildingIdMigration.OfficialCount);
        Assert.Empty(CityCapitalLayoutConfig.ValidateBlueprintInvariants());
    }

    [Fact]
    public void LabelOrthoWindow_IsSane()
    {
        Assert.True(ValgorHudLayoutConfig.BuildingLabelMinOrtho < ValgorHudLayoutConfig.BuildingLabelMaxOrtho);
        Assert.InRange(ValgorHudLayoutConfig.BuildingLabelMinOrtho, 8f, 12f);
        Assert.InRange(ValgorHudLayoutConfig.BuildingLabelMaxOrtho, 16f, 22f);
    }
}

public sealed class ValgorUiThemeTests
{
    [Fact]
    public void Typography_AndLayout_Reference1920x1080()
    {
        Assert.Equal(1920, ValgorHudLayoutConfig.ReferenceWidth);
        Assert.Equal(1080, ValgorHudLayoutConfig.ReferenceHeight);
        Assert.True(ValgorTypographyConfig.TitleSize > ValgorTypographyConfig.SmallSize);
        Assert.True(ValgorHudLayoutConfig.PlayerPanelWidth > 100f);
        Assert.True(ValgorHudLayoutConfig.ChatWidth > 100f);
    }

    [Fact]
    public void IconCatalog_BottomAndTopMenus_AreDeclared()
    {
        Assert.Equal(4, ValgorIconCatalog.TopMenuKeys.Length);
        Assert.Equal(6, ValgorIconCatalog.BottomMenuKeys.Length);
    }
}

public sealed class CapitalFinalLayoutTests
{
    [Fact]
    public void SupportedResolutions_IncludeRequiredSet()
    {
        Assert.True(ValgorHudLayoutConfig.IsSupportedResolution(1920, 1080));
        Assert.True(ValgorHudLayoutConfig.IsSupportedResolution(1600, 900));
        Assert.True(ValgorHudLayoutConfig.IsSupportedResolution(1366, 768));
        Assert.True(ValgorHudLayoutConfig.IsSupportedResolution(1280, 720));
        Assert.True(ValgorHudLayoutConfig.IsSupportedResolution(1080, 640));
        Assert.False(ValgorHudLayoutConfig.IsSupportedResolution(800, 480));
    }

    [Fact]
    public void UrbanDensity_DoesNotCreateCanonicalIds()
    {
        Assert.True(CityCapitalFinalConfig.UrbanDensityEnabled);
        Assert.False(BuildingIdMigration.IsCanonical("CottageRing_0"));
        Assert.False(BuildingIdMigration.IsCanonical("UrbanDensity"));
        Assert.True(BuildingIdMigration.IsCanonical("castle"));
    }

    [Fact]
    public void Horizon_MountainBudget_IsRaised()
    {
        Assert.Equal(16, CityCapitalFinalConfig.HorizonMountainCount);
        Assert.True(CityCapitalFinalConfig.CinematicLightingEnabled);
    }
}

public sealed class HudResponsiveTests
{
    [Fact]
    public void AspectRatios_RemainWidescreenFamily()
    {
        for (var i = 0; i < ValgorHudLayoutConfig.SupportedWidths.Length; i++)
        {
            var w = ValgorHudLayoutConfig.SupportedWidths[i];
            var h = ValgorHudLayoutConfig.SupportedHeights[i];
            Assert.True(w > h);
            Assert.InRange(w / (double)h, 1.5, 1.8);
        }
    }
}
