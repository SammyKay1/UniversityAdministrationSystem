using FluentValidation.TestHelper;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Validators;

public class StudentEditValidatorTests
{
    private readonly StudentEditValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidStudentEdit_ReturnsSuccess()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
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

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_EmptyFirstName_ReturnsFailure()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "",
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

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task ValidateAsync_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
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

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task ValidateAsync_UnderageStudent_ReturnsFailure()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = DateTime.Today.AddYears(-10),
            DepartmentId = 1,
            Address = new AddressDto
            {
                AddressLine1 = "123 Main St",
                City = "Boston",
                State = "MA",
                Country = "USA"
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Student must be at least 16 years old.");
    }

    [Fact]
    public async Task ValidateAsync_InvalidAddress_ReturnsFailure()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DateOfBirth = DateTime.Today.AddYears(-20),
            DepartmentId = 1,
            Address = new AddressDto
            {
                AddressLine1 = "",
                City = "",
                State = "",
                Country = ""
            }
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address.AddressLine1);
        result.ShouldHaveValidationErrorFor(x => x.Address.City);
        result.ShouldHaveValidationErrorFor(x => x.Address.State);
        result.ShouldHaveValidationErrorFor(x => x.Address.Country);
    }

    [Fact]
    public async Task ValidateAsync_FirstNameTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = new string('A', 51),
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

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task ValidateAsync_LastNameTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new StudentEditDto
        {
            Id = 1,
            FirstName = "John",
            LastName = new string('B', 51),
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

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}