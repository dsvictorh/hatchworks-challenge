using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CartonCaps.Referrals.Api.Tests;

public class ReferralsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ReferralsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetReferrals_ReturnsOk()
    {
        var res = await _client.GetAsync("/api/v1/referrals");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task GenerateLink_ValidRequest_ReturnsCreated()
    {
        var request = new { Channel = "email" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/referrals/link", content);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("https://cartoncaps.link/", responseBody);
    }

    [Fact]
    public async Task GenerateLink_InvalidJson_ReturnsBadRequest()
    {
        var invalidJson = "{ invalid json }";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/referrals/link", content);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyReferral_ValidCode_ReturnsOk()
    {
        var request = new { ReferralCode = "XY7G4D", DeviceId = "test-device-123" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/referrals/verify", content);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/referrals")]
    [InlineData("/swagger/index.html")]
    public async Task PublicEndpoints_ReturnSuccessStatusCodes(string url)
    {
        // Just basic smoke tests for public endpoints
        var response = await _client.GetAsync(url);
        
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected success status code for {url} but got {response.StatusCode}");
    }

    [Fact]
    public async Task ShareMessage_EmailChannel_HasSubject()
    {
        var req = new { Channel = "email", Locale = "en" };
        var json = JsonSerializer.Serialize(req);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/referrals/share-message", content);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("subject", body.ToLower());
    }
}
