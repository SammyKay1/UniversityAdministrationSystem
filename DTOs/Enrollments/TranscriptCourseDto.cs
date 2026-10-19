
public class TranscriptCourseDto
{
    public string CourseCode { get; set; } = "";

    public string CourseName { get; set; } = "";

    public int CreditHours { get; set; }

    public Grade Grade { get; set; }

    public EnrollmentStatus Status { get; set; }

    public DateTime EnrollmentDate { get; set; }
}
