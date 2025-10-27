using CartonCaps.Referrals.Core.Application.Contracts;

namespace CartonCaps.Referrals.Core.Application.Abstractions;

/// <summary>
///     Referral service operations.
/// </summary>
public interface IReferralService
{
    GetReferralsResponse GetMyReferrals(string userId, int page = 1, int size = 20, string? status = null);

    /// <summary>
    ///     Generate referral link.
    /// </summary>
    GenerateLinkResponse GenerateLink(string userId, string channel);

    ShareMessageResponse GetShareMessage(string userId, string channel, string? locale);

    void RecordShare(string userId, ShareEventRequest req);

    /// <summary>
    ///     Verify referral code.
    /// </summary>
    VerifyReferralResponse Verify(VerifyReferralRequest req);

    CreateSessionResponse CreateSession(CreateSessionRequest req);

    RedeemReferralResponse Redeem(string userId, RedeemReferralRequest req);
}
