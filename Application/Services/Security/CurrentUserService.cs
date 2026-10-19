
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal Principal =>   _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal(new ClaimsIdentity());

    public ClaimsPrincipal User => Principal;

    public bool IsAuthenticated =>
        Principal.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        Principal.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        Principal.FindFirstValue(ClaimTypes.Email);

     public string? UserName =>  User.Identity?.Name;
}