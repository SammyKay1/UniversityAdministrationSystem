
public class EnrollmentListDto
{
    public int Id { get; set; }

    public string CourseName { get; set; } = "";

    public string CourseCode { get; set; } = "";

    public DateTime EnrollmentDate { get; set; }

    public Grade Grade { get; set; }

    public EnrollmentStatus Status { get; set; }
}