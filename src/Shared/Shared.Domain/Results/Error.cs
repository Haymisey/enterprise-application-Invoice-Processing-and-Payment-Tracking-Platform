namespace Shared.Domain.Results;

/// <summary>
/// Represents an error with a code and description.
/// Used with the Result pattern for structured error handling.
/// </summary>
public record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified value is null.");
}
