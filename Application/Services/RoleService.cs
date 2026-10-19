
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class RoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public RoleService(RoleManager<ApplicationRole> roleManager,  ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .ToListAsync();

        var result = new List<RoleDto>();

        foreach (var role in roles)
        {
            var userCount = await _context.UserRoles
                .CountAsync(x => x.RoleId == role.Id);

            var permissionCount = await _context.RolePermissions
                .CountAsync(x => x.RoleId == role.Id);

            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                UserCount = userCount,
                PermissionCount = permissionCount
            });
        }

        return result;
    }

    public async Task<IdentityResult> CreateRoleAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Role name is required."
                });
        }

        name = name.Trim();

        var existingRole =    await _roleManager.FindByNameAsync(name);

        if (existingRole is not null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "A role with this name already exists."
                });
        }

        var role = new ApplicationRole
        {
            Name = name
        };

        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> UpdateRoleAsync( string roleId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Role name is required."
                });
        }

        var role =
            await _roleManager.FindByIdAsync(roleId);

        if (role is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Role not found."
                });
        }

        name = name.Trim();

        var existingRole =
            await _roleManager.FindByNameAsync(name);

        if (existingRole is not null &&
            existingRole.Id != role.Id)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description =
                        "Another role already uses this name."
                });
        }

        role.Name = name;

        return await _roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(
        string roleId)
    {
        var role =
            await _roleManager.FindByIdAsync(roleId);

        if (role is null)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Role not found."
                });
        }

        var hasUsers = await _context.UserRoles
            .AnyAsync(x => x.RoleId == role.Id);

        if (hasUsers)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description =
                        "This role cannot be deleted because users are assigned to it."
                });
        }

        var permissions = await _context.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .ToListAsync();

        if (permissions.Count > 0)
        {
            _context.RolePermissions.RemoveRange(permissions);
            await _context.SaveChangesAsync();
        }

        return await _roleManager.DeleteAsync(role);
    }
}