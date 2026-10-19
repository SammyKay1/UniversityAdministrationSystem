
public class CourseService : BaseService, ICrudService<CourseCreateDto, CourseEditDto, CourseListDto>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly CourseCreateValidator _createValidator;

    private readonly CourseEditValidator _editValidator;

    public CourseService( IUnitOfWork unitOfWork, CourseCreateValidator createValidator, CourseEditValidator editValidator)
    {
        _unitOfWork = unitOfWork;

        _createValidator = createValidator;

        _editValidator = editValidator;
    }

    public async Task<OperationResult> CreateAsync(CourseCreateDto dto)
    {
        var validation =   await ValidateAsync(_createValidator, dto);

        //Console.WriteLine($"Course code: {dto.CourseCode}");
        

        if (validation is not null)
            return validation;

        //Console.WriteLine("Inside course create 1");

        if (await _unitOfWork.Courses.ExistsByCourseCodeAsync(dto.CourseCode))
        {
            return OperationResult.Failure(
                "Course code already exists.");
        }
        //Console.WriteLine("Inside course create 2");

        var course = new Course(
            dto.CourseCode,
            dto.Name,
            dto.Description,
            dto.CreditHours,
            dto.Capacity,
            dto.DepartmentId);

            course.TeacherId = dto.TeacherId;

        await _unitOfWork.Courses.AddAsync(course);

        await _unitOfWork.SaveChangesAsync();

        return OperationResult.Successful(
            "Course created successfully.");
    }

    public async Task<OperationResult> UpdateAsync(
        CourseEditDto dto)
    {
        var validation =
            await ValidateAsync(_editValidator, dto);

        if (validation is not null)
            return validation;

        if (await _unitOfWork.Courses
            .ExistsByCourseCodeAsync(dto.CourseCode, dto.Id))
        {
            return OperationResult.Failure(
                "Course code already exists.");
        }

        var course =
            await _unitOfWork.Courses
                .GetByIdForUpdateAsync(dto.Id);

        if (course is null)
            return OperationResult.Failure(
                "Course not found.");

        if (dto.TeacherId.HasValue)
    {
        var teacher =
            await _unitOfWork.Teachers
                .GetEntityByIdAsync(dto.TeacherId.Value);

        if (teacher is null)
        {
            throw new InvalidOperationException(
                "The selected teacher does not exist.");
        }


        if (teacher.DepartmentId != dto.DepartmentId)
        {
            throw new InvalidOperationException(
                "The selected teacher does not belong to " +
                "the selected department.");
        }
    }

        course.Update(
            dto.CourseCode,
            dto.Name,
            dto.Description,
            dto.CreditHours,
            dto.Capacity,
            dto.DepartmentId);

            course.TeacherId = dto.TeacherId;

        await _unitOfWork.SaveChangesAsync();

        return OperationResult.Successful(
            "Course updated successfully.");
    }

    public async Task<OperationResult> ArchiveAsync(
        int id)
    {
        var course =
            await _unitOfWork.Courses
                .GetByIdForUpdateAsync(id);

        if (course is null)
            return OperationResult.Failure(
                "Course not found.");

        if (course.Enrollments.Any(e =>
            e.Status == EnrollmentStatus.Active))
        {
            return OperationResult.Failure(
                "Cannot archive a course with active enrollments.");
        }

        course.Archive();

        await _unitOfWork.SaveChangesAsync();

        return OperationResult.Successful(
            "Course archived.");
    }

    public async Task<IEnumerable<CourseListDto>> GetAllAsync()
    {
        return await _unitOfWork.Courses.GetCourseListsAsync();
    }

    public async Task<CourseEditDto?> GetForEditAsync(int id)
    {
        return await _unitOfWork.Courses.GetEditDtoAsync(id);
    }

    public async Task<CourseDetailsDto?> GetDetailsAsync(int id)
    {
        return await _unitOfWork
            .Courses
            .GetDetailsAsync(id);
    }

    public async Task<IEnumerable<CourseLookupDto>> GetLookupAsync()
    {
        return await _unitOfWork
            .Courses
            .GetLookupAsync();
    }






}