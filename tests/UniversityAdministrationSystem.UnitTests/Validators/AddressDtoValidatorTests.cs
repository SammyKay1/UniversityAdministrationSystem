using FluentValidation.TestHelper;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.UnitTests.Validators;

public class AddressDtoValidatorTests
{
    private readonly AddressDtoValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ValidAddress_ReturnsSuccess()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            AddressLine2 = "Apt 4B",
            City = "Boston",
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_AddressLine2Optional_ReturnsSuccess()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            AddressLine2 = "",
            City = "Boston",
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_EmptyAddressLine1_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "",
            City = "Boston",
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AddressLine1);
    }

    [Fact]
    public async Task ValidateAsync_EmptyCity_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            City = "",
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public async Task ValidateAsync_EmptyState_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            City = "Boston",
            State = "",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public async Task ValidateAsync_EmptyCountry_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            City = "Boston",
            State = "MA",
            Country = ""
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public async Task ValidateAsync_AddressLine1TooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = new string('A', 101),
            City = "Boston",
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AddressLine1);
    }

    [Fact]
    public async Task ValidateAsync_AddressLine2TooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            AddressLine2 = new string('B', 101),
            City = "Boston",
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AddressLine2);
    }

    [Fact]
    public async Task ValidateAsync_CityTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            City = new string('C', 51),
            State = "MA",
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public async Task ValidateAsync_StateTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            City = "Boston",
            State = new string('S', 51),
            Country = "USA"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public async Task ValidateAsync_CountryTooLong_ReturnsFailure()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "123 Main St",
            City = "Boston",
            State = "MA",
            Country = new string('C', 51)
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public async Task ValidateAsync_MultipleFieldsInvalid_ReturnsMultipleFailures()
    {
        // Arrange
        var dto = new AddressDto
        {
            AddressLine1 = "",
            City = "",
            State = "",
            Country = ""
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AddressLine1);
        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.State);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }
}