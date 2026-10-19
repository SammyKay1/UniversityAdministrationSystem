using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Course resource operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Courses")]
public class CoursesController : ControllerBase
{
    private readonly CourseService _courseService;

    public CoursesController(CourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Gets all courses.
    /// </summary>
    /// <returns>List of all courses with details including department, teacher, capacity, and enrollment counts.</returns>
    /// <response code="200">Returns the list of courses.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Courses.View permission.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Courses.View)]
    [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CourseListDto>>> GetCourses()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    /// <summary>
    /// Gets a lightweight course lookup list for enrollment forms.
    /// </summary>
    /// <returns>List of courses with ID, code, name, capacity, and active enrollment count.</returns>
    /// <response code="200">Returns the course lookup list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Courses.View permission.</response>
    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.Courses.View)]
    [ProducesResponseType(typeof(IEnumerable<CourseLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<CourseLookupDto>>> GetLookup()
    {
        var courses = await _courseService.GetLookupAsync();
        return Ok(courses);
    }

    /// <summary>
    /// Gets detailed information about a specific course by ID.
    /// </summary>
    /// <param name="id">The course ID.</param>
    /// <returns>Course details including description, credits, capacity, teacher, and department.</returns>
    /// <response code="200">Returns the course details.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Courses.View permission.</response>
    /// <response code="404">Course not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.Courses.View)]
    [ProducesResponseType(typeof(CourseDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDetailsDto>> GetCourse(int id)
    {
        var course = await _courseService.GetDetailsAsync(id);
        if (course is null)
        {
            return NotFound(new { message = $"Course with id {id} not found." });
        }
        return Ok(course);
    }

/// <summary>
    /// Creates a new course.
    /// </summary>
    /// <param name="dto">Course creation data including code, name, description, credits, capacity, department, and optional teacher.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="201">Course created successfully.</response>
    /// <response code="400">Validation failed or business rule violation (e.g., invalid teacher).</response>
    /// <response code="409">Business conflict (e.g., duplicate course code).</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Courses.Create permission.</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Courses.Create)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> CreateCourse([FromBody] CourseCreateDto dto)
    {
        var result = await _courseService.CreateAsync(dto);
        if (!result.Success)
        {
            if (result.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetCourse), new { id = 0 }, result);
    }

/// <summary>
    /// Updates an existing course including teacher assignment.
    /// </summary>
    /// <param name="id">The course ID from route.</param>
    /// <param name="dto">Course update data including optional teacher ID (validated for department match).</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="200">Course updated successfully.</response>
    /// <response code="400">Validation failed, ID mismatch, or business rule violation (teacher mismatch).</response>
    /// <response code="409">Business conflict (e.g., duplicate course code).</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Courses.Edit permission.</response>
    /// <response code="404">Course not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.Courses.Edit)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> UpdateCourse(int id, [FromBody] CourseEditDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "Route id does not match payload id." });
        }

        var result = await _courseService.UpdateAsync(dto);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

/// <summary>
    /// Archives (soft-deletes) a course.
    /// </summary>
    /// <param name="id">The course ID.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Course archived successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Courses.Archive permission.</response>
    /// <response code="404">Course not found.</response>
    /// <response code="409">Business conflict (course has active enrollments).</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.Courses.Archive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> DeleteCourse(int id)
    {
        var result = await _courseService.ArchiveAsync(id);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("active enrollments", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }
        return NoContent();
    }
}
