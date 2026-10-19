
using System.Net;
using FluentValidation;

using Microsoft.EntityFrameworkCore;


public class StudentService
{

    private IUnitOfWork _unitOfWork;
    private readonly IValidator<StudentEditDto> _editValidator;

    private readonly IValidator<StudentCreateDto> _createValidor;

    private readonly ILogger<StudentService> _logger;

    public StudentService(IUnitOfWork unitOfWork, IValidator<StudentEditDto> editValidator, IValidator<StudentCreateDto> createValidator, ILogger<StudentService> logger)
    {
        _unitOfWork = unitOfWork;
        _editValidator = editValidator;
        _createValidor = createValidator;
        _logger = logger;
    }

    /*private readonly List<Student> students = new List<Student>()
    {
        new Student{Id=1, Name="Samuel", Course="Computer Science", Age=35},
        new Student{Id=2, Name="Lon", Course="Data Science", Age=30},
        new Student{Id=3, Name="Kuttin", Course="Computer Architecture", Age=59},
        new Student{Id=4, Name="Kojo", Course="C# Fundermentals", Age=25},
        new Student{Id=5, Name="Manu", Course="Software Engineering", Age=90}
    }; */


    public event Action? StudentChanged;


    /* public async Task<IEnumerable<Student>> GetStudentsAsync()
     {
         await Task.Delay(1000);

         //return Task.FromResult(students);
         return await _unitOfWork.Students.GetAllAsync();
     }  */


    public async Task<IEnumerable<StudentListDto>> GetStudentsAsync()
    {

        return await _unitOfWork.Students.GetStudentListAsync();
    }

    public async Task<StudentDetailsDto?> GetStudentDetailsAsync(int id)
    {
        return await _unitOfWork.Students.GetStudentDetailsAsync(id);
    }

    public void AddStudent(Student student)
    {
        //student.Id = students.Count + 1;
        //students.Add(student);
        //_studentRepository.Add(student);

        StudentChanged?.Invoke();
    }

    public void DeleteStudent(int id)
    {
        /*var student = students.FirstOrDefault( s => s.Id == id);

        if (student != null)
        {
            students.Remove(student);

            StudentChanged?.Invoke();
        } */

        _unitOfWork.Students.Delete(id);
    }

    /*
        public async Task<StudentEditDto?> GetStudentForEditAsync(int id)
        {
            var student =  await _unitOfWork.Students.GetByIdForUpdateAsync(id);

            if(student == null)
                return null;

            return StudentMapper.ToEditDto(student);

        }  */


    public async Task<UpdateStudentResult> UpdateStudentAsync(StudentEditDto dto)
    {

        var validationResult = await _editValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return new UpdateStudentResult
            {
                Success = false,
                Message =
                string.Join(
                    ", ",
                    validationResult.Errors
                    .Select(e => e.ErrorMessage))
            };
        }

        var student = await _unitOfWork.Students.GetByIdForUpdateAsync(dto.Id);

        if (student is null)
        {
            return new UpdateStudentResult
            {
                Success = false,
                Message = "Student not found."
            };
        }

        StudentMapper.UpdateEntity(dto, student);

        /*student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.Email = dto.Email;
        student.DateOfBirth = dto.DateOfBirth;
        student.DepartmentId = dto.DepartmentId; */
        try
        {
            await _unitOfWork.SaveChangesAsync();

            return new UpdateStudentResult
            {
                Success = true,
                Message = "Student Updated Successfully!"
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            var currentStudent = await _unitOfWork.Students.GetByIdForUpdateAsync(dto.Id);


            return new UpdateStudentResult
            {
                Success = false,

                HasConcurrencyConflict = true,

                Message =   "This student record was modified by another user.",

                CurrentStudent = (currentStudent == null) ? null   : new StudentEditDto
                    {
                        Id = currentStudent.Id,

                        FirstName = currentStudent.FirstName,

                        LastName = currentStudent.LastName,

                        Email = currentStudent.Email,

                        DateOfBirth = currentStudent.DateOfBirth,

                        DepartmentId =
                            currentStudent.DepartmentId,

                        ConcurrencyToken =
                            currentStudent.ConcurrencyToken
                    }
            };
        }


    }


    public async Task<OperationResult> CreateStudentAsync(StudentCreateDto dto)
    {
        _logger.LogInformation("Creating student {Email}", dto.Email);

        var validationResult = await _createValidor.ValidateAsync(dto);


        //Console.WriteLine("Inside create student async1.");
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Student validation failed for {Email}", dto.Email);


            return OperationResult.Failure(validationResult.Errors.First().ErrorMessage);
            /*return new UpdateStudentResult
            {
                Success = false,
                Message = string.Join(
                    Environment.NewLine,
                    validationResult.Errors.Select(e => e.ErrorMessage))
            };  */
        }
        //Console.WriteLine("Inside create student async2.");

        try
        {
            var student = StudentMapper.ToEntity(dto);
            //student.Status = StudentStatus.Active;

            await _unitOfWork.Students.AddAsync(student);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Student {student.Id} created.", student.Id);

            return OperationResult.Successful("student created successfully.");

            /*return new UpdateStudentResult
            {
                Success = true,
                Message = "Student created successfully."
            };   */


        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Unexpected arror while creating student.");


            return OperationResult.Failure("An unexpected error occured");


            /*return new UpdateStudentResult
            {
              Success = false,
              Message = "Unexpected Error Ouccured when creating student"
 
            };  */
        }



    }


    public async Task<IEnumerable<DepartmentLookUpDto>> GetDepartmentsAsync()
    {
        return await _unitOfWork.Departments.GetLookupAsync();

    }


    public async Task<StudentEditDto?> GetStudentForEditAsync(int id)
    {
        return await _unitOfWork
            .Students
            .GetEditDtoAsync(id);
    }

    public async Task<UpdateStudentResult> DeleteStudentAsync(int id)
    {
        var hasEnrollments = await _unitOfWork.Students.HasActiveEnrollmentsAsync(id);


        if (hasEnrollments)
        {
            return new UpdateStudentResult
            {
                Success = false,
                Message =
                    "Student cannot be deleted because they have existing enrollments."
            };
        }

        var student =
        await _unitOfWork.Students.GetByIdForUpdateAsync(id);


        if (student == null)
        {
            return new UpdateStudentResult
            {
                Success = false,
                Message = "Student not found."
            };
        }


        await _unitOfWork.Students.SoftDeleteAsync(student);


        await _unitOfWork.SaveChangesAsync();


        return new UpdateStudentResult
        {
            Success = true,
            Message = "Student deleted successfully."
        };
    }


    public async Task<PagedResult<StudentListDto>> GetPagedAsync(PagedRequest request)
    {
        return await
            _unitOfWork.Students.GetPagedAsync(request);
    }


}