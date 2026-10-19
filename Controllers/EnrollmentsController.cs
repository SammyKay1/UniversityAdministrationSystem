using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Enrollment creation and enrollment state transitions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly EnrollmentService _enrollmentService;

    public EnrollmentsController(EnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Gets all enrollments for a specific student.
    /// </summary>
    /// <param name="studentId">The student ID.</param>
    /// <returns>List of enrollments with course details, status, grade, and enrollment date.</returns>
    /// <response code="200">Returns the student's enrollments.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Enrollments.View permission.</response>
    [HttpGet("student/{studentId:int}")]
    [Authorize(Policy = Permissions.Enrollments.View)]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EnrollmentListDto>>> GetStudentEnrollments(int studentId)
    {
        var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);
        return Ok(enrollments);
    }

    /// <summary>
    /// Gets available courses for enrollment forms with capacity and availability information.
    /// </summary>
    /// <returns>List of courses with code, name, capacity, and available seats.</returns>
    /// <response code="200">Returns the course lookup list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Enrollments.View permission.</response>
    [HttpGet("course-lookup")]
    [Authorize(Policy = Permissions.Enrollments.View)]
    [ProducesResponseType(typeof(IEnumerable<CourseLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CourseLookupDto>>> GetCourseLookup()
    {
        var courses = await _enrollmentService.GetCourseLookupAsync();
        return Ok(courses);
    }

    /// <summary>
    /// Enrolls a student in a course.
    /// </summary>
    /// <param name="dto">Enrollment data with student ID and course ID.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="201">Student enrolled successfully.</response>
    /// <response code="400">Validation failed or student/course not found.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Enrollments.Create permission.</response>
    /// <response code="404">Student or course not found.</response>
    /// <response code="409">Business conflict: duplicate enrollment, course capacity reached, course archived, or student cannot enroll.</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Enrollments.Create)]
    [ProducesResponseType(typeof(EnrollmentResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(EnrollmentResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(EnrollmentResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(EnrollmentResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EnrollmentResult>> EnrollStudent([FromBody] EnrollmentCreateDto dto)
    {
        var result = await _enrollmentService.EnrollStudentAsync(dto);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("already", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("capacity", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("archived", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("cannot be enrolled", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetStudentEnrollments), new { studentId = dto.StudentId }, result);
    }

    /// <summary>
    /// Drops an active enrollment.
    /// </summary>
    /// <param name="id">The enrollment ID.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="200">Enrollment dropped successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Enrollments.Archive permission.</response>
    /// <response code="404">Enrollment not found.</response>
    /// <response code="400">Invalid state transition (enrollment not active).</response>
    [HttpPost("{id:int}/drop")]
    [Authorize(Policy = Permissions.Enrollments.Archive)]
    [ProducesResponseType(typeof(DropEnrollmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DropEnrollmentResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DropEnrollmentResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DropEnrollmentResult>> DropEnrollment(int id)
    {
        var result = await _enrollmentService.DropEnrollmentAsync(id);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Completes an active enrollment.
    /// </summary>
    /// <param name="id">The enrollment ID.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="200">Enrollment completed successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Enrollments.Edit permission.</response>
    /// <response code="404">Enrollment not found.</response>
    /// <response code="400">Invalid state transition (enrollment not active).</response>
    [HttpPost("{id:int}/complete")]
    [Authorize(Policy = Permissions.Enrollments.Edit)]
    [ProducesResponseType(typeof(CompleteEnrollmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CompleteEnrollmentResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(CompleteEnrollmentResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CompleteEnrollmentResult>> CompleteEnrollment(int id)
    {
        var result = await _enrollmentService.CompleteEnrollmentAsync(id);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Assigns a grade to an enrollment.
    /// </summary>
    /// <param name="id">The enrollment ID from route.</param>
    /// <param name="dto">Grade assignment data with enrollment ID and grade.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="200">Grade assigned successfully.</response>
    /// <response code="400">ID mismatch or invalid grade.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Enrollments.Edit permission.</response>
    /// <response code="404">Enrollment not found.</response>
    [HttpPost("{id:int}/grade")]
    [Authorize(Policy = Permissions.Enrollments.Edit)]
    [ProducesResponseType(typeof(AssignGradeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AssignGradeResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AssignGradeResult>> AssignGrade(int id, [FromBody] AssignGradeDto dto)
    {
        if (id != dto.EnrollmentId)
        {
            return BadRequest(new { message = "Route enrollment id does not match payload enrollment id." });
        }

        var result = await _enrollmentService.AssignGradeAsync(dto);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }
}
