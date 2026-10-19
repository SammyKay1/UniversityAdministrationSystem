
public interface IReportRepository
{
    Task<IEnumerable<DepartmentReportDto>> GetDepartmentReportAsync();

    Task<IEnumerable<EnrollmentReportDto>>  GetEnrollmentReportAsync();

    Task<IEnumerable<AcademicPerformanceReportDto>>  GetAcademicPerformanceReportAsync();
}