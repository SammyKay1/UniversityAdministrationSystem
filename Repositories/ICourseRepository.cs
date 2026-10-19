
public interface ICourseRepository : IRepository<Course>
{
    Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId);


    Task<Course?>  GetCourseWithTeacherAsync(int id);

    Task<IEnumerable<CourseLookupDto>> GetLookupAsync();

    Task<CourseEditDto?> GetEditDtoAsync(int id);

    Task<CourseDetailsDto?> GetDetailsAsync(int id);

    Task<IEnumerable<CourseListDto>> GetCourseListsAsync();


    Task<Course?> GetByIdForUpdateAsync(int id);

    Task<bool> ExistsByCourseCodeAsync( string courseCode, int? excludeId = null);

    //Task<Course?> GetByIdAsync(int id);


    //Task<bool> HasAvailableSeatsAsync(int courseId);
}