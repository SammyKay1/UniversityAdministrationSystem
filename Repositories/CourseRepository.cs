
using Microsoft.EntityFrameworkCore;

public class CourseRepository : Repository<Course>, ICourseRepository
{

   
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
        
    }


    public async Task<IEnumerable<CourseListDto>> GetCourseListsAsync()
    {
        return await _context.Courses.AsNoTracking().Select(c => new CourseListDto{
            Id = c.Id,
            CourseCode = c.CourseCode,
            Name = c.Name,
            CreditHours = c.CreditHours,
            Capacity = c.Capacity,
            DepartmentName = c.Department.Name,
            TeacherName = c.Teacher == null ? null : c.Teacher.FirstName + " " + c.Teacher.LastName,
            IsArchived = c.IsArchived
        }).OrderBy(c => c.CourseCode)
            .ToListAsync();
    }



    public async Task<bool> ExistsByCourseCodeAsync(string courseCode, int? excludeId = null)
    {
        return await _context.Courses
        .AnyAsync(c =>
            c.CourseCode == courseCode &&
            (!excludeId.HasValue || c.Id != excludeId));
    }

    public async Task<Course?> GetByIdForUpdateAsync(int id)
    {
        return await _context.Courses
        .Include(c => c.Enrollments)
        .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId)
    {
        return await _context.Courses
            .Where(c => c.Id == departmentId)
            .ToListAsync();
    }


    public async Task<Course?> GetCourseWithTeacherAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CourseDetailsDto?> GetDetailsAsync(int id)
    {
        return await _context.Courses
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CourseDetailsDto
        {
            Id = c.Id,

            CourseCode = c.CourseCode,

            Name = c.Name,

            Description = c.Description,

            CreditHours = c.CreditHours,

            Capacity = c.Capacity,

            Department = c.Department.Name,

            TeacherName = c.Teacher == null  ? null    : c.Teacher.FirstName + " " + c.Teacher.LastName,

            IsArchived = c.IsArchived
        }).FirstOrDefaultAsync(); 

    }

    public async Task<CourseEditDto?> GetEditDtoAsync(int id)
    {
        return await _context.Courses
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CourseEditDto
        {
            Id = c.Id,

            CourseCode = c.CourseCode,

            Name = c.Name,

            Description = c.Description,

            CreditHours = c.CreditHours,

            Capacity = c.Capacity,

            DepartmentId = c.DepartmentId,
            
            TeacherId = c.TeacherId
        })
        .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CourseLookupDto>> GetLookupAsync()
    {
        return await _context.Courses
            .AsNoTracking()
            .Where( c => !c.IsArchived)
            .OrderBy(c => c.Name)
            .Select(c => new CourseLookupDto
            {
                Id = c.Id,
                
                CourseCode = c.CourseCode,

                CourseName = c.Name,

                Capacity = c.Capacity,

                ActiveEnrollmentCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)
            })
            .ToListAsync();
    }

   
    /*
    public async Task<bool> HasAvailableSeatsAsync(int courseId)
    {
        var course = await _context.Courses
            .FindAsync(courseId);


        return course != null &&
               course.AvailableSeats > 0;
    } */
}