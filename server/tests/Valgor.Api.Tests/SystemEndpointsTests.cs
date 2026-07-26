using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Valgor.Contracts.Health;
using Valgor.Contracts.Versioning;

namespace Valgor.Api.Tests;

public sealed class SystemEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SystemEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(_ => { }).CreateClient();
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
