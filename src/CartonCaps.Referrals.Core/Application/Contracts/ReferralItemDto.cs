namespace CartonCaps.Referrals.Core.Application.Contracts;

/// <summary>
///     Individual referral item.
/// </summary>
public class ReferralItemDto
{
    /// <summary>
    ///     Referral ID.
    /// </summary>
    /// <example>ref_2024_fall_001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Display name.
    /// </summary>
    /// <example>Maria S.</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Referral status.
    /// </summary>
    /// <example>complete</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    ///     Share channel.
    /// </summary>
    /// <example>whatsapp</example>
    public string? Channel { get; set; }

    /// <summary>
    ///     Invitation timestamp.
    /// </summary>
    /// <example>2025-10-20T14:30:00Z</example>
    public DateTimeOffset InvitedAt { get; set; }

    /// <summary>
    ///     Registration timestamp.
    /// </summary>
    /// <example>2025-10-22T09:15:00Z</example>
    public DateTimeOffset? RegisteredAt { get; set; }
}
