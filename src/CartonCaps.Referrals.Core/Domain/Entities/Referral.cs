namespace CartonCaps.Referrals.Core.Domain.Entities;

/// <summary>
///     Referral entity.
/// </summary>
public class Referral
{
    public Referral()
    {
        Id = Guid.NewGuid().ToString("N");
        ReferrerId = string.Empty;
        ReferralCode = string.Empty;
        InvitedAt = DateTimeOffset.UtcNow;
    }

    // Unique identifier for the referral
    public string Id { get; set; }

    // Identifier of the user who made the referral  
    public string ReferrerId { get; set; }

    // Identifier of the user who was referred
    public string? RefereeUserId { get; set; }

    // The referral code used for this referral
    public string ReferralCode { get; set; }

    // Status of the referral (e.g., invited, complete)
    public string Status { get; set; } = "invited";

    // Channel through which the referral was made (e.g., email, sms)
    public string? Channel { get; set; }

    // Timestamp when the referral was created
    public DateTimeOffset InvitedAt { get; set; }

    // Timestamp when the referred user registered
    public DateTimeOffset? RegisteredAt { get; set; }
}
