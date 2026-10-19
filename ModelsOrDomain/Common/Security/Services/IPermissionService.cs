
public interface IPermissionService
{
    Task<bool> HasPermissionAsync( string permission);

    Task<List<string>> GetCurrentUserPermissionsAsync();


    //Task<List<string>> GetPermissionsAsync(string userId);

    //Task AssignPermissionAsync( string roleId,  int permissionId);

    //Task RemovePermissionAsync( string roleId,  int permissionId);


}