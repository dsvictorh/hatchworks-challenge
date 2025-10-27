using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using CartonCaps.Referrals.Core.Domain.Enums;
using CartonCaps.Referrals.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace CartonCaps.Referrals.Infrastructure.Services;

/// <summary>
///     Referral service implementation.
/// </summary>
public class ReferralService : IReferralService
{
    private readonly InMemoryDb _db;
    private readonly ILogger<ReferralService> _logger;

    public ReferralService(InMemoryDb db, ILogger<ReferralService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public GetReferralsResponse GetMyReferrals(string userId, int page = 1, int size = 20, string? status = null)
    {
        _logger.LogInformation("Getting referrals for user {UserId}", userId);

        var code = _db.ReferralCodes.FirstOrDefault(x => x.UserId == userId)?.Code ?? "XY7G4D";

        var q = _db.Referrals.Where(r => r.ReferrerId == userId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        var results = q.OrderByDescending(r => r.InvitedAt).ToList();
        var pageItems = results.Take(size).Select((r, i) => new ReferralItemDto
        {
            Id = r.Id,
            Name = NameFromIndex(i),
            Status = r.Status,
            Channel = r.Channel,
            InvitedAt = r.InvitedAt,
            RegisteredAt = r.RegisteredAt
        }).ToList();

        var total = results.Count;
        var complete = results.Count(r => r.Status == "complete");
        var pending = total - complete;

        return new GetReferralsResponse
        {
            ReferralCode = code,
            Summary = new SummaryDto
            {
                Total = total,
                Complete = complete,
                Pending = pending
            },
            Items = pageItems
        };
    }

    /// <summary>
    ///     Generate referral link.
    /// </summary>
    public GenerateLinkResponse GenerateLink(string userId, string channel)
    {
        _logger.LogInformation("Generating link for user {UserId} channel {Channel}", userId, channel);

        var code = _db.ReferralCodes.FirstOrDefault(x => x.UserId == userId)?.Code ?? "XY7G4D";
        var linkId = Guid.NewGuid().ToString("N");
        var link = $"https://cartoncaps.link/{linkId}?ref={code}";
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);
        var meta = new Dictionary<string, object> { ["channel"] = channel, ["campaignId"] = "fall-2024" };

        return new GenerateLinkResponse
        {
            ReferralLink = link,
            ExpiresAt = expiresAt,
            Metadata = meta
        };
    }

    /// <summary>
    ///     Get share message.
    /// </summary>
    public ShareMessageResponse GetShareMessage(string userId, string channel, string? locale)
    {
        _logger.LogInformation("Getting share message for user {UserId}", userId);

        var link = GenerateLink(userId, channel).ReferralLink;

        if (channel == "email")
            return new ShareMessageResponse
            {
                Subject = MessageTemplates.EmailTemplate.Subject,
                Message = MessageTemplates.EmailTemplate.Body.Replace("[REFERRAL_LINK]", link),
                Link = link
            };

        return new ShareMessageResponse
        {
            Subject = null,
            Message = MessageTemplates.SmsTemplate.Body.Replace("[REFERRAL_LINK]", link),
            Link = link
        };
    }

    public void RecordShare(string userId, ShareEventRequest req)
    {
        _logger.LogInformation("Recording share event for user {UserId}", userId);
        // Analytics tracking service integration would be implemented here
    }

    /// <summary>
    ///     Verify referral code.
    /// </summary>
    public VerifyReferralResponse Verify(VerifyReferralRequest req)
    {
        _logger.LogInformation("Verifying code {ReferralCode}", req.ReferralCode);

        var valid = _db.ReferralCodes.Any(c => c.Code.Equals(req.ReferralCode, StringComparison.OrdinalIgnoreCase));

        if (valid)
            return new VerifyReferralResponse
            {
                IsValid = true,
                Referrer = new ReferrerSummary { FirstName = "Sarah", School = "Westfield Elementary" },
                Campaign = new Dictionary<string, object> { { "id", "fall-2024" } }
            };

        _logger.LogWarning("Referral code {ReferralCode} is invalid or not found", req.ReferralCode);
        return new VerifyReferralResponse { IsValid = false, Referrer = null, Campaign = null };
    }

    /// <summary>
    ///     Create referral session.
    /// </summary>
    public CreateSessionResponse CreateSession(CreateSessionRequest req)
    {
        _logger.LogInformation("Creating session for code {ReferralCode}", req.ReferralCode);

        var sessionId = $"ses-{Guid.NewGuid():N}";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

        return new CreateSessionResponse
        {
            SessionId = sessionId,
            ReferralCode = req.ReferralCode,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>
    ///     Redeem referral code.
    /// </summary>
    public RedeemReferralResponse Redeem(string userId, RedeemReferralRequest req)
    {
        _logger.LogInformation("Redeeming code {ReferralCode} for user {UserId}", req.ReferralCode, userId);

        var referral = _db.Referrals.FirstOrDefault(r => r.ReferralCode == req.ReferralCode && r.Status != "complete");

        if (referral != null)
        {
            referral.Status = "redeemed";
            referral.RefereeUserId = req.RefereeUserId;
            referral.RegisteredAt = DateTimeOffset.UtcNow;

            return new RedeemReferralResponse { Status = "redeemed", RewardEligible = true };
        }

        _logger.LogWarning("No eligible referral found for redemption with code {ReferralCode} by user {UserId}",
            req.ReferralCode, userId);

        return new RedeemReferralResponse { Status = "complete", RewardEligible = true };
    }

    private static string NameFromIndex(int i)
    {
        var names = new[] { "Sarah M.", "Mike J.", "Lisa K.", "Tom W.", "Amy L.", "Dave R.", "Kate P." };
        return i < names.Length ? names[i] : $"User {i + 1}";
    }
}
