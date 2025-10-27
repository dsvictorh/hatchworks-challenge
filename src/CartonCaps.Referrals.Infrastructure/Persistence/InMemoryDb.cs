using CartonCaps.Referrals.Core.Domain.Entities;

namespace CartonCaps.Referrals.Infrastructure.Persistence;

/// <summary>
///     In-memory database.
/// </summary>
public class InMemoryDb
{
    public List<Referral> Referrals { get; } = new();
    public List<ReferralCode> ReferralCodes { get; } = new();
}
