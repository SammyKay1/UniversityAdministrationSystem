
using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;


    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;


    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;


    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;


    [Required]
    [StringLength(
        100,
        MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;


    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;


    public string? Role { get; set; }
}