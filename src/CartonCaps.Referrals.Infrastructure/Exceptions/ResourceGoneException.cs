namespace CartonCaps.Referrals.Infrastructure.Exceptions;

public class ResourceGoneException : Exception
{
    public ResourceGoneException(string message) : base(message)
    {
    }
}
