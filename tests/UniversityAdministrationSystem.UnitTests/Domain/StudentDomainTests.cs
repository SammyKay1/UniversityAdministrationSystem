using System;
using System.Reflection;
using FluentAssertions;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Domain;

public class StudentDomainTests
{
    private Student CreateStudent(StudentStatus status = StudentStatus.Active)
    {
        var student = new Student
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };
        typeof(Student).GetProperty("Status")?.SetValue(student, status);
        return student;
    }

    [Fact]
    public void CanEnroll_ActiveStudent_ReturnsTrue()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Active);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEnroll_PendingStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Pending);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_InactiveStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Inactive);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_SuspendedStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Suspended);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_GraduatedStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Graduated);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_WithdrawnStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Withdrawn);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_ExpelledStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Expelled);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_AlumniStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Alumni);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnroll_ArchivedStudent_ReturnsFalse()
    {
        // Arrange
        var student = CreateStudent(StudentStatus.Archived);

        // Act
        var result = student.CanEnroll();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DefaultStatus_IsActive()
    {
        // Arrange
        var student = new Student
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1
        };

        // Act
        var status = student.Status;

        // Assert
        status.Should().Be(StudentStatus.Active);
    }
}