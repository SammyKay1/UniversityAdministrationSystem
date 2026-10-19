using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Read-only reporting endpoints.
/// </summary>
[ApiController]
[Route("api/reports")]
[Tags("Reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Gets the department report with student, teacher, course, and enrollment counts per department.
    /// </summary>
    /// <returns>List of department reports with aggregate counts.</returns>
    /// <response code="200">Returns the department report.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Reports.View permission.</response>
    [HttpGet("departments")]
    [Authorize(Policy = Permissions.Reports.View)]
    [ProducesResponseType(typeof(IEnumerable<DepartmentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DepartmentReportDto>>> GetDepartmentReport()
    {
        var report = await _reportService.GetDepartmentReportAsync();
        return Ok(report);
    }

    /// <summary>
    /// Gets the enrollment report with active, completed, and dropped counts per course.
    /// </summary>
    /// <returns>List of enrollment reports per course with status breakdown.</returns>
    /// <response code="200">Returns the enrollment report.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Reports.View permission.</response>
    [HttpGet("enrollments")]
    [Authorize(Policy = Permissions.Reports.View)]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EnrollmentReportDto>>> GetEnrollmentReport()
    {
        var report = await _reportService.GetEnrollmentReportAsync();
        return Ok(report);
    }

    /// <summary>
    /// Gets the academic performance report with grade distribution and pass rates per course.
    /// </summary>
    /// <returns>List of academic performance reports with grade counts, pass/failed counts, and pass rate.</returns>
    /// <response code="200">Returns the academic performance report.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Reports.View permission.</response>
    [HttpGet("academic-performance")]
    [Authorize(Policy = Permissions.Reports.View)]
    [ProducesResponseType(typeof(IEnumerable<AcademicPerformanceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AcademicPerformanceReportDto>>> GetAcademicPerformanceReport()
    {
        var report = await _reportService.GetAcademicPerformanceReportAsync();
        return Ok(report);
    }
}
