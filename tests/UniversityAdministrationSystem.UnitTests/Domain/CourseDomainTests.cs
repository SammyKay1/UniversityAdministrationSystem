using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Domain;

public class CourseDomainTests
{
    private Course CreateCourse(string courseCode = "CS101", string name = "Test Course", string description = "Description", 
        int creditHours = 3, int capacity = 30, int departmentId = 1)
    {
        return new Course(courseCode, name, description, creditHours, capacity, departmentId);
    }

    private void SetProperty(Course course, string propertyName, object value)
    {
        typeof(Course).GetProperty(propertyName)?.SetValue(course, value);
    }

    private void SetProperty(Enrollment enrollment, string propertyName, object value)
    {
        typeof(Enrollment).GetProperty(propertyName)?.SetValue(enrollment, value);
    }

    private void AddActiveEnrollments(Course course, int count)
    {
        var enrollments = course.Enrollments;
        for (int i = 1; i <= count; i++)
        {
            var enrollment = new Enrollment(i, course.Id);
            SetProperty(enrollment, "Status", EnrollmentStatus.Active);
            enrollments.Add(enrollment);
        }
    }

    [Fact]
    public void Constructor_ValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var course = CreateCourse("CS101", "Intro to CS", "Description", 3, 30, 1);

        // Assert
        course.CourseCode.Should().Be("CS101");
        course.Name.Should().Be("Intro to CS");
        course.Description.Should().Be("Description");
        course.CreditHours.Should().Be(3);
        course.Capacity.Should().Be(30);
        course.DepartmentId.Should().Be(1);
        course.IsArchived.Should().BeFalse();
        course.Enrollments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_TrimsCourseCodeAndName()
    {
        // Arrange & Act
        var course = CreateCourse(" CS101 ", " Intro to CS ", "Description", 3, 30, 1);

        // Assert
        course.CourseCode.Should().Be("CS101");
        course.Name.Should().Be("Intro to CS");
    }

    [Fact]
    public void Constructor_EmptyCourseCode_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("", "Test Course", "Description", 3, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Course code is required.");
    }

    [Fact]
    public void Constructor_WhitespaceCourseCode_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("   ", "Test Course", "Description", 3, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Course code is required.");
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("CS101", "", "Description", 3, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Course name is required.");
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("CS101", "   ", "Description", 3, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Course name is required.");
    }

    [Fact]
    public void Constructor_CreditHoursBelowMinimum_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("CS101", "Test Course", "Description", 0, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Credit hours must be between 1 and 6.");
    }

    [Fact]
    public void Constructor_CreditHoursAboveMaximum_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("CS101", "Test Course", "Description", 7, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Credit hours must be between 1 and 6.");
    }

    [Fact]
    public void Constructor_CapacityZero_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("CS101", "Test Course", "Description", 3, 0, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Capacity must be greater than zero.");
    }

    [Fact]
    public void Constructor_CapacityNegative_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => CreateCourse("CS101", "Test Course", "Description", 3, -1, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Capacity must be greater than zero.");
    }

    [Fact]
    public void Update_ValidParameters_UpdatesProperties()
    {
        // Arrange
        var course = CreateCourse("CS101", "Old Name", "Old Description", 3, 30, 1);

        // Act
        course.Update("CS102", "New Name", "New Description", 4, 40, 2);

        // Assert
        course.CourseCode.Should().Be("CS102");
        course.Name.Should().Be("New Name");
        course.Description.Should().Be("New Description");
        course.CreditHours.Should().Be(4);
        course.Capacity.Should().Be(40);
        course.DepartmentId.Should().Be(2);
    }

    [Fact]
    public void Update_EmptyCourseCode_ThrowsArgumentException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        Action act = () => course.Update("", "Test Course", "Description", 3, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Course code is required.");
    }

    [Fact]
    public void Update_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        Action act = () => course.Update("CS101", "", "Description", 3, 30, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Course name is required.");
    }

    [Fact]
    public void Archive_SetsIsArchivedToTrue()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);
        course.IsArchived.Should().BeFalse();

        // Act
        course.Archive();

        // Assert
        course.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Restore_SetsIsArchivedToFalse()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);
        course.Archive(); // First archive it
        course.IsArchived.Should().BeTrue();

        // Act
        course.Restore();

        // Assert
        course.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void ChangeCapacity_ValidCapacity_UpdatesCapacity()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        course.ChangeCapacity(50);

        // Assert
        course.Capacity.Should().Be(50);
    }

    [Fact]
    public void ChangeCapacity_ZeroCapacity_ThrowsArgumentException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        Action act = () => course.ChangeCapacity(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Capacity must be greater than zero.");
    }

    [Fact]
    public void ChangeCapacity_NegativeCapacity_ThrowsArgumentException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        Action act = () => course.ChangeCapacity(-5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Capacity must be greater than zero.");
    }

    [Fact]
    public void ChangeCapacity_BelowActiveEnrollments_ThrowsInvalidOperationException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);
        
        // Add some active enrollments using reflection
        AddActiveEnrollments(course, 2);

        // Act
        Action act = () => course.ChangeCapacity(1); // Try to set capacity below 2 active enrollments

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Capacity cannot be less than the number of active enrollments.");
    }

    [Fact]
    public void ChangeCapacity_EqualToActiveEnrollments_Succeeds()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);
        
        AddActiveEnrollments(course, 2);

        // Act
        course.ChangeCapacity(2); // Equal to active enrollments

        // Assert
        course.Capacity.Should().Be(2);
    }

    [Fact]
    public void ChangeCreditHours_ValidHours_UpdatesCreditHours()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        course.ChangeCreditHours(4);

        // Assert
        course.CreditHours.Should().Be(4);
    }

    [Fact]
    public void ChangeCreditHours_BelowMinimum_ThrowsArgumentException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        Action act = () => course.ChangeCreditHours(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Credit hours must be between 1 and 6.");
    }

    [Fact]
    public void ChangeCreditHours_AboveMaximum_ThrowsArgumentException()
    {
        // Arrange
        var course = CreateCourse("CS101", "Test Course", "Description", 3, 30, 1);

        // Act
        Action act = () => course.ChangeCreditHours(7);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Credit hours must be between 1 and 6.");
    }
}