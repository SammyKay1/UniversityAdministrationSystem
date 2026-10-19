
public class DashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<DashboardDto> GetDashboardAsync()
    {
        var dashboard = new DashboardDto
        {
            TotalStudents =   await _unitOfWork.Dashboard.GetStudentCountAsync(),

            TotalTeachers =
                await _unitOfWork.Dashboard
                    .GetTeacherCountAsync(),

            TotalCourses =
                await _unitOfWork.Dashboard
                    .GetCourseCountAsync(),

            TotalDepartments =
                await _unitOfWork.Dashboard
                    .GetDepartmentCountAsync(),

            TotalEnrollments =
                await _unitOfWork.Dashboard
                    .GetEnrollmentCountAsync(),

            ActiveEnrollments =
                await _unitOfWork.Dashboard
                    .GetActiveEnrollmentCountAsync(),

            CompletedEnrollments =
                await _unitOfWork.Dashboard
                    .GetCompletedEnrollmentCountAsync(),

            DroppedEnrollments =
                await _unitOfWork.Dashboard
                    .GetDroppedEnrollmentCountAsync(),

            ArchivedCourses =
                await _unitOfWork.Dashboard
                    .GetArchivedCourseCountAsync(),

            RecentEnrollments =
                await _unitOfWork.Dashboard
                    .GetRecentEnrollmentsAsync(10),

            DepartmentStatistics =
                await _unitOfWork.Dashboard.GetDepartmentStatisticsAsync()
        };

        return dashboard;
    }
}