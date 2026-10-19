
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;


    public AccountService(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }



    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {

        if (!_currentUser.IsAuthenticated)
            return null;

        return await _userManager.FindByIdAsync(_currentUser.UserId!);
    }



    public async Task<ProfileDto?> GetProfileAsync()
    {
        var user =
            await GetCurrentUserAsync();


        if (user == null)
            return null;


        return new ProfileDto
        {
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }



    public async Task<bool> UpdateProfileAsync(ProfileDto dto)
    {
        var user =
            await GetCurrentUserAsync();


        if (user == null)
            return false;


        user.FirstName = dto.FirstName;

        user.LastName = dto.LastName;


        var result =
            await _userManager.UpdateAsync(user);


        return result.Succeeded;
    }

    public async Task<IdentityResult> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User not found."
                });

        return await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword);
    }


    public async Task<IList<string>> GetCurrentUserRolesAsync()
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
            return [];

        return await _userManager.GetRolesAsync(user);
    }


}