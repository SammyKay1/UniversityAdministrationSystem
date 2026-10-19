# UniversityAdministrationSystem.IntegrationTests

## Purpose

This project contains integration tests for the University Administration System. Integration tests verify that the application's components work correctly together with real infrastructure:

* EF Core with SQLite database
* Repository implementations
* Database constraints (unique, foreign key)
* ASP.NET Core Identity
* API endpoints through WebApplicationFactory
* Authentication/Authorization pipeline

## Project Structure

```
tests/
└── UniversityAdministrationSystem.IntegrationTests/
    ├── Fixtures/
    │   ├── DatabaseFixture.cs          # SQLite in-memory database with migrations
    │   ├── IntegrationTestFactory.cs   # WebApplicationFactory with test DB
    │   └── CollectionDefinitions.cs    # xUnit collection fixtures
    ├── Repositories/
    │   ├── StudentRepositoryIntegrationTests.cs
    │   ├── CourseRepositoryIntegrationTests.cs
    │   └── EnrollmentRepositoryIntegrationTests.cs
    ├── Database/
    │   └── DatabaseConstraintTests.cs  # Unique/FK constraints, audit, concurrency
    ├── Identity/                       # (future)
    └── Api/
        ├── StudentsApiIntegrationTests.cs
        ├── CoursesApiIntegrationTests.cs
        ├── AuthIntegrationTests.cs
        └── DTOs for response deserialization
```

## Database Strategy

Uses **SQLite in-memory with shared cache** for test isolation:

* Each test collection gets its own in-memory database
* Database is created fresh per test run
* EF Core migrations applied via `EnsureCreatedAsync`
* Connection kept open for the test lifetime to preserve in-memory DB

## Test Collections

| Collection | Fixture | Scope |
|------------|---------|-------|
| Database | DatabaseFixture | Per test class - repository tests, constraint tests |
| Integration | IntegrationTestFactory | Per test class - API tests |

## Running Tests

```powershell
# From solution root
dotnet test tests/UniversityAdministrationSystem.IntegrationTests/

# With verbose output
dotnet test tests/UniversityAdministrationSystem.IntegrationTests/ --verbosity normal
```

## Configuration

Test database uses:
* Provider: SQLite
* Connection: `Data Source=Test_{Guid};Mode=Memory;Cache=Shared`
* Migrations: Applied via `Database.EnsureCreatedAsync()`

## Key Features Tested

### Repository Layer
- CRUD operations against real EF Core
- Query methods (ExistsByCode, HasActiveEnrollments, etc.)

### Database Constraints
- Unique constraints (CourseCode, Department Code, Email, Enrollment)
- Foreign key constraints (DepartmentId, StudentId, CourseId)
- Concurrency token updates
- Audit field persistence (CreatedDateUtc, ModifiedDateUtc)
- Soft delete persistence

### Identity
- User creation with roles
- Permission seeding
- Login/logout flow

### API
- Authentication (login, logout)
- Authorization (protected endpoints, permissions)
- CRUD operations (students, courses)
- Validation error responses (ProblemDetails)
- Business rule enforcement (capacity, duplicates, archiving)

## Authentication Strategy

Uses **real cookie authentication** with ASP.NET Core Identity:

1. Test client posts to `/api/auth/login` with admin credentials
2. Cookie is automatically stored in HttpClient's CookieContainer
3. Subsequent requests include the auth cookie
4. No JWT or test authentication scheme - respects production architecture

## Production Database Protection

Integration tests **never** connect to `University.db`:

* Each test run creates a unique in-memory database
* Connection string uses `Mode=Memory;Cache=Shared`
* No file-based database is created or accessed

## Extending Tests

When adding new integration tests:

1. Choose appropriate collection (`Database` for repo/constraint, `Integration` for API)
2. Use `IAsyncLifetime` for setup/teardown if needed
3. Use `HttpClient` from factory for API tests
4. Use `_context` from factory scope for direct DB assertions
5. Keep tests focused on one behavior
6. Clean up is automatic via in-memory database disposal