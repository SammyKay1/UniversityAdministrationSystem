using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Department resource operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Departments")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _departmentService;

    public DepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Gets all departments with student and teacher counts.
    /// </summary>
    /// <returns>List of departments with code, name, email, description, and counts.</returns>
    /// <response code="200">Returns the list of departments.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Departments.View permission.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Departments.View)]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(departments);
    }

    /// <summary>
    /// Gets a lightweight department lookup list for dropdowns/forms.
    /// </summary>
    /// <returns>List of departments with ID and name.</returns>
    /// <response code="200">Returns the department lookup list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Departments.View permission.</response>
    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.Departments.View)]
    [ProducesResponseType(typeof(IEnumerable<DepartmentLookUpDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DepartmentLookUpDto>>> GetLookup()
    {
        var departments = await _departmentService.GetDepartmentsAsync();
        return Ok(departments);
    }

    /// <summary>
    /// Gets detailed information about a specific department by ID.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <returns>Department details including code, name, email, description, and counts.</returns>
    /// <response code="200">Returns the department details.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Departments.View permission.</response>
    /// <response code="404">Department not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.Departments.View)]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department is null)
        {
            return NotFound(new { message = $"Department with id {id} not found." });
        }
        return Ok(department);
    }

    /// <summary>
    /// Creates a new department.
    /// </summary>
    /// <param name="dto">Department creation data including name, code (normalized to uppercase), email, and description.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="201">Department created successfully.</response>
    /// <response code="400">Validation failed or duplicate department code.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Departments.Create permission.</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Departments.Create)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> CreateDepartment([FromBody] DepartmentDto dto)
    {
        try
        {
            await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetDepartment), new { id = 0 }, new OperationResult { Success = true, Message = "Department created successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new OperationResult { Success = false, Message = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new OperationResult { Success = false, Message = "An unexpected error occurred while creating the department." });
        }
    }

    /// <summary>
    /// Updates an existing department.
    /// </summary>
    /// <param name="id">The department ID from route.</param>
    /// <param name="dto">Department update data.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="200">Department updated successfully.</response>
    /// <response code="400">Validation failed, ID mismatch, or duplicate department code.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Departments.Edit permission.</response>
    /// <response code="404">Department not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.Departments.Edit)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> UpdateDepartment(int id, [FromBody] DepartmentDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "Route id does not match payload id." });
        }

        try
        {
            await _departmentService.UpdateAsync(dto);
            return Ok(new OperationResult { Success = true, Message = "Department updated successfully." });
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
            return BadRequest(new OperationResult { Success = false, Message = "An unexpected error occurred while updating the department." });
        }
    }

    /// <summary>
    /// Archives (soft-deletes) a department.
    /// </summary>
    /// <param name="id">The department ID.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Department archived successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Departments.Archive permission.</response>
    /// <response code="404">Department not found.</response>
    /// <response code="400">Business rule violation.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.Departments.Archive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> DeleteDepartment(int id)
    {
        try
        {
            await _departmentService.ArchiveAsync(id);
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
            return BadRequest(new OperationResult { Success = false, Message = "An unexpected error occurred while archiving the department." });
        }
    }
}
