
using System.Runtime.CompilerServices;

public class OperationResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public static OperationResult Successful(string message = "")
        => new()
        {
            Success = true,
            Message = message
        };

    public static OperationResult Failure(string message)
        => new()
        {
            Success = false,
            Message = message
        };

    
}