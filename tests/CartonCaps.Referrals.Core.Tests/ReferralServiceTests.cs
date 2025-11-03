using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using CartonCaps.Referrals.Infrastructure.Exceptions;
using CartonCaps.Referrals.Infrastructure.Persistence;
using CartonCaps.Referrals.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace CartonCaps.Referrals.Core.Tests;

public class ReferralServiceTests
{
    private static ReferralsDbContext CreateContext()
    {
        // Ensure test database exists (admin connection)
        const string adminCs =
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

        using (var cn = new NpgsqlConnection(adminCs))
        {
            cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText =
                "SELECT 1 FROM pg_database WHERE datname = 'cartoncaps_core_test'";
            var exists = cmd.ExecuteScalar() != null;

            if (!exists)
            {
                using var create = cn.CreateCommand();
                create.CommandText = "CREATE DATABASE cartoncaps_core_test";
                create.ExecuteNonQuery();
            }
        }

        // App/test DbContext
        const string cs =
            "Host=localhost;Port=5432;Database=cartoncaps_core_test;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ReferralsDbContext>()
            .UseNpgsql(cs)
            .Options;

        var db = new ReferralsDbContext(options);

        // Apply migrations and seed a clean slate for repeatability
        db.Database.Migrate();
        db.ReferralEvents.RemoveRange(db.ReferralEvents);
        db.ReferralSessions.RemoveRange(db.ReferralSessions);
        db.ReferralLinks.RemoveRange(db.ReferralLinks);
        db.Referrals.RemoveRange(db.Referrals);
        db.ReferralCodes.RemoveRange(db.ReferralCodes);
        db.SaveChanges();

        DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        return db;
    }

    [Fact]
    public void GenerateLink_ReturnsValidUrl()
    {
        using var db = CreateContext();
        var logger = NullLogger<EfReferralService>.Instance;
        IReferralService svc = new EfReferralService(db, logger);

        var res = svc.GenerateLink("usr_referrer_001", "sms");

        res.ReferralLink.Should().StartWith("https://cartoncaps.link/");
    }

    [Fact]
    public void GetMyReferrals_WithValidUser_ReturnsReferrals()
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);

        var result = svc.GetMyReferrals("usr_referrer_001");

        result.Should().NotBeNull();
        result.ReferralCode.Should().NotBeEmpty();
        result.Summary.Should().NotBeNull();
        result.Summary.Total.Should().BeGreaterThanOrEqualTo(0);
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public void Verify_InvalidCode_ThrowsNotFound()
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);
        var req = new VerifyReferralRequest
        {
            ReferralCode = "INVALID123",
            DeviceId = "test-device"
        };

        Action act = () => svc.Verify(req);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Verify_ValidCode_ReturnsValid()
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);
        var req = new VerifyReferralRequest { ReferralCode = "XY7G4D", DeviceId = "device-verify-ok" };

        var result = svc.Verify(req);

        result.IsValid.Should().BeTrue();
        result.Referrer.Should().NotBeNull();
    }

    [Fact]
    public void GetShareMessage_EmailChannel_IncludesSubject()
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);

        var result = svc.GetShareMessage("usr_001", "email", "en");

        result.Subject.Should().NotBeNullOrEmpty();
        result.Message.Should().Contain("https://cartoncaps.link/");
        result.Link.Should().StartWith("https://cartoncaps.link/");
    }

    [Fact]
    public void GetShareMessage_SmsChannel_NoSubject()
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);

        var result = svc.GetShareMessage("usr_001", "sms", null);

        result.Subject.Should().BeNull();
        result.Message.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Redeem_EmptyReferralCode_HandlesGracefully(string badCode)
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);
        var req = new RedeemReferralRequest
        {
            ReferralCode = badCode,
            RefereeUserId = "user123"
        };

        var result = svc.Redeem("usr_001", req);

        // Should handle bad input without crashing
        result.Should().NotBeNull();
    }

    [Fact]
    public void Redeem_DuplicateForSameUser_ThrowsConflict()
    {
        using var db = CreateContext();
        var svc = new EfReferralService(db, NullLogger<EfReferralService>.Instance);

        var redeem = new RedeemReferralRequest { ReferralCode = "XY7G4D", RefereeUserId = "dup_user" };

        var first = svc.Redeem("usr_new_001", redeem);
        first.Should().NotBeNull();

        Action second = () => svc.Redeem("usr_new_001", redeem);
        second.Should().Throw<ConflictOperationException>();
    }
}
