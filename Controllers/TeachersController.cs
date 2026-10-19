using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Teacher resource operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Teachers")]
public class TeachersController : ControllerBase
{
    private readonly TeacherService _teacherService;

    public TeachersController(TeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    /// <summary>
    /// Gets a paginated, searchable, and sortable list of teachers with optional department filter.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10, max: 100).</param>
    /// <param name="search">Search term for first name, last name, or email.</param>
    /// <param name="departmentId">Optional department filter.</param>
    /// <returns>Paged list of teachers with total count.</returns>
    /// <response code="200">Returns the paged teacher list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Teachers.View permission.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Teachers.View)]
    [ProducesResponseType(typeof(PagedResult<TeacherListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<TeacherListDto>>> GetTeachers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? departmentId = null)
    {
        var result = await _teacherService.GetPagedAsync(pageNumber, pageSize, search, departmentId);
        return Ok(result);
    }

    /// <summary>
    /// Gets a lightweight teacher lookup list for dropdowns/forms, optionally filtered by department.
    /// </summary>
    /// <param name="departmentId">Optional department filter.</param>
    /// <returns>List of teachers with ID, full name, and department.</returns>
    /// <response code="200">Returns the teacher lookup list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Teachers.View permission.</response>
    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.Teachers.View)]
    [ProducesResponseType(typeof(IEnumerable<TeacherLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<TeacherLookupDto>>> GetLookup([FromQuery] int? departmentId = null)
    {
        var teachers = await _teacherService.GetLookupAsync(departmentId);
        return Ok(teachers);
    }

    /// <summary>
    /// Gets detailed information about a specific teacher by ID.
    /// </summary>
    /// <param name="id">The teacher ID.</param>
    /// <returns>Teacher details including assigned courses.</returns>
    /// <response code="200">Returns the teacher details.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Teachers.View permission.</response>
    /// <response code="404">Teacher not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.Teachers.View)]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherDto>> GetTeacher(int id)
    {
        var teacher = await _teacherService.GetByIdAsync(id);
        if (teacher is null)
        {
            return NotFound(new { message = $"Teacher with id {id} not found." });
        }
        return Ok(teacher);
    }

    /// <summary>
    /// Creates a new teacher.
    /// </summary>
    /// <param name="dto">Teacher creation data.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="201">Teacher created successfully.</response>
    /// <response code="400">Validation failed or business rule violation (e.g., duplicate).</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Teachers.Create permission.</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Teachers.Create)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> CreateTeacher([FromBody] TeacherDto dto)
    {
        try
        {
            await _teacherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetTeacher), new { id = 0 }, new OperationResult { Success = true, Message = "Teacher created successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new OperationResult { Success = false, Message = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new OperationResult { Success = false, Message = "An unexpected error occurred while creating the teacher." });
        }
    }

    /// <summary>
    /// Updates an existing teacher.
    /// </summary>
    /// <param name="id">The teacher ID from route.</param>
    /// <param name="dto">Teacher update data.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="200">Teacher updated successfully.</response>
    /// <response code="400">Validation failed, ID mismatch, or business rule violation.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Teachers.Edit permission.</response>
    /// <response code="404">Teacher not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.Teachers.Edit)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> UpdateTeacher(int id, [FromBody] TeacherDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "Route id does not match payload id." });
        }

        try
        {
            await _teacherService.UpdateAsync(dto);
            return Ok(new OperationResult { Success = true, Message = "Teacher updated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new OperationResult { Success = false, Message = ex.Message });
            }
            return BadRequest(new OperationResult { Success = false, Message = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new OperationResult { Success = false, Message = "An unexpected error occurred while updating the teacher." });
        }
    }

    /// <summary>
    /// Archives (soft-deletes) a teacher.
    /// </summary>
    /// <param name="id">The teacher ID.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Teacher archived successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Teachers.Archive permission.</response>
    /// <response code="404">Teacher not found.</response>
    /// <response code="400">Business rule violation.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.Teachers.Archive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> DeleteTeacher(int id)
    {
        try
        {
            await _teacherService.ArchiveAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new OperationResult { Success = false, Message = ex.Message });
            }
            return BadRequest(new OperationResult { Success = false, Message = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new OperationResult { Success = false, Message = "An unexpected error occurred while archiving the teacher." });
        }
    }
}
