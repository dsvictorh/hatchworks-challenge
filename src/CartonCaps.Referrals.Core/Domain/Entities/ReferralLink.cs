namespace CartonCaps.Referrals.Core.Domain.Entities;

public class ReferralLink
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ReferralCode { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Channel { get; set; }
    public string? VendorId { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
