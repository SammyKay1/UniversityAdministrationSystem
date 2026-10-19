
public class RecentEnrollmentDto
{
    public int Id { get; set; }

    public string StudentName { get; set; } = "";

    public string CourseName { get; set; } = "";

    public string CourseCode { get; set; } = "";

    public DateTime EnrollmentDate { get; set; }

    public EnrollmentStatus Status { get; set; }

    public Grade Grade { get; set; }
}