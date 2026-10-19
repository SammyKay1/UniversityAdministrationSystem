
public class AcademicPerformanceReportDto
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = "";

    public string CourseName { get; set; } = "";

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = "";

    public int GradedCount { get; set; }

    public int PendingGradeCount { get; set; }

    public int ACount { get; set; }

    public int BCount { get; set; }

    public int CCount { get; set; }

    public int DCount { get; set; }

    public int FCount { get; set; }

    public int FailedCount { get; set; }

    public int PassedCount { get; set; }

    public double PassRate => GradedCount == 0 ? 0 : (double)PassedCount / GradedCount * 100;
}