public interface ITeacherRepository
{
    Task<PagedResult<TeacherListDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null,
        int? departmentId = null);

    Task<TeacherDto?> GetByIdAsync(int id);

    Task<Teacher?> GetEntityByIdAsync(int id);

    Task<Teacher> AddAsync(Teacher teacher);

    Task<IEnumerable<TeacherLookupDto>> GetLookupAsync(int? departmentId = null);

    Task UpdateAsync(Teacher teacher);

    Task ArchiveAsync(int id);


}