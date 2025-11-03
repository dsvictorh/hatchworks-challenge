using System.Net;
using System.Text;
using System.Text.Json;
using CartonCaps.Referrals.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CartonCaps.Referrals.Api.Tests;

public class ReferralsEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ReferralsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    // Ensure DB is reset and seeded before each test for isolation
    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReferralsDbContext>();
        db.ReferralEvents.RemoveRange(db.ReferralEvents);
        db.ReferralSessions.RemoveRange(db.ReferralSessions);
        db.ReferralLinks.RemoveRange(db.ReferralLinks);
        db.Referrals.RemoveRange(db.Referrals);
        db.ReferralCodes.RemoveRange(db.ReferralCodes);
        await db.SaveChangesAsync();
        await DbSeeder.SeedAsync(db);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
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

    [Fact]
    public async Task VerifyReferral_InvalidCode_ReturnsNotFound()
    {
        var request = new { ReferralCode = "INVALID999", DeviceId = "device-xyz-12345" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/referrals/verify", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task VerifyReferral_InactiveCode_ReturnsGone()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReferralsDbContext>();
        var code = db.ReferralCodes.First(c => c.Code == "XY7G4D");
        code.Status = "inactive";
        db.SaveChanges();

        var req = new { ReferralCode = "XY7G4D", DeviceId = "device-xyz-12345" };
        var json = JsonSerializer.Serialize(req);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/referrals/verify", content);
        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact]
    public async Task Redeem_SelfReferral_ReturnsForbidden()
    {
        var req = new { ReferralCode = "XY7G4D", RefereeUserId = "user-self-1" };
        var json = JsonSerializer.Serialize(req);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PatchAsync("/api/v1/referrals/redeem", content);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Events_DuplicateEventId_IsIdempotent()
    {
        var payload = new
        {
            Event = "click",
            ReferralCode = "XY7G4D",
            EventId = "evt-dup-001",
            DeviceInfo = new { DeviceId = "dev-1" }
        };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var r1 = await _client.PostAsync("/api/v1/referrals/events", content);
        var r2 = await _client.PostAsync("/api/v1/referrals/events", content);

        Assert.Equal(HttpStatusCode.Accepted, r1.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, r2.StatusCode);
    }

    [Fact]
    public async Task Redeem_WithoutSession_RemainsRegistered()
    {
        var req = new { ReferralCode = "XY7G4D", RefereeUserId = "user-no-session" };
        var json = JsonSerializer.Serialize(req);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var res = await _client.PatchAsync("/api/v1/referrals/redeem", content);
        var body = await res.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("registered", body.ToLower());
    }

    [Fact]
    public async Task GetReferrals_FilterAndPaging_Works()
    {
        // Filter by status=complete and size=1
        var res = await _client.GetAsync("/api/v1/referrals?status=complete&size=1&page=1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var text = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        var items = root.GetProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.Equal(1, items.GetArrayLength());

        var status = items[0].GetProperty("status").GetString();
        Assert.Equal("complete", status);
    }

    [Fact]
    public async Task Redeem_DuplicateAttempt_ReturnsConflict()
    {
        var req = new { ReferralCode = "XY7G4D", RefereeUserId = "user-dup" };
        var json = JsonSerializer.Serialize(req);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var first = await _client.PatchAsync("/api/v1/referrals/redeem", content);
        // second attempt should conflict
        var second = await _client.PatchAsync("/api/v1/referrals/redeem", content);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Link_RateLimited_Returns429()
    {
        var payload = new { Channel = "email" };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpStatusCode? rateLimited = null;
        for (var i = 0; i < 6; i++)
        {
            var res = await _client.PostAsync("/api/v1/referrals/link", content);
            if (res.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimited = res.StatusCode;
                break;
            }
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimited);
    }

    [Fact]
    public async Task Events_ProgressesLifecycle_OpenStatusAfterSequence()
    {
        // Click -> Install -> Open
        async Task PostEvent(string ev)
        {
            var payload = new
            {
                Event = ev, ReferralCode = "XY7G4D", EventId = Guid.NewGuid().ToString("N"),
                DeviceInfo = new { DeviceId = "dev-lc" }
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/v1/referrals/events", content);
        }

        await PostEvent("click");
        await PostEvent("install");
        await PostEvent("open");

        var res = await _client.GetAsync("/api/v1/referrals");
        var body = await res.Content.ReadAsStringAsync();
        // crude assert: ensure the serialized JSON contains "open"
        Assert.Contains("\"open\"", body);
    }

    [Fact]
    public async Task Redeem_WithValidSession_Completes()
    {
        // Create a valid session
        var sessionReq = new { ReferralCode = "XY7G4D", DeviceId = "dev-valid-session" };
        var sjson = JsonSerializer.Serialize(sessionReq);
        var scontent = new StringContent(sjson, Encoding.UTF8, "application/json");
        var sres = await _client.PostAsync("/api/v1/referrals/session", scontent);
        Assert.Equal(HttpStatusCode.Created, sres.StatusCode);

        // Redeem should complete
        var redeem = new { ReferralCode = "XY7G4D", RefereeUserId = "user-with-session" };
        var rjson = JsonSerializer.Serialize(redeem);
        var rcontent = new StringContent(rjson, Encoding.UTF8, "application/json");
        var rres = await _client.PatchAsync("/api/v1/referrals/redeem", rcontent);
        var body2 = await rres.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, rres.StatusCode);
        Assert.Contains("complete", body2.ToLower());
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
