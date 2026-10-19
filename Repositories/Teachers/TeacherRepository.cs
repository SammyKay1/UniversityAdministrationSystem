using Microsoft.EntityFrameworkCore;

public class TeacherRepository : ITeacherRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<PagedResult<TeacherListDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, int? departmentId = null)
    {
        var query = _context.Teachers
            .AsNoTracking()
            .Include(t => t.Department)
            .AsQueryable();


        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();

            query = query.Where(t =>
                t.FirstName.Contains(searchTerm) ||
                t.LastName.Contains(searchTerm) ||
                t.Email.Contains(searchTerm));
        }


        if (departmentId.HasValue)
        {
            query = query.Where(t =>
                t.DepartmentId == departmentId.Value);
        }


        var totalCount = await query.CountAsync();


        var items = await query
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TeacherListDto
                {
                    Id = t.Id,

                    FullName = t.FirstName + " " + t.LastName,

                    Email = t.Email,

                    DepartmentName = t.Department.Name,

                    CourseCount = t.Courses.Count()
                })
                .ToListAsync();


        return new PagedResult<TeacherListDto>
        {
            Items = items,

            TotalCount = totalCount,

            PageNumber = pageNumber,

            PageSize = pageSize
        };
    }


    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        return await _context.Teachers
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TeacherDto
            {
                Id = t.Id,

                FirstName = t.FirstName,

                LastName = t.LastName,

                Email = t.Email,

                DateOfBirth = t.DateOfBirth,

                DepartmentId = t.DepartmentId,

                DepartmentName = t.Department.Name,

                CourseCount = t.Courses.Count(),

                Courses = t.Courses
                .OrderBy(c => c.CourseCode)
                .Select(c => new TeacherCourseDto
                {
                    Id = c.Id,

                    CourseCode = c.CourseCode,

                    Name = c.Name,

                    CreditHours = c.CreditHours,

                    Capacity = c.Capacity,

                    EnrollmentCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)
                })
                .ToList()
            })
            .FirstOrDefaultAsync();
    }


    public async Task<Teacher?> GetEntityByIdAsync(int id)
    {
        return await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == id);
    }


    public async Task<Teacher> AddAsync(Teacher teacher)
    {
        await _context.Teachers.AddAsync(teacher);

        return teacher;
    }


    public async Task UpdateAsync(Teacher teacher)
    {
        _context.Teachers.Update(teacher);

        await Task.CompletedTask;
    }


    public async Task ArchiveAsync(int id)
    {
        var teacher =
            await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher is null)
            return;


        teacher.IsDeleted = true;

        teacher.DeletedDateUtc =
            DateTime.UtcNow;


        _context.Teachers.Update(teacher);
    }

    public async Task<IEnumerable<TeacherLookupDto>> GetLookupAsync(
    int? departmentId = null)
    {
        var query = _context.Teachers.AsNoTracking().AsQueryable();


        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId.Value);
        }


        return await query
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new TeacherLookupDto
            {
                Id = t.Id,

                FullName = t.FirstName + " " + t.LastName,

                DepartmentName = t.Department.Name
            }).ToListAsync();
    }
}