using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Dashboard summary and aggregate information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Gets the complete dashboard summary with aggregate counts, recent enrollments, and department statistics.
    /// </summary>
    /// <returns>Dashboard data including total counts for students, teachers, courses, departments, enrollments, recent enrollments, and department statistics.</returns>
    /// <response code="200">Returns the dashboard summary.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Reports.View permission.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }
}
