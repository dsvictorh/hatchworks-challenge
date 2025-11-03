namespace CartonCaps.Referrals.Core.Domain.Enums;

public static class ReferralStateMachine
{
    private static readonly string[] Order =
    {
        ReferralStatus.Invited, ReferralStatus.Clicked, ReferralStatus.Installed, ReferralStatus.Open,
        ReferralStatus.Registered, ReferralStatus.Redeemed, ReferralStatus.Complete
    };

    public static string Advance(string? currentStatus, string eventType)
    {
        var target = eventType switch
        {
            "click" => ReferralStatus.Clicked,
            "install" => ReferralStatus.Installed,
            "open" => ReferralStatus.Open,
            "registered" => ReferralStatus.Registered,
            "redeemed" => ReferralStatus.Redeemed,
            _ => currentStatus ?? ReferralStatus.Invited
        };

        var curIndex = currentStatus == null ? -1 : Array.IndexOf(Order, currentStatus);
        var tgtIndex = Array.IndexOf(Order, target);
        return tgtIndex > curIndex ? target : currentStatus ?? target;
    }
}
