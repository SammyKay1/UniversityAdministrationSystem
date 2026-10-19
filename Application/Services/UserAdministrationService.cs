using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserAdministrationService
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly RoleManager<ApplicationRole> _roleManager;

    private readonly ApplicationDbContext _context;


    public UserAdministrationService(UserManager<ApplicationUser> userManager,   RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;

        _roleManager = roleManager;

        _context = context;
    }


    public async Task<PagedResult<UserDto>> GetUsersAsync(   UserListRequest request)
    {
        var query = _userManager.Users.AsNoTracking();


        // ==========================================
        // SEARCH
        // ==========================================

        if (!string.IsNullOrWhiteSpace( request.SearchTerm))
        {
            var search =  request.SearchTerm.Trim();

            query = query.Where(x =>
                (x.UserName != null &&
                 x.UserName.Contains(search)) ||

                (x.Email != null &&
                 x.Email.Contains(search)) ||

                (x.FirstName != null &&
                 x.FirstName.Contains(search)) ||

                (x.LastName != null &&
                 x.LastName.Contains(search)));
        }


        // ==========================================
        // TOTAL COUNT
        // ==========================================

        var totalCount =
            await query.CountAsync();


        // ==========================================
        // SORTING
        // ==========================================

        query = request.SortColumn switch
        {
            "Email" =>
                request.SortDescending
                    ? query.OrderByDescending(
                        x => x.Email)
                    : query.OrderBy(
                        x => x.Email),

            "FirstName" =>
                request.SortDescending
                    ? query.OrderByDescending(
                        x => x.FirstName)
                    : query.OrderBy(
                        x => x.FirstName),

            "LastName" =>
                request.SortDescending
                    ? query.OrderByDescending(
                        x => x.LastName)
                    : query.OrderBy(
                        x => x.LastName),

            _ =>
                request.SortDescending
                    ? query.OrderByDescending(
                        x => x.UserName)
                    : query.OrderBy(
                        x => x.UserName)
        };


        // ==========================================
        // PAGING
        // ==========================================

        var users =
            await query
                .Skip( (request.PageNumber - 1)  * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();


        // ==========================================
        // USER DTOs
        // ==========================================

        var result =
            users.Select(user =>
                new UserDto
                {
                    Id = user.Id,

                    UserName =
                        user.UserName ?? string.Empty,

                    Email =
                        user.Email ?? string.Empty,

                    FirstName =
                        user.FirstName,

                    LastName =
                        user.LastName,

                    EmailConfirmed = user.EmailConfirmed,

                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                })
                .ToList();


        // ==========================================
        // LOAD ROLES FOR THESE USERS
        // ==========================================

        var userIds =
            users.Select(x => x.Id)
                 .ToList();


        if (userIds.Count > 0)
        {
            var roleData =
                await (from userRole in _context.UserRoles

                       join role
                       in _context.Roles
                       on userRole.RoleId equals role.Id

                       where userIds.Contains(
                           userRole.UserId)

                       select new
                       {
                           UserId =
                               userRole.UserId,

                           RoleName =
                               role.Name
                       }
                )
                .ToListAsync();


            /*foreach (var dto in result)
            {
                dto.Roles =
                    roleData
                        .Where(x =>
                            x.UserId == dto.Id)
                        .Select(x =>
                            x.RoleName!)
                        .ToList();
            }*/
        }


        // ==========================================
        // LOCKOUT STATUS
        // ==========================================

        foreach (var dto in result)
        {
            var user =
                users.First(x =>
                    x.Id == dto.Id);

            dto.IsLockedOut =
                await _userManager
                    .IsLockedOutAsync(user);
        }


        return new PagedResult<UserDto>
        {
            Items = result,

            PageNumber =
                request.PageNumber,

            PageSize =
                request.PageSize,

            TotalCount =
                totalCount
        };
    }


    /// <summary>
    /// Create new User
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>

    public async Task<IdentityResult> CreateUserAsync(
        CreateUserDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.UserName.Trim(),

            Email = dto.Email.Trim(),

            FirstName = dto.FirstName.Trim(),

            LastName = dto.LastName.Trim(),

            EmailConfirmed = true
        };


        var result =
            await _userManager.CreateAsync(
                user,
                dto.Password);


        if (!result.Succeeded)
            return result;


        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    dto.Role);

            if (!roleResult.Succeeded)
            {
                // Roll back the newly created user
                await _userManager.DeleteAsync(user);

                return roleResult;
            }
        }


        return IdentityResult.Success;
    }


    /// <summary>
    /// Get user roles; required by the create user page
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetRolesAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.Name!)
            .ToListAsync();
    }


    /// <summary>
    /// Get user details for editing
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>

    public async Task<EditUserDto?> GetUserForEditAsync(
    string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
            return null;

        return new EditUserDto
        {
            Id = user.Id,

            UserName =
                user.UserName ?? string.Empty,

            Email =
                user.Email ?? string.Empty,

            FirstName =
                user.FirstName ?? string.Empty,

            LastName =
                user.LastName ?? string.Empty,

            EmailConfirmed =
                user.EmailConfirmed
        };
    }


    /// <summary>
    /// Update user data
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<IdentityResult> UpdateUserAsync(
    EditUserDto dto)
    {
        var user =
            await _userManager.FindByIdAsync(dto.Id);

        if (user is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User was not found."
                });
        }


        user.UserName =
            dto.UserName.Trim();

        user.Email =
            dto.Email.Trim();

        user.FirstName =
            dto.FirstName.Trim();

        user.LastName =
            dto.LastName.Trim();

        user.EmailConfirmed =
            dto.EmailConfirmed;


        return await _userManager.UpdateAsync(user);
    }




    public async Task<UserRolesDto?> GetUserRolesAsync(
    string userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;


        var allRoles = await _roleManager.Roles
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => x.Name!)
                .ToListAsync();


        var currentRoles =
            await _userManager.GetRolesAsync(user);


        return new UserRolesDto
        {
            UserId = user.Id,

            UserName =
                user.UserName ?? string.Empty,

            AvailableRoles = allRoles,

            SelectedRoles =
                currentRoles.ToList()
        };
    }



    public async Task<IdentityResult> UpdateUserRolesAsync(
    string userId,
    IEnumerable<string> selectedRoles)
    {
        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User was not found."
                });
        }


        var currentRoles =
            await _userManager.GetRolesAsync(user);


        var rolesToRemove =
            currentRoles
                .Except(
                    selectedRoles,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();


        var rolesToAdd =
            selectedRoles
                .Except(
                    currentRoles,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();


        if (rolesToRemove.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    rolesToRemove);

            if (!removeResult.Succeeded)
                return removeResult;
        }


        if (rolesToAdd.Count > 0)
        {
            var addResult =
                await _userManager.AddToRolesAsync(
                    user,
                    rolesToAdd);

            if (!addResult.Succeeded)
                return addResult;
        }


        return IdentityResult.Success;
    }

    public async Task<IdentityResult> LockUserAsync(
    string userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User was not found."
                });
        }

        user.LockoutEnabled = true;

        user.LockoutEnd =
            DateTimeOffset.MaxValue;

        return await _userManager.UpdateAsync(user);
    }



    public async Task<IdentityResult> UnlockUserAsync(
        string userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User was not found."
                });
        }

        user.LockoutEnd = null;

        user.AccessFailedCount = 0;

        return await _userManager.UpdateAsync(user);
    }


    /*
        public async Task<IdentityResult> LockUserAsync(
            string userId)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User was not found."
                    });
            }

            // Lock for a very long period.
            user.LockoutEnd =
                DateTimeOffset.UtcNow.AddYears(100);

            user.LockoutEnabled = true;

            return await _userManager.UpdateAsync(user);
        }


        public async Task<IdentityResult> UnlockUserAsync(
            string userId)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User was not found."
                    });
            }

            user.LockoutEnd = null;

            return await _userManager.UpdateAsync(user);
        }  */


    public async Task<IdentityResult> DeleteUserAsync(
        string userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "User was not found."
                });
        }

        return await _userManager.DeleteAsync(user);
    }






}