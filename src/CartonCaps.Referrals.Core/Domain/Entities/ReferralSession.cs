namespace CartonCaps.Referrals.Core.Domain.Entities;

public class ReferralSession
{
    public string SessionId { get; set; } = $"ses-{Guid.NewGuid():N}";
    public string ReferralCode { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Status { get; set; } = "verified"; // initiated, verified, redeemed
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddHours(2);
}
