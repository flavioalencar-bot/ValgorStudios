namespace Valgor.Application.Common.Results;

public sealed class Error
{
    public string Code { get; }
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Validation(string message) => new("validation", message);
    public static Error Unauthorized(string message) => new("unauthorized", message);
    public static Error NotFound(string message) => new("not_found", message);
    public static Error Conflict(string message) => new("conflict", message);
    public static Error Failure(string message) => new("failure", message);
}
