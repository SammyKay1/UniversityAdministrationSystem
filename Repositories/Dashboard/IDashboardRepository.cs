

public interface IDashboardRepository
{
    Task<int> GetStudentCountAsync();

    Task<int> GetTeacherCountAsync();

    Task<int> GetCourseCountAsync();

    Task<int> GetDepartmentCountAsync();

    Task<int> GetEnrollmentCountAsync();

    Task<int> GetActiveEnrollmentCountAsync();

    Task<int> GetCompletedEnrollmentCountAsync();

    Task<int> GetDroppedEnrollmentCountAsync();

    Task<int> GetArchivedCourseCountAsync();

    Task<IEnumerable<RecentEnrollmentDto>> GetRecentEnrollmentsAsync(
        int count = 10);

    Task<IEnumerable<DepartmentStatisticsDto>>
        GetDepartmentStatisticsAsync();
}