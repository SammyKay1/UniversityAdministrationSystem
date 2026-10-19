using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityAdministrationSystem.Controllers;

/// <summary>
/// Student resource operations and student-related academic information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Students")]
public class StudentsController : ControllerBase
{
    private readonly StudentService _studentService;
    private readonly TranscriptService _transcriptService;
    private readonly EnrollmentService _enrollmentService;

    public StudentsController(StudentService studentService, TranscriptService transcriptService, EnrollmentService enrollmentService)
    {
        _studentService = studentService;
        _transcriptService = transcriptService;
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Gets a paginated, searchable, and sortable list of students.
    /// </summary>
    /// <param name="request">Pagination, search, and sort parameters.</param>
    /// <returns>Paged list of students with total count.</returns>
    /// <response code="200">Returns the paged student list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.View permission.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Students.View)]
    [ProducesResponseType(typeof(PagedResult<StudentListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<StudentListDto>>> GetStudents([FromQuery] PagedRequest request)
    {
        var result = await _studentService.GetPagedAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Gets detailed information about a specific student by ID.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <returns>Student details including academic information.</returns>
    /// <response code="200">Returns the student details.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.View permission.</response>
    /// <response code="404">Student not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.Students.View)]
    [ProducesResponseType(typeof(StudentDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDetailsDto>> GetStudent(int id)
    {
        var student = await _studentService.GetStudentDetailsAsync(id);
        if (student is null)
        {
            return NotFound(new { message = $"Student with id {id} not found." });
        }
        return Ok(student);
    }

    /// <summary>
    /// Creates a new student.
    /// </summary>
    /// <param name="dto">Student creation data.</param>
    /// <returns>Operation result with success status.</returns>
    /// <response code="201">Student created successfully.</response>
    /// <response code="400">Validation failed or business rule violation.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.Create permission.</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Students.Create)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperationResult>> CreateStudent([FromBody] StudentCreateDto dto)
    {
        var result = await _studentService.CreateStudentAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetStudent), new { id = 0 }, result);
    }

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    /// <param name="id">The student ID from route.</param>
    /// <param name="dto">Student update data including concurrency token.</param>
    /// <returns>Operation result with success status and concurrency information.</returns>
    /// <response code="200">Student updated successfully.</response>
    /// <response code="400">Validation failed, ID mismatch, or business rule violation.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.Edit permission.</response>
    /// <response code="404">Student not found.</response>
    /// <response code="409">Concurrency conflict - record modified by another user.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.Students.Edit)]
    [ProducesResponseType(typeof(UpdateStudentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UpdateStudentResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UpdateStudentResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UpdateStudentResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UpdateStudentResult>> UpdateStudent(int id, [FromBody] StudentEditDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { message = "Route id does not match payload id." });
        }

        var result = await _studentService.UpdateStudentAsync(dto);
        if (!result.Success)
        {
            if (result.HasConcurrencyConflict)
            {
                return Conflict(result);
            }
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

/// <summary>
    /// Archives (soft-deletes) a student.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Student archived successfully.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.Archive permission.</response>
    /// <response code="404">Student not found.</response>
    /// <response code="409">Business conflict (student has active enrollments).</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.Students.Archive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(UpdateStudentResult), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(UpdateStudentResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UpdateStudentResult>> DeleteStudent(int id)
    {
        var result = await _studentService.DeleteStudentAsync(id);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            if (result.Message.Contains("existing enrollments", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(result);
            }
            return BadRequest(result);
        }
        return NoContent();
    }

    /// <summary>
    /// Gets the academic transcript for a specific student.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <returns>Transcript with courses, grades, GPA, and academic standing.</returns>
    /// <response code="200">Returns the student transcript.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.View permission.</response>
    /// <response code="404">Transcript not found for student.</response>
    [HttpGet("{id:int}/transcript")]
    [Authorize(Policy = Permissions.Students.View)]
    [ProducesResponseType(typeof(StudentTranscriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentTranscriptDto>> GetTranscript(int id)
    {
        var transcript = await _transcriptService.GetTranscriptAsync(id);
        if (transcript is null)
        {
            return NotFound(new { message = $"Transcript for student with id {id} not found." });
        }
        return Ok(transcript);
    }

    /// <summary>
    /// Gets the enrollment header for a specific student.
    /// </summary>
    /// <param name="id">The student ID.</param>
    /// <returns>Enrollment header with student summary and status.</returns>
    /// <response code="200">Returns the enrollment header.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.View permission.</response>
    /// <response code="404">Enrollment header not found for student.</response>
    [HttpGet("{id:int}/enrollments")]
    [Authorize(Policy = Permissions.Students.View)]
    [ProducesResponseType(typeof(StudentEnrollmentHeaderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentEnrollmentHeaderDto>> GetEnrollments(int id)
    {
        var header = await _enrollmentService.GetStudentHeaderAsync(id);
        if (header is null)
        {
            return NotFound(new { message = $"Enrollment header for student with id {id} not found." });
        }

        return Ok(header);
    }

    /// <summary>
    /// Gets available departments for student creation/lookup.
    /// </summary>
    /// <returns>List of departments with ID and name.</returns>
    /// <response code="200">Returns the department lookup list.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="403">Forbidden - requires Students.View permission.</response>
    [HttpGet("departments")]
    [Authorize(Policy = Permissions.Students.View)]
    [ProducesResponseType(typeof(IEnumerable<DepartmentLookUpDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DepartmentLookUpDto>>> GetDepartments()
    {
        var departments = await _studentService.GetDepartmentsAsync();
        return Ok(departments);
    }
}
