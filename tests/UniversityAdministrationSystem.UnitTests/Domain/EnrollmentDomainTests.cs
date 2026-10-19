using System;
using FluentAssertions;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Domain;

public class EnrollmentDomainTests
{
    [Fact]
    public void Constructor_ValidIds_SetsPropertiesCorrectly()
    {
        // Arrange
        var studentId = 1;
        var courseId = 2;

        // Act
        var enrollment = new Enrollment(studentId, courseId);

        // Assert
        enrollment.StudentId.Should().Be(studentId);
        enrollment.CourseId.Should().Be(courseId);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.Grade.Should().Be(Grade.None);
        enrollment.EnrollmentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AssignGrade_ValidGradeA_SetsGrade()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        enrollment.AssignGrade(Grade.A);

        // Assert
        enrollment.Grade.Should().Be(Grade.A);
    }

    [Fact]
    public void AssignGrade_ValidGradeB_SetsGrade()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        enrollment.AssignGrade(Grade.B);

        // Assert
        enrollment.Grade.Should().Be(Grade.B);
    }

    [Fact]
    public void AssignGrade_ValidGradeC_SetsGrade()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        enrollment.AssignGrade(Grade.C);

        // Assert
        enrollment.Grade.Should().Be(Grade.C);
    }

    [Fact]
    public void AssignGrade_ValidGradeD_SetsGrade()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        enrollment.AssignGrade(Grade.D);

        // Assert
        enrollment.Grade.Should().Be(Grade.D);
    }

    [Fact]
    public void AssignGrade_ValidGradeF_SetsGrade()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        enrollment.AssignGrade(Grade.F);

        // Assert
        enrollment.Grade.Should().Be(Grade.F);
    }

    [Fact]
    public void AssignGrade_NoneGrade_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        var action = () => enrollment.AssignGrade(Grade.None);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("A valid grade must be assigned.");
    }

    [Fact]
    public void Drop_ActiveEnrollment_ChangesStatusToDropped()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);

        // Act
        enrollment.Drop();

        // Assert
        enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
    }

    [Fact]
    public void Drop_CompletedEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Complete(); // Now status is Completed

        // Act
        var action = () => enrollment.Drop();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only active enrollments can be dropped.");
    }

    [Fact]
    public void Drop_DroppedEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Drop(); // Now status is Dropped

        // Act
        var action = () => enrollment.Drop();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only active enrollments can be dropped.");
    }

    [Fact]
    public void Drop_WithdrawnEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Status")?.SetValue(enrollment, EnrollmentStatus.Withdrawn);

        // Act
        var action = () => enrollment.Drop();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only active enrollments can be dropped.");
    }

    [Fact]
    public void Complete_ActiveEnrollment_ChangesStatusToCompleted()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);

        // Act
        enrollment.Complete();

        // Assert
        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
    }

    [Fact]
    public void Complete_DroppedEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Drop(); // Now status is Dropped

        // Act
        var action = () => enrollment.Complete();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only active enrollments can be completed.");
    }

    [Fact]
    public void Complete_CompletedEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Complete(); // Now status is Completed

        // Act
        var action = () => enrollment.Complete();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only active enrollments can be completed.");
    }

    [Fact]
    public void Complete_WithdrawnEnrollment_ThrowsInvalidOperationException()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Status")?.SetValue(enrollment, EnrollmentStatus.Withdrawn);

        // Act
        var action = () => enrollment.Complete();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only active enrollments can be completed.");
    }

    [Fact]
    public void CanBeDropped_ActiveEnrollment_ReturnsTrue()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);

        // Act
        var result = enrollment.CanBeDropped();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeDropped_CompletedEnrollment_ReturnsFalse()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Complete();

        // Act
        var result = enrollment.CanBeDropped();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeDropped_DroppedEnrollment_ReturnsFalse()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        enrollment.Drop();

        // Act
        var result = enrollment.CanBeDropped();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeDropped_WithdrawnEnrollment_ReturnsFalse()
    {
        // Arrange
        var enrollment = new Enrollment(1, 1);
        typeof(Enrollment).GetProperty("Status")?.SetValue(enrollment, EnrollmentStatus.Withdrawn);

        // Act
        var result = enrollment.CanBeDropped();

        // Assert
        result.Should().BeFalse();
    }
}