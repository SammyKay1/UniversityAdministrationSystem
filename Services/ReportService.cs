
public class ReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DepartmentReportDto>>
        GetDepartmentReportAsync()
    {
        return await _unitOfWork
            .Report.GetDepartmentReportAsync();
    }

    public async Task<IEnumerable<EnrollmentReportDto>>
    GetEnrollmentReportAsync()
    {
        return await _unitOfWork.Report.GetEnrollmentReportAsync();
    }

    public async Task<IEnumerable<AcademicPerformanceReportDto>>
    GetAcademicPerformanceReportAsync()
    {
        return await _unitOfWork.Report.GetAcademicPerformanceReportAsync();
    }

}