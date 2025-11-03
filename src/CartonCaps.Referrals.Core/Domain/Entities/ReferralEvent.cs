namespace CartonCaps.Referrals.Core.Domain.Entities;

public class ReferralEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string EventType { get; set; } = string.Empty; // click, install, open, redeemed
    public string ReferralCode { get; set; } = string.Empty;
    public string? EventId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? DeviceId { get; set; }
}
