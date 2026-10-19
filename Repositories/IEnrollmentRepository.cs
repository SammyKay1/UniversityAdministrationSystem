
public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<IEnumerable<EnrollmentListDto>> GetStudentsEnrollmentsAsync(int studentId);
    
    Task<int> CountActiveByCourseAsync(int courseId);

    Task<bool> ExistsActiveAsync(int studentId, int courseId);

    Task<Enrollment?> GetByIdForUpdateAsync(int id);

    
}