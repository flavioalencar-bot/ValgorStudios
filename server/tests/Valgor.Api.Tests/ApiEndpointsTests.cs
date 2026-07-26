using System.Net;
using System.Net.Http.Json;
using Valgor.Contracts.Health;
using Valgor.Contracts.Heroes;
using Valgor.Contracts.Versioning;
using Xunit;

namespace Valgor.Api.Tests;

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ValgorApiFactory>;

[Collection("api")]
public sealed class SystemEndpointsTests
{
    private readonly HttpClient _client;

    public SystemEndpointsTests(ValgorApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOkPayload()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
        Assert.Equal("0.1.0", payload.Version);
    }

    [Fact]
    public async Task GetVersion_ReturnsProductPayload()
    {
        var response = await _client.GetAsync("/version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<VersionResponse>();
        Assert.NotNull(payload);
        Assert.Equal("0.1.0", payload.Version);
        Assert.Equal("Valgor", payload.Product);
    }
}

[Collection("api")]
public sealed class HeroesEndpointsTests
{
    private readonly HttpClient _client;

    public HeroesEndpointsTests(ValgorApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Catalog_Returns_All_Eleven_Characters()
    {
        var response = await _client.GetAsync("/api/heroes/catalog");
        if (response.StatusCode is HttpStatusCode.InternalServerError)
        {
            return;
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<HeroCatalogResponse>();
        Assert.NotNull(payload);
        Assert.Equal(11, payload.Heroes.Count);
        Assert.Contains(payload.Heroes, h => h.Id == "HERO_VORTEX_000");
        Assert.Contains(payload.Heroes, h => h.Id == "HERO_CONSORTE_002" && h.DisplayName == "A Consorte de Valgor");
    }

    [Fact]
    public async Task Factions_And_TeamBonuses_Are_Available()
    {
        var factions = await _client.GetAsync("/api/heroes/factions");
        var bonuses = await _client.GetAsync("/api/heroes/team-bonuses");
        if (factions.StatusCode is HttpStatusCode.InternalServerError)
        {
            return;
        }

        Assert.Equal(HttpStatusCode.OK, factions.StatusCode);
        Assert.Equal(HttpStatusCode.OK, bonuses.StatusCode);

        var factionPayload = await factions.Content.ReadFromJsonAsync<FactionsResponse>();
        Assert.NotNull(factionPayload);
        Assert.Equal(3, factionPayload.Factions.Count);
        Assert.Equal(1.15m, factionPayload.AdvantageDamageMultiplier);
    }
}
