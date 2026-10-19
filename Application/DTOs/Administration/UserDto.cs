
public class UserDto
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool IsLockedOut { get; set; }

    public bool EmailConfirmed { get; set; }

    public List<string> Roles { get; set; } = [];

    public string FullName =>
        string.Join(" ",  new[]
            {
                FirstName,
                LastName
            }.Where(x =>!string.IsNullOrWhiteSpace(x)));
}