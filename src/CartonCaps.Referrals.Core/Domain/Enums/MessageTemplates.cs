namespace CartonCaps.Referrals.Core.Domain.Enums;

/// <summary>
///     Message templates.
/// </summary>
public static class MessageTemplates
{
    /// <summary>
    ///     Email template.
    /// </summary>
    public class EmailTemplate
    {
        public const string Subject = "You’re invited to try the Carton Caps app!";

        public const string Body =
            "Hey! " +
            "\n\nJoin me in earning cash for our school by using the Carton Caps app. " +
            "It's an easy way to make a difference. All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt. " +
            "Carton Caps are worth $.10 each and they add up fast! " +
            "Twice a year, our school receives a check to help pay for whatever we need - equipment, supplies or experiences the kids love!" +
            "\n\nDownload the Carton Caps app here: [REFERRAL_LINK]" +
            "\n";
    }

    /// <summary>
    ///     SMS template.
    /// </summary>
    public class SmsTemplate
    {
        public const string Body =
            "Hi! Join me in earning money for our school using the Carton Caps app." +
            "It's an easy way to make a difference." +
            "Use the link below to download the Carton Caps app: [REFERRAL_LINK]";
    }
}
