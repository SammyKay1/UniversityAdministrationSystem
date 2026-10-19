using System.Data.Common;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using UniversityAdministrationSystem;
using Xunit;

namespace UniversityAdministrationSystem.IntegrationTests.Fixtures;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _connection;
    private string _databaseName = $"Test_{Guid.NewGuid():N}";
    private readonly string _projectDir;
    private readonly string _databasePath;

    public IntegrationTestFactory()
    {
        _projectDir = GetProjectDirectory();
        _databasePath = Path.Combine(Path.GetTempPath(), $"UAS_Test_{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(_projectDir);
        builder.UseEnvironment("Testing");

        // Use the test server (in-memory) instead of real Kestrel
        builder.UseTestServer();

        builder.ConfigureTestServices(services =>
        {
            // Use file-based SQLite for better isolation and consistency
            _connection = new SqliteConnection($"Data Source={_databasePath};Cache=Shared");
            _connection.Open();

            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    private static string GetProjectDirectory()
    {
        // Use the known correct path
        var knownPath = @"C:\Users\Kwarteng\Documents\Projects\UAS\UniversityAdministrationSystem";
        if (Directory.Exists(knownPath) && File.Exists(Path.Combine(knownPath, "UniversityAdministrationSystem.csproj")))
        {
            return knownPath;
        }

        // Fallback: try to find from current directory
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var foundDir = currentDir;
        while (foundDir != null && !File.Exists(Path.Combine(foundDir.FullName, "UniversityAdministrationSystem.csproj")))
        {
            foundDir = foundDir.Parent;
        }

        if (foundDir != null && Directory.Exists(foundDir.FullName))
        {
            return foundDir.FullName;
        }

        throw new DirectoryNotFoundException("Could not find UniversityAdministrationSystem project directory");
    }

    public async Task InitializeAsync()
    {
        // Ensure database is migrated
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // Ensure reference data exists
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

        // Manually run the seeders
        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await identitySeeder.SeedAsync();

        var permissionSeeder = scope.ServiceProvider.GetRequiredService<PermissionSeeder>();
        await permissionSeeder.SeedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        // Clean up the database file
        try
        {
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }


    public async Task<HttpClient> CreateAuthenticatedClientAsync(string? email = null, string? password = null)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            // Retrieve the login page to obtain an antiforgery token.
            using var loginPageResponse = await client.GetAsync("/login");
            loginPageResponse.EnsureSuccessStatusCode();

            var html = await loginPageResponse.Content.ReadAsStringAsync();

            var match = System.Text.RegularExpressions.Regex.Match(html,
                @"<input\b(?=[^>]*\bname=""__RequestVerificationToken"")(?=[^>]*\bvalue=""([^""]+)"")[^>]*>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                throw new InvalidOperationException(
                    "Could not find an antiforgery token on the login page.");
            }

            var token = match.Groups[1].Value;

            var formData = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["RememberMe"] = "false",
                ["__RequestVerificationToken"] = token
            };

            using var loginResult = await client.PostAsync(
                "/account/login",
                new FormUrlEncodedContent(formData));

            /*  if (loginResult.StatusCode != System.Net.HttpStatusCode.Redirect ||
                loginResult.Headers.Location?.OriginalString != "/")
            {
                throw new InvalidOperationException(
                    $"Test login failed. Status: {(int)loginResult.StatusCode}; " +
                    $"Location: {loginResult.Headers.Location}");
            } */

            if (loginResult.StatusCode != System.Net.HttpStatusCode.Redirect ||
    loginResult.Headers.Location?.OriginalString != "/")
            {
                var responseBody =
                    await loginResult.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"Test login failed. " +
                    $"Status: {(int)loginResult.StatusCode}; " +
                    $"Location: {loginResult.Headers.Location}; " +
                    $"Final request URI: {loginResult.RequestMessage?.RequestUri}; " +
                    $"Response: {responseBody[..Math.Min(responseBody.Length, 1000)]}");
            }

        }

        return client;
    }

}