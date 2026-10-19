
using System.ComponentModel.DataAnnotations;

public class RoleFormModel
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(100,    MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;
}