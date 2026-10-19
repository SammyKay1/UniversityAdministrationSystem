
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; }  = DateTime.UtcNow;
}