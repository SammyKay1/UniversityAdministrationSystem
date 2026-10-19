
using System.ComponentModel.DataAnnotations;

public class ProfileDto
{
    public string Email { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = "";
}