
public class StudentTranscriptHeaderDto
{
    public string StudentName { get; set; } = "";

    public string Department { get; set; } = "";

    public int CompletedCourses { get; set; }

    public int ActiveCourses { get; set; }

    public int DroppedCourses { get; set; }

    public decimal GPA { get; set; }

    public int TotalCreditsEarned { get; set; }

    public int CurrentCredits { get; set; }

    public AcademicStanding AcademicStanding { get; set; }
}