
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


public class PermissionRepository  : IPermissionRepository
{

    private readonly ApplicationDbContext _context;


    public PermissionRepository(  ApplicationDbContext context)
    {
        _context = context;
    }



    public async Task<List<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .OrderBy(x => x.Name)
            .ToListAsync();
    }



    public async Task<List<Permission>>  GetPermissionsForRoleAsync(string roleId)
    {

        return await _context.RolePermissions

            .Where(x => x.RoleId == roleId)

            .Include(x => x.Permission)

            .Select(x => x.Permission)

            .ToListAsync();

    }



    public async Task<List<string>>   GetPermissionNamesForUserAsync(string userId)
    {


        var permissions =   await
            (
                from userRole in _context.UserRoles

                join rolePermission
                in _context.RolePermissions

                on userRole.RoleId equals rolePermission.RoleId


                join permission
                in _context.Permissions

                on rolePermission.PermissionId
                equals permission.Id


                where userRole.UserId == userId


                select permission.Name

            )
            .Distinct()
            .ToListAsync();


        return permissions;
    }



    public async Task AssignPermissionAsync( string roleId,  int permissionId)
    {

        var exists =    await _context.RolePermissions
            .AnyAsync(x =>
                x.RoleId == roleId &&
                x.PermissionId == permissionId);


        if(exists)
            return;



        await _context.RolePermissions.AddAsync( new RolePermission(roleId,    permissionId));


    }




    public async Task RemovePermissionAsync( string roleId, int permissionId)
    {

        var entity =
            await _context.RolePermissions
            .FirstOrDefaultAsync(x =>
                x.RoleId == roleId &&
                x.PermissionId == permissionId);



        if(entity != null)
        {
            _context.RolePermissions.Remove(entity);
        }

    }




    public async Task<bool>  RoleHasPermissionAsync(string roleId,  string permissionName)
    {
        return await _context.RolePermissions
            .AnyAsync(x =>
                x.RoleId == roleId &&
                x.Permission.Name == permissionName);

    }

}