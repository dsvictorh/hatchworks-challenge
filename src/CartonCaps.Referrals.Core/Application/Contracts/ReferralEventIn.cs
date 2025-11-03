using CartonCaps.Referrals.Core.Domain.ValueObjects;

namespace CartonCaps.Referrals.Core.Application.Contracts;

/// <summary>
///     Incoming vendor/SDK referral lifecycle event.
/// </summary>
public class ReferralEventIn
{
    public string Event { get; set; } = string.Empty; // click, install, open, redeemed
    public string ReferralCode { get; set; } = string.Empty;
    public string? EventId { get; set; }
    public DeviceInfo? DeviceInfo { get; set; }
}
