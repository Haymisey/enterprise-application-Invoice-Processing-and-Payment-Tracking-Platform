namespace Shared.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public IEnumerable<string> Errors { get; }
}
