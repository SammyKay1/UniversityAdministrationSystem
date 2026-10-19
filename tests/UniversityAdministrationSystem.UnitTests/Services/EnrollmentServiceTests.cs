using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class EnrollmentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<EnrollmentService>> _loggerMock;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<EnrollmentService>>();
        _service = new EnrollmentService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    private Student CreateStudent(int id, StudentStatus status = StudentStatus.Active)
    {
        var student = new Student
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        typeof(Student).GetProperty("Id")?.SetValue(student, id);
        typeof(Student).GetProperty("Status")?.SetValue(student, status);

        return student;
    }

    private Course CreateCourse(int id, string courseCode = "CS101", int capacity = 30, bool isArchived = false)
    {
        var course = new Course(courseCode, "Intro to CS", "Description", 3, capacity, 1);

        typeof(Course).GetProperty("Id")?.SetValue(course, id);
        typeof(Course).GetProperty("IsArchived")?.SetValue(course, isArchived);

        return course;
    }

    [Fact]
    public async Task EnrollStudentAsync_CourseAtCapacity_ReturnsCapacityFailure()
    {
        var student = CreateStudent(1);
        var course = CreateCourse(1, "CS101", 2, false);

        var dto = new EnrollmentCreateDto
        {
            StudentId = 1,
            CourseId = 1
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(x => x.Enrollments.ExistsActiveAsync(1, 1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Enrollments.CountActiveByCourseAsync(1))
            .ReturnsAsync(2);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This course has reached its capacity.");
    }

    [Fact]
    public async Task EnrollStudentAsync_StudentNotFound_ReturnsFailure()
    {
        var dto = new EnrollmentCreateDto
        {
            StudentId = 999,
            CourseId = 1
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Student?)null);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Student not found.");
    }

    [Fact]
    public async Task EnrollStudentAsync_CourseNotFound_ReturnsFailure()
    {
        var student = CreateStudent(1);
        var dto = new EnrollmentCreateDto
        {
            StudentId = 1,
            CourseId = 999
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Course?)null);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course not found.");
    }

    [Fact]
    public async Task EnrollStudentAsync_CourseArchived_ReturnsFailure()
    {
        var student = CreateStudent(1);
        var course = CreateCourse(1, "CS101", 30, true);

        var dto = new EnrollmentCreateDto
        {
            StudentId = 1,
            CourseId = 1
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This course is archived and cannot accept enrollments.");
    }

    [Fact]
    public async Task EnrollStudentAsync_AlreadyEnrolled_ReturnsFailure()
    {
        var student = CreateStudent(1);
        var course = CreateCourse(1, "CS101", 30, false);

        var dto = new EnrollmentCreateDto
        {
            StudentId = 1,
            CourseId = 1
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(x => x.Enrollments.ExistsActiveAsync(1, 1))
            .ReturnsAsync(true);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Student is already actively enrolled in this course.");
    }

    [Fact]
    public async Task EnrollStudentAsync_StudentInactive_ReturnsFailure()
    {
        var student = CreateStudent(1, StudentStatus.Inactive);
        var course = CreateCourse(1, "CS101", 30, false);

        var dto = new EnrollmentCreateDto
        {
            StudentId = 1,
            CourseId = 1
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(x => x.Enrollments.ExistsActiveAsync(1, 1))
            .ReturnsAsync(false);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This student cannot be enrolled.");
    }

    [Fact]
    public async Task EnrollStudentAsync_ValidEnrollment_ReturnsSuccess()
    {
        var student = CreateStudent(1);
        var course = CreateCourse(1, "CS101", 30, false);

        var dto = new EnrollmentCreateDto
        {
            StudentId = 1,
            CourseId = 1
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(x => x.Enrollments.ExistsActiveAsync(1, 1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Enrollments.CountActiveByCourseAsync(1))
            .ReturnsAsync(5);

        _unitOfWorkMock.Setup(x => x.Enrollments.AddAsync(It.IsAny<Enrollment>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.EnrollStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Student enrolled successfully.");
        
        _unitOfWorkMock.Verify(x => x.Enrollments.AddAsync(It.IsAny<Enrollment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignGradeAsync_EnrollmentNotFound_ReturnsFailure()
    {
        var dto = new AssignGradeDto
        {
            EnrollmentId = 999,
            Grade = Grade.A
        };

        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Enrollment?)null);

        var result = await _service.AssignGradeAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Enrollment not found.");
    }

    [Fact]
    public async Task AssignGradeAsync_ValidGrade_ReturnsSuccess()
    {
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Id")?.SetValue(enrollment, 1);

        var dto = new AssignGradeDto
        {
            EnrollmentId = 1,
            Grade = Grade.A
        };

        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(1))
            .ReturnsAsync(enrollment);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.AssignGradeAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Grade assigned successfully.");
        enrollment.Grade.Should().Be(Grade.A);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DropEnrollmentAsync_EnrollmentNotFound_ReturnsFailure()
    {
        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Enrollment?)null);

        var result = await _service.DropEnrollmentAsync(999);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Enrollment not found.");
    }

    [Fact]
    public async Task DropEnrollmentAsync_AlreadyCompleted_ReturnsFailure()
    {
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Id")?.SetValue(enrollment, 1);
        enrollment.Complete();

        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(1))
            .ReturnsAsync(enrollment);

        var result = await _service.DropEnrollmentAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("This enrollment cannot be dropped.");
    }

    [Fact]
    public async Task DropEnrollmentAsync_ValidActive_ReturnsSuccess()
    {
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Id")?.SetValue(enrollment, 1);

        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(1))
            .ReturnsAsync(enrollment);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.DropEnrollmentAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Enrollment dropped successfully.");
        enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteEnrollmentAsync_EnrollmentNotFound_ReturnsFailure()
    {
        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Enrollment?)null);

        var result = await _service.CompleteEnrollmentAsync(999);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Enrollment not found.");
    }

    [Fact]
    public async Task CompleteEnrollmentAsync_NotActive_ReturnsFailure()
    {
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Id")?.SetValue(enrollment, 1);
        enrollment.Drop();

        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(1))
            .ReturnsAsync(enrollment);

        var result = await _service.CompleteEnrollmentAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Only active enrollments can be completed.");
    }

    [Fact]
    public async Task CompleteEnrollmentAsync_ValidActive_ReturnsSuccess()
    {
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Id")?.SetValue(enrollment, 1);

        _unitOfWorkMock.Setup(x => x.Enrollments.GetByIdForUpdateAsync(1))
            .ReturnsAsync(enrollment);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CompleteEnrollmentAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Enrollment completed successfully.");
        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}