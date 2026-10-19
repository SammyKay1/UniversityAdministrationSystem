public interface IDepartmentRepository
{
    Task<IEnumerable<DepartmentLookUpDto>> GetLookupAsync();

    Task<IEnumerable<DepartmentDto>> GetAllAsync();

    Task<DepartmentDto?> GetByIdAsync(int id);

    Task<Department?> GetEntityByIdAsync(int id);

    Task<bool> CodeExistsAsync(string code);

    Task<Department> AddAsync(Department department);

    Task UpdateAsync(Department department);

    Task ArchiveAsync(int id);
}