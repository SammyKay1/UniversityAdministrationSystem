
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsActiveAsync( int studentId, int courseId)
    {
        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId &&
                                            e.CourseId == courseId &&
                                            e.Status == EnrollmentStatus.Active);
    }

    public async Task<IEnumerable<EnrollmentListDto>> GetStudentsEnrollmentsAsync(int studentId)
    {
        return await _context.Enrollments
           .AsNoTracking()
           .Where(e => e.StudentId == studentId)
           .OrderBy(e => e.Course.Name)
           .Select(e => new EnrollmentListDto
           {
               Id = e.Id,

               CourseName = e.Course.Name,

               CourseCode = e.Course.CourseCode,

               EnrollmentDate = e.EnrollmentDate,

               Grade = e.Grade,

               Status = e.Status
           })
           .ToListAsync();
    }

    public async Task<int> CountActiveByCourseAsync(int courseId)
    {
        return await _context.Enrollments
            .CountAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
    }


    

    public async Task<Enrollment?> GetByIdForUpdateAsync(int id)
    {
        return await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id);
    }




}