namespace CartonCaps.Referrals.Infrastructure.Exceptions;

public class ConflictOperationException : Exception
{
    public ConflictOperationException(string message) : base(message)
    {
    }
}
