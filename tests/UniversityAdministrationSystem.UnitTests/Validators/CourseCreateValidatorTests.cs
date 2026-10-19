using FluentValidation.TestHelper;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Validators;

public class CourseCreateValidatorTests
{
    private readonly CourseCreateValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidCourse_ReturnsSuccess()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            Description = "Fundamentals of programming",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_ValidCourseWithoutTeacher_ReturnsSuccess()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            Description = "Fundamentals of programming",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1,
            TeacherId = null
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_EmptyCourseCode_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "",
            Name = "Introduction to Computer Science",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CourseCode);
    }

    [Fact]
    public async Task ValidateAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task ValidateAsync_CourseCodeTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = new string('C', 21),
            Name = "Introduction to Computer Science",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CourseCode);
    }

    [Fact]
    public async Task ValidateAsync_NameTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = new string('N', 151),
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task ValidateAsync_DescriptionTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            Description = new string('D', 501),
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task ValidateAsync_CreditHoursBelowMinimum_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 0,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreditHours);
    }

    [Fact]
    public async Task ValidateAsync_CreditHoursAboveMaximum_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 7,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreditHours);
    }

    [Fact]
    public async Task ValidateAsync_CreditHoursAtMinimum_ReturnsSuccess()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 1,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CreditHours);
    }

    [Fact]
    public async Task ValidateAsync_CreditHoursAtMaximum_ReturnsSuccess()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 6,
            Capacity = 30,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CreditHours);
    }

    [Fact]
    public async Task ValidateAsync_CapacityZero_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 3,
            Capacity = 0,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public async Task ValidateAsync_CapacityNegative_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 3,
            Capacity = -1,
            DepartmentId = 1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public async Task ValidateAsync_DepartmentIdZero_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = 0
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public async Task ValidateAsync_DepartmentIdNegative_ReturnsFailure()
    {
        // Arrange
        var dto = new CourseCreateDto
        {
            CourseCode = "CS101",
            Name = "Introduction to Computer Science",
            CreditHours = 3,
            Capacity = 30,
            DepartmentId = -1
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }
}