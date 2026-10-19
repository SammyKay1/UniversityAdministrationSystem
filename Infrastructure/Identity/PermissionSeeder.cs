using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class PermissionSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public PermissionSeeder( ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await SeedPermissionsAsync();

        await SeedRolePermissionsAsync();
    }

    private async Task SeedPermissionsAsync()
    {
        var existingPermissions =  await _context.Permissions.ToListAsync();

        var existingPermissionNames =  existingPermissions
                .Select(x => x.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newPermissions =  PermissionCatalog.All
                .Where(x => !existingPermissionNames.Contains(x.Name))
                .Select(x => new Permission( x.Name,  x.Description))
                .ToList();

        if (newPermissions.Count == 0)
            return;

        await _context.Permissions.AddRangeAsync(newPermissions);

        await _context.SaveChangesAsync();
    }

    private async Task SeedRolePermissionsAsync()
    {
        var permissions =  await _context.Permissions.ToListAsync();

        foreach (var definition in RolePermissionCatalog.All)
        {
            var role = await _roleManager.FindByNameAsync(definition.RoleName);

            if (role is null)
                continue;

            foreach (var permissionName   in definition.Permissions)
            {
                var permission =permissions.FirstOrDefault( x => x.Name.Equals( permissionName,  StringComparison.OrdinalIgnoreCase));

                if (permission is null)
                    continue;

                var exists = await _context.RolePermissions .AnyAsync(x =>
                            x.RoleId == role.Id &&   x.PermissionId == permission.Id);

                if (exists)
                    continue;

                await _context.RolePermissions.AddAsync( new RolePermission( role.Id,   permission.Id));
            }
        }

        await _context.SaveChangesAsync();
    }
}