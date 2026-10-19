
using System.ComponentModel.DataAnnotations;
public class EnrollmentCreateDto
{
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a course.")]
    public int CourseId { get; set; }
}