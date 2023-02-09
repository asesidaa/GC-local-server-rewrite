namespace Application.Common.Exceptions;

public class CardExistsException : Exception
{
    public CardExistsException(string? message) : base(message)
    {
    }
}