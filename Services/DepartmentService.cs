public class DepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<IEnumerable<DepartmentLookUpDto>>  GetDepartmentsAsync()
    {
        return await _unitOfWork.Departments.GetLookupAsync();
    }


    public async Task<IEnumerable<DepartmentDto>>  GetAllAsync()
    {
        return await _unitOfWork.Departments.GetAllAsync();
    }


    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Departments.GetByIdAsync(id);
    }


    public async Task CreateAsync(DepartmentDto dto)
    {
        var code = dto.Code
            .Trim()
            .ToUpperInvariant();


        var exists =
            await _unitOfWork.Departments
                .CodeExistsAsync(code);


        if (exists)
        {
            throw new InvalidOperationException(
                $"A department with code '{code}' already exists.");
        }


        var department = new Department
        {
            Name = dto.Name.Trim(),

            Code = code,

            Email = dto.Email.Trim(),

            Description = dto.Description.Trim()
        };


        await _unitOfWork.Departments
            .AddAsync(department);


        await _unitOfWork.SaveChangesAsync();
    }


    public async Task UpdateAsync(DepartmentDto dto)
    {
        var department =
            await _unitOfWork.Departments
                .GetEntityByIdAsync(dto.Id);


        if (department is null)
        {
            throw new InvalidOperationException(
                "Department not found.");
        }


        var code = dto.Code
            .Trim()
            .ToUpperInvariant();


        var codeExists =
            await _unitOfWork.Departments
                .CodeExistsAsync(code);


        if (codeExists &&
            !string.Equals(
                department.Code,
                code,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"A department with code '{code}' already exists.");
        }


        department.Name =
            dto.Name.Trim();

        department.Code =
            code;

        department.Email =
            dto.Email.Trim();

        department.Description =
            dto.Description.Trim();


        await _unitOfWork.Departments
            .UpdateAsync(department);


        await _unitOfWork.SaveChangesAsync();
    }


    public async Task ArchiveAsync(int id)
    {
        var department =
            await _unitOfWork.Departments
                .GetEntityByIdAsync(id);

        if (department is null)
        {
            throw new InvalidOperationException(
                "Department not found.");
        }

        await _unitOfWork.Departments
            .ArchiveAsync(id);

        await _unitOfWork.SaveChangesAsync();
    }




    
}