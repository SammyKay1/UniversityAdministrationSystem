using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private DbConnection? _connection;
    private IServiceProvider? _serviceProvider;

    public string DatabaseName { get; } = $"Test_{Guid.NewGuid():N}";

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection($"Data Source={DatabaseName};Mode=Memory;Cache=Shared");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(_connection);
        });

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        // Seed required reference data
        await SeedReferenceDataAsync(context);
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext context)
    {
        // Create default department if not exists
        if (!await context.Departments.AnyAsync())
        {
            var department = new Department
            {
                Name = "Computer Science",
                Code = "CS",
                Email = "cs@university.edu",
                Description = "Computer Science Department"
            };
            context.Departments.Add(department);
            await context.SaveChangesAsync();
        }
    }

    public ApplicationDbContext CreateContext()
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("DatabaseFixture not initialized. Call InitializeAsync first.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection!)
            .Options;

        return new ApplicationDbContext(options);
    }

    public async Task SeedAsync()
    {
        using var scope = _serviceProvider!.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await identitySeeder.SeedAsync();

        var permissionSeeder = scope.ServiceProvider.GetRequiredService<PermissionSeeder>();
        await permissionSeeder.SeedAsync();
    }

    public T GetRequiredService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("DatabaseFixture not initialized. Call InitializeAsync first.");

        return _serviceProvider.GetRequiredService<T>();
    }

    public async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}