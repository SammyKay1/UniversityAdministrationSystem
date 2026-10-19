
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepartmentReportDto>>
        GetDepartmentReportAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentReportDto
            {
                DepartmentId = d.Id,

                DepartmentName = d.Name,

                DepartmentCode = d.Code,

                StudentCount = d.Students.Count(),

                TeacherCount = d.Teachers.Count(),

                CourseCount = _context.Courses
                    .Count(c => c.DepartmentId == d.Id),

                EnrollmentCount = _context.Enrollments
                    .Count(e =>
                        e.Course.DepartmentId == d.Id)
            })
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
    }


    public async Task<IEnumerable<EnrollmentReportDto>> GetEnrollmentReportAsync()
    {
        return await _context.Courses
            .AsNoTracking()
            .Select(c => new EnrollmentReportDto
            {
                CourseId = c.Id,

                CourseCode = c.CourseCode,

                CourseName = c.Name,

                DepartmentId = c.Department.Id,

                DepartmentName = c.Department.Name,

                ActiveCount = _context.Enrollments
                    .Count(e =>
                        e.CourseId == c.Id &&
                        e.Status == EnrollmentStatus.Active),

                CompletedCount = _context.Enrollments.Count(e => e.CourseId == c.Id &&
                        e.Status == EnrollmentStatus.Completed),

                DroppedCount = _context.Enrollments.Count(e => e.CourseId == c.Id &&
                                e.Status == EnrollmentStatus.Dropped)
            })
            .OrderBy(x => x.DepartmentName)
            .ThenBy(x => x.CourseCode)
            .ToListAsync();
    }


    public async Task<IEnumerable<AcademicPerformanceReportDto>> GetAcademicPerformanceReportAsync()
    {
        return await _context.Courses.AsNoTracking()
            .Select(c => new AcademicPerformanceReportDto
            {
                CourseId = c.Id,

                CourseCode = c.CourseCode,

                CourseName = c.Name,

                DepartmentId = c.Department.Id,

                DepartmentName = c.Department.Name,

                ACount = _context.Enrollments.Count(e =>  e.CourseId == c.Id &&  e.Grade == Grade.A),

                BCount = _context.Enrollments.Count(e =>   e.CourseId == c.Id &&   e.Grade == Grade.B),

                CCount = _context.Enrollments.Count(e =>  e.CourseId == c.Id &&  e.Grade == Grade.C),

                DCount = _context.Enrollments.Count(e =>  e.CourseId == c.Id &&  e.Grade == Grade.D),

                FCount = _context.Enrollments.Count(e =>  e.CourseId == c.Id &&    e.Grade == Grade.F),

                GradedCount = _context.Enrollments.Count(e => e.CourseId == c.Id && e.Grade != Grade.None),

                PendingGradeCount = _context.Enrollments
                    .Count(e =>
                        e.CourseId == c.Id &&
                        e.Grade == Grade.None),

                FailedCount = _context.Enrollments
                    .Count(e =>
                        e.CourseId == c.Id &&
                        e.Grade == Grade.F),

                PassedCount = _context.Enrollments
                    .Count(e =>
                        e.CourseId == c.Id &&
                        e.Grade != Grade.None &&
                        e.Grade != Grade.F)
            })
            .OrderBy(x => x.DepartmentName)
            .ThenBy(x => x.CourseCode)
            .ToListAsync();
    }







}