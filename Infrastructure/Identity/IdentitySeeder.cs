
using Microsoft.AspNetCore.Identity;

public class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentitySeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await SeedRoles();

        await SeedAdministrator();

        await SeedRegistrar();

        await SeedLecturer();

        await SeedStudent();
    }

    private async Task SeedRoles()
    {
        string[] roles =
        {
            "Administrator",
            "Registrar",
            "Lecturer",
            "Student"
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(
                    new ApplicationRole
                    {
                        Name = role,
                        Description = $"{role} Role"
                    });
            }
        }
    }

private async Task SeedAdministrator()
{
    const string email =
        "admin@school.com";

    var user =
        await _userManager.FindByEmailAsync(email);

    if (user != null)
        return;

    user = new ApplicationUser
    {
        UserName = email,
        Email = email,

        FirstName = "System",

        LastName = "Administrator",

        EmailConfirmed = true,

        IsActive = true
    };

    var result =
        await _userManager.CreateAsync(
            user,
            "Admin123!");

    if (result.Succeeded)
    {
        await _userManager.AddToRoleAsync(
            user,
            Roles.Administrator);
    }
}


private async Task SeedRegistrar()
{
    const string email =
        "registrar@school.com";

    if (await _userManager.FindByEmailAsync(email) != null)
        return;

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,

        FirstName = "Default",

        LastName = "Registrar",

        EmailConfirmed = true
    };

    var result =
        await _userManager.CreateAsync(
            user,
            "Registrar123!");

    if (result.Succeeded)
    {
        await _userManager.AddToRoleAsync(
            user,
            Roles.Registrar);
    }
}

private async Task SeedLecturer()
{
    const string email =
        "lecturer@school.com";

    if (await _userManager.FindByEmailAsync(email) != null)
        return;

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,

        FirstName = "Default",

        LastName = "Lecturer",

        EmailConfirmed = true
    };

    var result =
        await _userManager.CreateAsync(
            user,
            "Lecturer123!");

    if (result.Succeeded)
    {
        await _userManager.AddToRoleAsync(
            user,
            Roles.Lecturer);
    }
}

private async Task SeedStudent()
{
    const string email =
        "student@school.com";

    if (await _userManager.FindByEmailAsync(email) != null)
        return;

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,

        FirstName = "Default",

        LastName = "Student",

        EmailConfirmed = true
    };

    var result =
        await _userManager.CreateAsync(
            user,
            "Student123!");

    if (result.Succeeded)
    {
        await _userManager.AddToRoleAsync(
            user,
            Roles.Student);
    }
}


}