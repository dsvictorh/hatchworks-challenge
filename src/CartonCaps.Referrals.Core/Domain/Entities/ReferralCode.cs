namespace CartonCaps.Referrals.Core.Domain.Entities;

/// <summary>
///     Referral code entity.
/// </summary>
public class ReferralCode
{
    public ReferralCode()
    {
        Code = string.Empty;
        UserId = string.Empty;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Unique identifier for the referral code
    public string Code { get; set; }

    // Identifier of the user who owns this referral code
    public string UserId { get; set; }

    // Status of the referral code (e.g., active, inactive)
    public string Status { get; set; } = "active";

    // Timestamp when the referral code was created
    public DateTimeOffset CreatedAt { get; set; }
}
