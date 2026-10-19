
public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();

    Task<List<Permission>> GetPermissionsForRoleAsync(string roleId);

    Task<List<string>> GetPermissionNamesForUserAsync(string userId);

    Task AssignPermissionAsync(string roleId, int permissionId);

    Task RemovePermissionAsync(string roleId, int permissionId);

    Task<bool> RoleHasPermissionAsync(string roleId, string permissionName);
}