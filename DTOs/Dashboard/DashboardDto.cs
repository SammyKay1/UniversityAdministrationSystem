
public class DashboardDto
{
    public int TotalStudents { get; set; }

    public int TotalTeachers { get; set; }

    public int TotalCourses { get; set; }

    public int TotalDepartments { get; set; }

    public int TotalEnrollments { get; set; }

    public int ActiveEnrollments { get; set; }

    public int CompletedEnrollments { get; set; }

    public int DroppedEnrollments { get; set; }

    public int ArchivedCourses { get; set; }

    public IEnumerable<RecentEnrollmentDto> RecentEnrollments { get; set; }
        = [];

    public IEnumerable<DepartmentStatisticsDto> DepartmentStatistics { get; set; }
        = [];
}