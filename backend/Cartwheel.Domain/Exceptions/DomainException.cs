namespace Cartwheel.Domain.Exceptions;

/// <summary>
/// Base type for all exceptions that represent a violation of a domain rule.
/// Catch this type at application boundaries to distinguish expected domain
/// failures from unexpected/infrastructure errors.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
