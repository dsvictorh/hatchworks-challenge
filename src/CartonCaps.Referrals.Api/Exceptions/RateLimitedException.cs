namespace CartonCaps.Referrals.Api.Exceptions;

public class RateLimitedException : Exception
{
    public RateLimitedException(string message) : base(message)
    {
    }
}
