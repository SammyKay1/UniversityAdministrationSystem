using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Services;

public class StudentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<StudentService>> _loggerMock;
    private readonly IValidator<StudentEditDto> _editValidator;
    private readonly IValidator<StudentCreateDto> _createValidator;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StudentService>>();
        _editValidator = new StudentEditValidator();
        _createValidator = new StudentCreateValidator();
        _service = new StudentService(_unitOfWorkMock.Object, _editValidator, _createValidator, _loggerMock.Object);
    }

    private Student CreateStudent(int id = 1, StudentStatus status = StudentStatus.Active)
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

    [Fact]
    public async Task CreateStudentAsync_InvalidValidation_ReturnsFailure()
    {
        var dto = new StudentCreateDto
        {
            FirstName = "",
            LastName = "Doe",
            Email = "invalid-email",
            DateOfBirth = DateTime.Today.AddYears(-10),
            DepartmentId = 0,
            Address = new AddressDto()
        };

        var result = await _service.CreateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateStudentAsync_ValidStudent_ReturnsSuccess()
    {
        var dto = new StudentCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1,
            Address = new AddressDto
            {
                AddressLine1 = "123 Main St",
                City = "Boston",
                State = "MA",
                Country = "USA"
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.AddAsync(It.IsAny<Student>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CreateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("student created successfully.");

        _unitOfWorkMock.Verify(x => x.Students.AddAsync(It.IsAny<Student>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentAsync_StudentNotFound_ReturnsFailure()
    {
        var dto = new StudentEditDto
        {
            Id = 999,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1,
            Address = new AddressDto
            {
                AddressLine1 = "123 Main St",
                City = "Boston",
                State = "MA",
                Country = "USA"
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Student?)null);

        var result = await _service.UpdateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Student not found.");
    }

    [Fact]
    public async Task UpdateStudentAsync_InvalidValidation_ReturnsFailure()
    {
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "",
            LastName = "Doe",
            Email = "invalid-email",
            DateOfBirth = DateTime.Today.AddYears(-10),
            DepartmentId = 1,
            Address = new AddressDto()
        };

        var result = await _service.UpdateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateStudentAsync_ValidUpdate_ReturnsSuccess()
    {
        var student = CreateStudent(1);
        
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            DateOfBirth = DateTime.Today.AddYears(-21),
            DepartmentId = 2,
            Address = new AddressDto
            {
                AddressLine1 = "456 Oak Ave",
                City = "New York",
                State = "NY",
                Country = "USA"
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.UpdateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Student Updated Successfully!");
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentAsync_ConcurrencyConflict_ReturnsFailureWithDetails()
    {
        var student = CreateStudent(1);
        
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            DateOfBirth = DateTime.Today.AddYears(-21),
            DepartmentId = 2,
            Address = new AddressDto
            {
                AddressLine1 = "456 Oak Ave",
                City = "New York",
                State = "NY",
                Country = "USA"
            }
        };

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        var currentStudent = CreateStudent(1);
        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(currentStudent);

        var result = await _service.UpdateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.HasConcurrencyConflict.Should().BeTrue();
        result.Message.Should().Contain("modified by another user");
        result.CurrentStudent.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteStudentAsync_StudentNotFound_ReturnsFailure()
    {
        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(999))
            .ReturnsAsync((Student?)null);

        var result = await _service.DeleteStudentAsync(999);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Student not found.");
    }

    [Fact]
    public async Task DeleteStudentAsync_HasActiveEnrollments_ReturnsFailure()
    {
        var student = CreateStudent(1);
        
        _unitOfWorkMock.Setup(x => x.Students.HasActiveEnrollmentsAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteStudentAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Student cannot be deleted because they have existing enrollments.");
    }

    [Fact]
    public async Task DeleteStudentAsync_NoActiveEnrollments_ReturnsSuccess()
    {
        var student = CreateStudent(1);
        
        _unitOfWorkMock.Setup(x => x.Students.HasActiveEnrollmentsAsync(1))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.Students.GetByIdForUpdateAsync(1))
            .ReturnsAsync(student);

        _unitOfWorkMock.Setup(x => x.Students.SoftDeleteAsync(student))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.DeleteStudentAsync(1);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Student deleted successfully.");

        _unitOfWorkMock.Verify(x => x.Students.SoftDeleteAsync(student), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}