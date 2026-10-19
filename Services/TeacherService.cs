public class TeacherService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeacherService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<PagedResult<TeacherListDto>>
        GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            int? departmentId = null)
    {
        return await _unitOfWork.Teachers
            .GetPagedAsync(
                pageNumber,
                pageSize,
                searchTerm,
                departmentId);
    }


    public async Task<TeacherDto?>
        GetByIdAsync(int id)
    {
        return await _unitOfWork.Teachers
            .GetByIdAsync(id);
    }


    public async Task CreateAsync(
        TeacherDto dto)
    {
        var teacher = new Teacher
        {
            FirstName = dto.FirstName.Trim(),

            LastName = dto.LastName.Trim(),

            Email = dto.Email.Trim(),

            DateOfBirth = dto.DateOfBirth,

            DepartmentId = dto.DepartmentId
        };


        await _unitOfWork.Teachers
            .AddAsync(teacher);


        await _unitOfWork.SaveChangesAsync();
    }


    public async Task UpdateAsync(TeacherDto dto)
    {
        var teacher = await _unitOfWork.Teachers
                .GetEntityByIdAsync(dto.Id);


        if (teacher is null)
        {
            throw new InvalidOperationException("Teacher not found.");
        }

        //Mappings
        teacher.FirstName = dto.FirstName.Trim();
        teacher.LastName = dto.LastName.Trim();
        teacher.Email = dto.Email.Trim();
        teacher.DateOfBirth = dto.DateOfBirth;
        teacher.DepartmentId = dto.DepartmentId;


        await _unitOfWork.Teachers.UpdateAsync(teacher);

        await _unitOfWork.SaveChangesAsync();
    }


    public async Task ArchiveAsync(int id)
    {
        var teacher = await _unitOfWork.Teachers.GetEntityByIdAsync(id);

        if (teacher is null)
        {
            throw new InvalidOperationException("Teacher not found.");
        }


        await _unitOfWork.Teachers.ArchiveAsync(id);


        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<TeacherLookupDto>> GetLookupAsync(int? departmentId = null)
    {
        return await _unitOfWork.Teachers.GetLookupAsync(departmentId);
    }




}