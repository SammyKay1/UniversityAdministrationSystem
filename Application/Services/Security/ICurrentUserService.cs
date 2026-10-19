
using System.Security.Claims;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

     string? UserName { get; }

    ClaimsPrincipal User { get; }
}