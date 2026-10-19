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

public class CourseServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CourseCreateValidator _createValidator;
    private readonly CourseEditValidator _editValidator;
    private readonly CourseService _service;

    public CourseServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _createValidator = new CourseCreateValidator();
        _editValidator = new CourseEditValidator();
        _service = new CourseService(_unitOfWorkMock.Object, _createValidator, _editValidator);
    }

    private Course CreateCourse(int id = 1, string courseCode = "CS101", bool isArchived = false)
    {
        var course = new Course(courseCode, "Intro to CS", "Description", 3, 30, 1);
        
        typeof(Course).GetProperty("Id")?.SetValue(course, id);
        typeof(Course).GetProperty("IsArchived")?.SetValue(course, isArchived);
        
        return course;
    }

    private Teacher CreateTeacher(int id = 1, int departmentId = 1)
    {
        var teacher = new Teacher
        {
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@university.edu",
            DateOfBirth = DateTime.Today.AddYears(-40),
            DepartmentId = departmentId
        };
        
        typeof(Teacher).GetProperty("Id")?.SetValue(teacher, id);
        
        return teacher;
    }

    [Fact]
    public async Task CreateAsync_DuplicateCourseCode_ReturnsFailure()
    {
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Intro to CS",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101"))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course code already exists.");
    }

    [Fact]
    public async Task CreateAsync_InvalidValidation_ReturnsFailure()
    {
        var dto = new CourseCreateDto
        {
            CourseCode = "",
            Name = "Intro to CS",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("must not be empty");
    }

    [Fact]
    public async Task CreateAsync_ValidCourse_ReturnsSuccess()
    {
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Intro to CS",
            Description = "Fundamentals",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = 1
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101"))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.AddAsync(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Course created successfully.");

        _unitOfWorkMock.Verify(x => x.Courses.AddAsync(It.Is<Course>(c => 
            c.CourseCode == "CS101" && 
            c.Name == "Intro to CS" &&
            c.TeacherId == 1)), Times.Once);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidCourseWithoutTeacher_ReturnsSuccess()
    {
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Intro to CS",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = null
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101"))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.AddAsync(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        
        _unitOfWorkMock.Verify(x => x.Courses.AddAsync(It.Is<Course>(c => c.TeacherId == null)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CourseNotFound_ReturnsFailure()
    {
        var dto = new CourseEditDto
        {
            Id = 999,
            CourseCode = "CS101",
            Name = "Updated",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101", 999))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Course?)null);

        var result = await _service.UpdateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course not found.");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCourseCode_ReturnsFailure()
    {
        var dto = new CourseEditDto
        {
            Id = 1,
            CourseCode = "CS102",
            Name = "Updated",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS102", 1))
            .ReturnsAsync(true);

        var result = await _service.UpdateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course code already exists.");
    }

    [Fact]
    public async Task UpdateAsync_TeacherNotFound_ThrowsInvalidOperationException()
    {
        var existingCourse = CreateCourse(1, "CS101");
        
        var dto = new CourseEditDto
        {
            Id = 1,
            CourseCode = "CS101",
            Name = "Updated",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = 999
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101", 1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(existingCourse);

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(999))
            .ReturnsAsync((Teacher?)null);

        var act = async () => await _service.UpdateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The selected teacher does not exist.");
    }

    [Fact]
    public async Task UpdateAsync_TeacherWrongDepartment_ThrowsInvalidOperationException()
    {
        var existingCourse = CreateCourse(1, "CS101");
        var teacher = CreateTeacher(1, 2); // Different department

        var dto = new CourseEditDto
        {
            Id = 1,
            CourseCode = "CS101",
            Name = "Updated",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = 1
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101", 1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(existingCourse);

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(1))
            .ReturnsAsync(teacher);

        var act = async () => await _service.UpdateAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The selected teacher does not belong to the selected department.");
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        var existingCourse = CreateCourse(1, "CS101");
        
        var dto = new CourseEditDto
        {
            Id = 1,
            CourseCode = "CS101",
            Name = "Updated Course",
            Description = "New description",
            CreditHours = 4,
            Capacity = 40,
            DepartmentId = 1,
            TeacherId = null
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101", 1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(existingCourse);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.UpdateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Course updated successfully.");

        existingCourse.Name.Should().Be("Updated Course");
        existingCourse.CreditHours.Should().Be(4);
        existingCourse.Capacity.Should().Be(40);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdateWithTeacher_ReturnsSuccess()
    {
        var existingCourse = CreateCourse(1, "CS101");
        var teacher = CreateTeacher(1, 1); // Same department

        var dto = new CourseEditDto
        {
            Id = 1,
            CourseCode = "CS101",
            Name = "Updated Course",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = 1
        };

        _unitOfWorkMock.Setup(x => x.Courses.ExistsByCourseCodeAsync("CS101", 1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(existingCourse);

        _unitOfWorkMock.Setup(x => x.Teachers.GetEntityByIdAsync(1))
            .ReturnsAsync(teacher);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.UpdateAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        existingCourse.TeacherId.Should().Be(1);
    }

    [Fact]
    public async Task ArchiveAsync_CourseNotFound_ReturnsFailure()
    {
        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Course?)null);

        var result = await _service.ArchiveAsync(999);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course not found.");
    }

    [Fact]
    public async Task ArchiveAsync_HasActiveEnrollments_ReturnsFailure()
    {
        var course = CreateCourse(1, "CS101");
        
        var enrollment1 = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Status")?.SetValue(enrollment1, EnrollmentStatus.Active);
        
        var enrollments = new List<Enrollment> { enrollment1 };
        typeof(Course).GetProperty("Enrollments")?.SetValue(course, enrollments);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        var result = await _service.ArchiveAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot archive a course with active enrollments.");
    }

    [Fact]
    public async Task ArchiveAsync_NoActiveEnrollments_ReturnsSuccess()
    {
        var course = CreateCourse(1, "CS101");
        
        var enrollment1 = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Status")?.SetValue(enrollment1, EnrollmentStatus.Completed);
        
        var enrollment2 = new Enrollment(2, 1);
        typeof(Enrollment).GetProperty("Status")?.SetValue(enrollment2, EnrollmentStatus.Dropped);
        
        var enrollments = new List<Enrollment> { enrollment1, enrollment2 };
        typeof(Course).GetProperty("Enrollments")?.SetValue(course, enrollments);

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.ArchiveAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Course archived.");
        course.IsArchived.Should().BeTrue();
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_EmptyEnrollments_ReturnsSuccess()
    {
        var course = CreateCourse(1, "CS101");
        course.GetType().GetProperty("Enrollments")?.SetValue(course, new List<Enrollment>());

        _unitOfWorkMock.Setup(x => x.Courses.GetByIdForUpdateAsync(1))
            .ReturnsAsync(course);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.ArchiveAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        course.IsArchived.Should().BeTrue();
    }
}