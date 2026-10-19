public class RolePermission : AuditableEntity
{
    public int Id { get; set; }

    public string RoleId { get; private set; } = string.Empty;

    public int PermissionId { get; private set; }

    public ApplicationRole Role { get; private set; } = default!;

    public Permission Permission { get; private set; } = default!;

    private RolePermission() { }

    public RolePermission(string roleId, int permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}