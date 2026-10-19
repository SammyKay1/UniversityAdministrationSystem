
public class UserRolesDto
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public List<string> AvailableRoles { get; set; } = [];

    public List<string> SelectedRoles { get; set; } = [];
}