
using System.ComponentModel.DataAnnotations;

public class StudentEditDto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public AddressDto Address { get; set; } = null!;

    public Guid ConcurrencyToken { get; set; }
}