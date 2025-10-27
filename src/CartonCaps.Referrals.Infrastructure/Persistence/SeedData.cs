using CartonCaps.Referrals.Core.Domain.Entities;

namespace CartonCaps.Referrals.Infrastructure.Persistence;

public static class SeedData
{
    /// <summary>
    ///     Loads seed data into the provided in-memory database.
    /// </summary>
    /// <param name="db">The in-memory database instance.</param>
    public static void Load(InMemoryDb db)
    {
        var referrerUserId = "usr_referrer_001";
        db.ReferralCodes.Add(new ReferralCode { Code = "XY7G4D", UserId = referrerUserId, Status = "active" });

        db.Referrals.Add(new Referral
        {
            ReferrerId = referrerUserId,
            ReferralCode = "XY7G4D",
            Status = "complete",
            Channel = "sms",
            RegisteredAt = DateTimeOffset.UtcNow.AddDays(-20)
        });
        db.Referrals.Add(new Referral
        {
            ReferrerId = referrerUserId,
            ReferralCode = "XY7G4D",
            Status = "complete",
            Channel = "email",
            RegisteredAt = DateTimeOffset.UtcNow.AddDays(-15)
        });
        db.Referrals.Add(new Referral
        {
            ReferrerId = referrerUserId,
            ReferralCode = "XY7G4D",
            Status = "invited",
            Channel = "generic"
        });
    }
}
