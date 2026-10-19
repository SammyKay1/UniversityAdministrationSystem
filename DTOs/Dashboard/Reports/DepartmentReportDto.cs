
public class DepartmentReportDto
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = "";

    public string DepartmentCode { get; set; } = "";

    public int StudentCount { get; set; }

    public int TeacherCount { get; set; }

    public int CourseCount { get; set; }

    public int EnrollmentCount { get; set; }
}