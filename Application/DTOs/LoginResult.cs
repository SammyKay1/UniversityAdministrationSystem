
public class LoginResult
{
    public bool Success { get; init; }

    public bool LockedOut { get; init; }

    public bool RequiresTwoFactor { get; init; }

    public string? ErrorMessage { get; init; }

    public static LoginResult Successful()
        => new() { Success = true };

    public static LoginResult Failed(string message)
        => new()
        {
            Success = false,
            ErrorMessage = message
        };
}