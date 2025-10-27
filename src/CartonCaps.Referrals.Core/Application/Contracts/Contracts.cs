using System.ComponentModel.DataAnnotations;
using CartonCaps.Referrals.Core.Domain.ValueObjects;

namespace CartonCaps.Referrals.Core.Application.Contracts;

/// <summary>
///     Response with user referrals.
/// </summary>
public class GetReferralsResponse
{
    /// <summary>
    ///     User's referral code.
    /// </summary>
    /// <example>XY7G4D</example>
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>
    ///     Referral summary statistics.
    /// </summary>
    public SummaryDto Summary { get; set; } = new();

    /// <summary>
    ///     List of referral items.
    /// </summary>
    public IReadOnlyList<ReferralItemDto> Items { get; set; } = new List<ReferralItemDto>();
}

/// <summary>
///     Request to generate a referral link.
/// </summary>
public class GenerateLinkRequest
{
    /// <example>email</example>
    [Required]
    public string Channel { get; set; } = string.Empty;
}

/// <summary>
///     Generated referral link response.
/// </summary>
public class GenerateLinkResponse
{
    /// <summary>
    ///     The referral URL.
    /// </summary>
    /// <example>https://cartoncaps.link/r/XY7G4D?utm_source=email&amp;utm_campaign=spring2025</example>
    public string ReferralLink { get; set; } = string.Empty;

    /// <summary>
    ///     Link expiration time.
    /// </summary>
    /// <example>2025-11-26T23:59:59Z</example>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     Additional link metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; set; }
}

/// <summary>
///     Request for share message.
/// </summary>
public class ShareMessageRequest
{
    /// <summary>
    ///     Target channel.
    /// </summary>
    /// <example>sms</example>
    [Required]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    ///     Optional locale code (default: 'en').
    /// </summary>
    /// <example>en</example>
    public string? Locale { get; set; }
}

/// <summary>
///     Share message response.
/// </summary>
public class ShareMessageResponse
{
    /// <summary>
    ///     Email subject line (null for non-email channels).
    /// </summary>
    /// <example>You're invited to try the Carton Caps app!</example>
    public string? Subject { get; set; }

    /// <summary>
    ///     Formatted message text with embedded referral link.
    /// </summary>
    /// <example>
    ///     Hi! Join me in earning money for our school using the Carton Caps app. Use the link below to download:
    ///     https://cartoncaps.link/r/XY7G4D
    /// </example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     The referral link embedded in the message.
    /// </summary>
    /// <example>https://cartoncaps.link/r/XY7G4D</example>
    public string Link { get; set; } = string.Empty;
}

public class ShareEventRequest
{
    /// <example>instagram</example>
    [Required]
    public string Channel { get; set; } = string.Empty;

    /// <example>https://cartoncaps.link/f84bc2a8ed94d1a06bc4e4e79b8f6c21?ref=XY7G4D</example>
    [Required]
    [Url]
    public string Link { get; set; } = string.Empty;

    public DeviceInfo? DeviceInfo { get; set; }
}

/// <summary>
///     Referral code verification request.
/// </summary>
public class VerifyReferralRequest
{
    /// <summary>
    ///     Referral code to verify.
    /// </summary>
    /// <example>XY7G4D</example>
    [Required]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Referral code must be between 6 and 20 characters")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Referral code can only contain letters and numbers")]
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>
    ///     Device identifier.
    /// </summary>
    /// <example>ios-4A7B8C9D-E2F3-4G5H-6I7J-8K9L0M1N2O3P</example>
    [Required]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Device ID must be between 10 and 100 characters")]
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
///     Referral code verification result.
/// </summary>
public class VerifyReferralResponse
{
    public bool IsValid { get; set; }
    public ReferrerSummary? Referrer { get; set; }
    public IDictionary<string, object>? Campaign { get; set; }
}

public class ReferrerSummary
{
    public string FirstName { get; set; } = string.Empty;
    public string? School { get; set; }
}

/// <summary>
///     Create referral session request.
/// </summary>
public class CreateSessionRequest
{
    /// <summary>
    ///     Referral code.
    /// </summary>
    /// <example>XY7G4D</example>
    [Required]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Referral code must be between 6 and 20 characters")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Referral code can only contain letters and numbers")]
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>
    ///     Device identifier.
    /// </summary>
    /// <example>android-B8C9D0E1-F4G5-6H7I-8J9K-L2M3N4O5P6Q7</example>
    [Required]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Device ID must be between 10 and 100 characters")]
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
///     Response containing the created referral session information
/// </summary>
public class CreateSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string ReferralCode { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}

/// <summary>
///     Redeem referral request.
/// </summary>
public class RedeemReferralRequest
{
    /// <summary>
    ///     Referral code to redeem.
    /// </summary>
    /// <example>XY7G4D</example>
    [Required]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Referral code must be between 6 and 20 characters")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Referral code can only contain letters and numbers")]
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>
    ///     New user ID.
    /// </summary>
    /// <example>usr_new_98765</example>
    [Required]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Referee user ID must be between 5 and 50 characters")]
    public string RefereeUserId { get; set; } = string.Empty;
}

public class RedeemReferralResponse
{
    public string Status { get; set; } = string.Empty;
    public bool RewardEligible { get; set; }
}
