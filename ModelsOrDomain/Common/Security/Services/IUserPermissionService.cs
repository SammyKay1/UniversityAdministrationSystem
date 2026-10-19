
public interface IUserPermissionService
{
    Task<List<string>> GetPermissionsAsync(string userId);

    Task AssignPermissionAsync( string roleId,  int permissionId);

    Task RemovePermissionAsync(string roleId,    int permissionId);
}