

using Microsoft.EntityFrameworkCore;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<int> GetStudentCountAsync()
    {
        return await _context.Students
            .CountAsync();
    }


    public async Task<int> GetTeacherCountAsync()
    {
        return await _context.Teachers
            .CountAsync();
    }


    public async Task<int> GetCourseCountAsync()
    {
        return await _context.Courses
            .CountAsync();
    }


    public async Task<int> GetDepartmentCountAsync()
    {
        return await _context.Departments
            .CountAsync();
    }


    public async Task<int> GetEnrollmentCountAsync()
    {
        return await _context.Enrollments
            .CountAsync();
    }


    public async Task<int> GetActiveEnrollmentCountAsync()
    {
        return await _context.Enrollments
            .CountAsync(e => e.Status == EnrollmentStatus.Active);
    }


    public async Task<int> GetCompletedEnrollmentCountAsync()
    {
        return await _context.Enrollments
            .CountAsync(e => e.Status == EnrollmentStatus.Completed);
    }


    public async Task<int> GetDroppedEnrollmentCountAsync()
    {
        return await _context.Enrollments
            .CountAsync(e => e.Status == EnrollmentStatus.Dropped);
    }


    public async Task<int> GetArchivedCourseCountAsync()
    {
        return await _context.Courses
            .IgnoreQueryFilters()
            .CountAsync(c => c.IsArchived);
    }


    public async Task<IEnumerable<RecentEnrollmentDto>>
        GetRecentEnrollmentsAsync(int count = 10)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .OrderByDescending(e => e.EnrollmentDate)
            .Take(count)
            .Select(e => new RecentEnrollmentDto
            {
                Id = e.Id,

                StudentName =
                    e.Student.FirstName + " " +
                    e.Student.LastName,

                CourseName = e.Course.Name,

                CourseCode = e.Course.CourseCode,

                EnrollmentDate = e.EnrollmentDate,

                Status = e.Status,

                Grade = e.Grade
            })
            .ToListAsync();
    }


    public async Task<IEnumerable<DepartmentStatisticsDto>>   GetDepartmentStatisticsAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentStatisticsDto
            {
                DepartmentId = d.Id,

                DepartmentName = d.Name,

                StudentCount = d.Students.Count(),

                TeacherCount = d.Teachers.Count(),

                CourseCount = _context.Courses.Count(c => c.DepartmentId == d.Id)
            })
            .OrderByDescending(x => x.StudentCount)
            .ToListAsync();
    }
}