using System.ComponentModel.DataAnnotations;

public class DepartmentDto
{
    public int Id { get; set; }


    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(
        100,
        ErrorMessage =
            "Department name cannot exceed 100 characters.")]
    public string Name { get; set; } = "";


    [Required(ErrorMessage = "Department code is required.")]
    [StringLength(
        10,
        ErrorMessage =
            "Department code cannot exceed 10 characters.")]
    public string Code { get; set; } = "";


    [EmailAddress(
        ErrorMessage =
            "Enter a valid email address.")]
    [StringLength(
        100,
        ErrorMessage =
            "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = "";


    [StringLength( 500,  ErrorMessage =  "Description cannot exceed 500 characters.")]
    public string Description { get; set; } = "";


    public int StudentCount { get; set; }

    public int TeacherCount { get; set; }





}