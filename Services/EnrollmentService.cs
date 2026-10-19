
using FluentValidation;

using Microsoft.EntityFrameworkCore;

public class EnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IUnitOfWork unitOfWork, ILogger<EnrollmentService> logger)
    {
        _unitOfWork = unitOfWork;

        _logger = logger;
    }

    public async Task<StudentEnrollmentHeaderDto?> GetStudentHeaderAsync(int id)
    {
        return await _unitOfWork
            .Students
            .GetEnrollmentHeaderAsync(id);
    }

    public async Task<IEnumerable<CourseLookupDto>> GetCourseLookupAsync()
    {
        return await _unitOfWork.Courses.GetLookupAsync();
    }

    public async Task<IEnumerable<EnrollmentListDto>> GetStudentEnrollmentsAsync(int studentId)
    {
        return await _unitOfWork
            .Enrollments
            .GetStudentsEnrollmentsAsync(studentId);
    }


    public async Task<EnrollmentResult> EnrollStudentAsync(  EnrollmentCreateDto dto)
    {
        _logger.LogInformation(
            "Enrolling student {StudentId} into course {CourseId}",
            dto.StudentId,
            dto.CourseId);


        var student =
            await _unitOfWork.Students
                .GetByIdForUpdateAsync(dto.StudentId);


        if (student is null)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = "Student not found."
            };
        }


        var course =
            await _unitOfWork.Courses
                .GetByIdForUpdateAsync(dto.CourseId);


        if (course is null)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = "Course not found."
            };
        }


        if (course.IsArchived)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = "This course is archived and cannot accept enrollments."
            };
        }


        if (!student.CanEnroll())
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = "This student cannot be enrolled."
            };
        }


        var exists =
            await _unitOfWork.Enrollments.ExistsActiveAsync(dto.StudentId, dto.CourseId);


        if (exists)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = "Student is already actively enrolled in this course."
            };
        }


        var activeEnrollmentCount = await _unitOfWork.Enrollments.CountActiveByCourseAsync(dto.CourseId);


        if (activeEnrollmentCount >= course.Capacity)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = "This course has reached its capacity."
            };
        }


        var enrollment =
            new Enrollment(
                dto.StudentId,
                dto.CourseId);


        await _unitOfWork.Enrollments
            .AddAsync(enrollment);


        await _unitOfWork.SaveChangesAsync();


        _logger.LogInformation(
            "Enrollment successful. Student {StudentId}, Course {CourseId}",
            dto.StudentId,
            dto.CourseId);


        return new EnrollmentResult
        {
            Success = true,
            Message = "Student enrolled successfully."
        };
    }


    public async Task<AssignGradeResult> AssignGradeAsync(AssignGradeDto dto)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByIdForUpdateAsync(dto.EnrollmentId);


        if (enrollment is null)
        {
            return new AssignGradeResult
            {
                Success = false,
                Message = "Enrollment not found."
            };
        }


        enrollment.AssignGrade(dto.Grade);


        await _unitOfWork.SaveChangesAsync();


        return new AssignGradeResult
        {
            Success = true,
            Message = "Grade assigned successfully."
        };
    }

    public async Task<DropEnrollmentResult> DropEnrollmentAsync(
        int enrollmentId)
    {
        var enrollment =
            await _unitOfWork.Enrollments
                .GetByIdForUpdateAsync(enrollmentId);

        if (enrollment is null)
        {
            return new DropEnrollmentResult
            {
                Success = false,
                Message = "Enrollment not found."
            };
        }

        if (!enrollment.CanBeDropped())
        {
            return new DropEnrollmentResult
            {
                Success = false,
                Message = "This enrollment cannot be dropped."
            };
        }

        enrollment.Drop();

        await _unitOfWork.SaveChangesAsync();

        return new DropEnrollmentResult
        {
            Success = true,
            Message = "Enrollment dropped successfully."
        };
    }

    public async Task<CompleteEnrollmentResult> CompleteEnrollmentAsync( int enrollmentId)
    {
        var enrollment =  await _unitOfWork.Enrollments .GetByIdForUpdateAsync(enrollmentId);

        if (enrollment is null)
        {
            return new CompleteEnrollmentResult
            {
                Success = false,
                Message = "Enrollment not found."
            };
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            return new CompleteEnrollmentResult
            {
                Success = false,
                Message = "Only active enrollments can be completed."
            };
        }

        enrollment.Complete();

        await _unitOfWork.SaveChangesAsync();

        return new CompleteEnrollmentResult
        {
            Success = true,
            Message = "Enrollment completed successfully."
        };
    }




}