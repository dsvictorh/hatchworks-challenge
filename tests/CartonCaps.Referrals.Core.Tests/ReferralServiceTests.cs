using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using CartonCaps.Referrals.Infrastructure.Persistence;
using CartonCaps.Referrals.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CartonCaps.Referrals.Core.Tests;

public class ReferralServiceTests
{
    [Fact]
    public void GenerateLink_ReturnsValidUrl()
    {
        var db = new InMemoryDb();
        SeedData.Load(db);
        var logger = NullLogger<ReferralService>.Instance;
        IReferralService svc = new ReferralService(db, logger);

        var res = svc.GenerateLink("usr_referrer_001", "sms");
        res.ReferralLink.Should().StartWith("https://cartoncaps.link/");
    }

    [Fact]
    public void GetMyReferrals_WithValidUser_ReturnsReferrals()
    {
        // Arrange
        var db = new InMemoryDb();
        SeedData.Load(db);
        var svc = new ReferralService(db, NullLogger<ReferralService>.Instance);

        // Act
        var result = svc.GetMyReferrals("usr_referrer_001");

        // Assert
        result.Should().NotBeNull();
        result.ReferralCode.Should().NotBeEmpty();
        result.Summary.Should().NotBeNull();
        result.Summary.Total.Should().BeGreaterThanOrEqualTo(0);
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public void Verify_InvalidCode_ReturnsFalse()
    {
        // Arrange
        var db = new InMemoryDb();
        SeedData.Load(db);
        var svc = new ReferralService(db, NullLogger<ReferralService>.Instance);
        var req = new VerifyReferralRequest { ReferralCode = "INVALID123", DeviceId = "test-device" };

        // Act
        var result = svc.Verify(req);

        // Assert  
        result.IsValid.Should().BeFalse();
        result.Referrer.Should().BeNull();
        result.Campaign.Should().BeNull();
    }

    [Fact] 
    public void GetShareMessage_EmailChannel_IncludesSubject()
    {
        // Arrange
        var db = new InMemoryDb();
        SeedData.Load(db);
        var svc = new ReferralService(db, NullLogger<ReferralService>.Instance);

        // Act
        var result = svc.GetShareMessage("usr_001", "email", "en");

        // Assert
        result.Subject.Should().NotBeNullOrEmpty();
        result.Message.Should().Contain("https://cartoncaps.link/");
        result.Link.Should().StartWith("https://cartoncaps.link/");
    }

    [Fact]
    public void GetShareMessage_SmsChannel_NoSubject()
    {
        var db = new InMemoryDb();
        SeedData.Load(db);
        var svc = new ReferralService(db, NullLogger<ReferralService>.Instance);

        var result = svc.GetShareMessage("usr_001", "sms", null);

        result.Subject.Should().BeNull();
        result.Message.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Redeem_EmptyReferralCode_HandlesGracefully(string badCode)
    {
        var db = new InMemoryDb();
        var svc = new ReferralService(db, NullLogger<ReferralService>.Instance);
        var req = new RedeemReferralRequest { ReferralCode = badCode, RefereeUserId = "user123" };

        var result = svc.Redeem("usr_001", req);
        
        // Should handle bad input without crashing
        result.Should().NotBeNull();
    }
}
