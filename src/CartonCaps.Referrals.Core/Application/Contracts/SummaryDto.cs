namespace CartonCaps.Referrals.Core.Application.Contracts;

/// <summary>
///     Referral summary statistics.
/// </summary>
public class SummaryDto
{
    /// <summary>
    ///     Total referrals.
    /// </summary>
    /// <example>25</example>
    public int Total { get; set; }

    /// <summary>
    ///     Completed referrals.
    /// </summary>
    /// <example>18</example>
    public int Complete { get; set; }

    /// <summary>
    ///     Pending referrals.
    /// </summary>
    /// <example>7</example>
    public int Pending { get; set; }
}
