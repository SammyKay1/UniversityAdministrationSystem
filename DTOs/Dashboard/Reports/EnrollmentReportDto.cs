
public class EnrollmentReportDto
{
    public int CourseId { get; set; }

    public int DepartmentId { get; set; }

    public EnrollmentStatus Status { get; set; }

    public string CourseCode { get; set; } = "";

    public string CourseName { get; set; } = "";

    public string DepartmentName { get; set; } = "";

    public int ActiveCount { get; set; }

    public int CompletedCount { get; set; }

    public int DroppedCount { get; set; }

    public int TotalCount => ActiveCount + CompletedCount + DroppedCount;
}