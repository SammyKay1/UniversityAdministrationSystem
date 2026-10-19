
using Microsoft.AspNetCore.Mvc.RazorPages;

public interface IStudentRepository : IRepository<Student>
{

    //Task<IEnumerable<Student>> GetActiveStudentsAsync();

    Task<Student?> GetStudentWithCoursesAsync(int id);

    Task<StudentDetailsDto?> GetStudentDetailsAsync(int id);
    Task<Student?> GetForEditAsync(int id);
    //void Add(Student student);
    //void Update(Student student);
    void Delete(int id);

    Task<IEnumerable<StudentListDto>> GetStudentsAsync();

    Task<PagedResult<StudentListDto>> SearchAsync(StudentSearchCriteria criteria);

    Task<IEnumerable<StudentListDto>> GetStudentListAsync();

    Task<Student?> GetByIdForUpdateAsync(int id);

    Task<StudentEditDto?> GetEditDtoAsync(int id);

    Task SoftDeleteAsync(Student student);

    Task<bool> HasActiveEnrollmentsAsync(int studentId);

    Task<StudentEnrollmentHeaderDto?> GetEnrollmentHeaderAsync(int studentId);

    Task<StudentTranscriptDto?> GetTranscriptAsync(int studentId);

    Task<PagedResult<StudentListDto>> GetPagedAsync(PagedRequest request);

    
}