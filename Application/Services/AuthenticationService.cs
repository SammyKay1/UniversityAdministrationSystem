
using Microsoft.AspNetCore.Identity;

public class AuthenticationService
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthenticationService( SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<LoginResult> LoginAsync(LoginDto dto)
    {
        var result =
            await _signInManager.PasswordSignInAsync( dto.Email, dto.Password,  dto.RememberMe,  lockoutOnFailure: true);

        if (result.Succeeded)
            return LoginResult.Successful();

        if (result.IsLockedOut)
        {
            return new LoginResult
            {
                LockedOut = true,
                ErrorMessage =   "Your account is locked."
            };
        }

        if (result.RequiresTwoFactor)
        {
            return new LoginResult
            {
                RequiresTwoFactor = true
            };
        }

        return LoginResult.Failed(  "Invalid email or password.");
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}