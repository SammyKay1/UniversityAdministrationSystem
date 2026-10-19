# UniversityAdministrationSystem.UnitTests

## Purpose

This project contains unit tests for the University Administration System. Unit tests isolate individual classes and business behaviors from external infrastructure (databases, HTTP, authentication, etc.).

## What Qualifies as a Unit Test

- Tests a single class or business behavior in isolation
- Does not require a running web server, real HTTP requests, real database connections, or external services
- Uses mocks/stubs for dependencies that cross infrastructure boundaries (repositories, external services)
- Runs fast (milliseconds per test)
- Tests behavior and business rules, not implementation trivia

## Testing Conventions

### Test Naming

```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `CreateAsync_DuplicateCourseCode_ReturnsFailure`
- `EnrollStudentAsync_WhenCourseIsFull_ReturnsCapacityFailure`
- `AssignGrade_ValidGrade_SetsGrade`

Avoid: `Test1`, `TestCreate`, `ShouldWork`

### Arrange / Act / Assert

Structure every test with clear sections:

```csharp
// Arrange

// Act

// Assert
```

Keep tests focused on one behavior.

### FluentAssertions

Prefer FluentAssertions for readable assertions:

```csharp
result.Should().NotBeNull();
result.Success.Should().BeTrue();
result.Message.Should().Be("Expected message");
```

### Async Testing

All async production methods are tested with async test methods:

```csharp
[Fact]
public async Task MethodName_State_ExpectedBehavior()
{
    // Arrange
    // Act
    await service.SomeMethodAsync();
    // Assert
}
```

Never use `.Result` or `.Wait()` in tests.

### Isolation Strategy

- Repository dependencies are mocked using Moq
- Validators are tested directly (no mocking needed)
- Domain entities are tested directly (no mocking needed)
- Authorization handlers are tested with mocked permission services

## Test Structure

```
tests/UniversityAdministrationSystem.UnitTests/
├── Validators/      # FluentValidation validator tests
├── Services/        # Business service tests (with mocked repositories)
├── Domain/          # Domain entity/business rule tests
├── Security/        # Authorization/permission tests
├── Helpers/         # Test helpers/builders (when needed)
└── UniversityAdministrationSystem.UnitTests.csproj
```

## Running Tests

```powershell
dotnet test
```

Or from the solution root:

```powershell
dotnet test tests/UniversityAdministrationSystem.UnitTests/
```

## Intentionally Excluded

The following are NOT part of unit tests and belong to later phases:

- Integration tests (database, Testcontainers, SQLite in-memory)
- API/HTTP tests (WebApplicationFactory, TestServer)
- Controller tests
- End-to-end tests
- UI tests

## Future Integration Tests

Integration tests will live in:

```
tests/UniversityAdministrationSystem.IntegrationTests/
```