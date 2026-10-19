
using Microsoft.EntityFrameworkCore;

public class PermissionAdministrationService
{
    private readonly ApplicationDbContext _context;

    public PermissionAdministrationService( ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionDto>> GetPermissionsForRoleAsync( string roleId)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        var assignedPermissionIds =
            await _context.RolePermissions
                .Where(x => x.RoleId == roleId)
                .Select(x => x.PermissionId)
                .ToListAsync();

        return permissions
            .Select(permission => new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Group = GetPermissionGroup(permission.Name),
                IsSelected = assignedPermissionIds.Contains(
                    permission.Id)
            })
            .ToList();
    }

    public async Task SavePermissionsAsync( string roleId,  IEnumerable<int> permissionIds)
    {
        var selectedPermissionIds =
            permissionIds.ToHashSet();

        var existingAssignments =
            await _context.RolePermissions
                .Where(x => x.RoleId == roleId)
                .ToListAsync();

        _context.RolePermissions.RemoveRange(
            existingAssignments);

        foreach (var permissionId in selectedPermissionIds)
        {
            await _context.RolePermissions.AddAsync(
                new RolePermission(roleId,  permissionId));
        }

        await _context.SaveChangesAsync();
    }

    private static string GetPermissionGroup(
        string permissionName)
    {
        var separatorIndex =
            permissionName.IndexOf('.');

        if (separatorIndex <= 0)
            return "Other";

        return permissionName[..separatorIndex];
    }
}