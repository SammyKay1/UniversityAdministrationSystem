using Microsoft.EntityFrameworkCore;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // LOOKUP
    // =========================================================

    public async Task<IEnumerable<DepartmentLookUpDto>> GetLookupAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentLookUpDto
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToListAsync();
    }


    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Email = d.Email,
                Description = d.Description
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,

                Name = d.Name,

                Code = d.Code,

                Email = d.Email,

                Description = d.Description,

                StudentCount = d.Students.Count(),

                TeacherCount = d.Teachers.Count()
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // ADD
    // =========================================================

    public async Task<Department> AddAsync(
        Department department)
    {
        await _context.Departments.AddAsync(department);

        return department;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);

        return Task.CompletedTask;
    }


    // =========================================================
    // DELETE/ Archive
    // =========================================================

    public async Task ArchiveAsync(int id)
    {
        var department =  await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            return;

        department.IsDeleted = true;
        department.DeletedDateUtc = DateTime.UtcNow;

        _context.Departments.Update(department);
    }


    // =========================================================
    // Get Department by ID
    // =========================================================

    public async Task<Department?> GetEntityByIdAsync(int id)
    {
        return await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
    }


    // =========================================================
    // Check Department Code duplication
    // =========================================================
    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _context.Departments
            .AnyAsync(d => d.Code == code);
    }


}