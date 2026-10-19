
using System.ComponentModel.DataAnnotations;

public class TeacherDto
{
    public int Id { get; set; }


    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100)]
    public string FirstName { get; set; } = "";


    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100)]
    public string LastName { get; set; } = "";


    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(100)]
    public string Email { get; set; } = "";


    public DateTime DateOfBirth { get; set; }


    [Required(ErrorMessage = "Department is required.")]
    public int DepartmentId { get; set; }


    public string DepartmentName { get; set; } = "";


    public int CourseCount { get; set; }

    public List<TeacherCourseDto>? Courses { get; set; }
}