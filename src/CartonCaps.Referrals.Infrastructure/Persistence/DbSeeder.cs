using CartonCaps.Referrals.Core.Domain.Entities;

namespace CartonCaps.Referrals.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ReferralsDbContext db, CancellationToken ct = default)
    {
        if (!db.ReferralCodes.Any())
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

            await db.SaveChangesAsync(ct);
        }
    }
}
