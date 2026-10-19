using UniversityAdministrationSystem.Components;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University Administration System API",
        Version = "v1",
        Description = "REST API for the University Administration System"
    });

    // Include XML documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Use controller names as tags
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    options.DocInclusionPredicate((_, _) => true);

    // Add ProblemDetails schema
    options.MapType<ProblemDetails>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = "string" },
            ["title"] = new OpenApiSchema { Type = "string" },
            ["status"] = new OpenApiSchema { Type = "integer" },
            ["detail"] = new OpenApiSchema { Type = "string" },
            ["instance"] = new OpenApiSchema { Type = "string" },
            ["errors"] = new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "string" } }
        }
    });

    // Add security requirement documentation (cookie auth)
    options.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Identity.Application",
        Description = "Cookie-based authentication using ASP.NET Core Identity"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CookieAuth" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.SignIn.RequireConfirmedAccount = false;
    
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});


builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();


builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<PermissionSeeder>();


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IRepository<Student>, Repository<Student>>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<TranscriptService>();
builder.Services.AddScoped<GPAService>();
builder.Services.AddScoped<AcademicStandingService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<LoadingService>();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();


builder.Services.AddScoped<TableActionsService>();
builder.Services.AddScoped<AdminLayoutState>();


builder.Services.AddScoped< ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<TeacherService>();

builder.Services.AddScoped<RoleService>();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<IDialogService>(sp => sp.GetRequiredService<DialogService>());

builder.Services.AddScoped<PermissionAdministrationService>();
builder.Services.AddScoped<UserAdministrationService>();


builder.Services.AddScoped<StudentService>();

builder.Services.AddValidatorsFromAssembly(typeof(StudentEditValidator).Assembly);
builder.Services.AddScoped<AuthenticationService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<AccountService>();

builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped< IPermissionService,  PermissionService>();

builder.Services.AddScoped< IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapIdentityEndpoints();
app.MapControllers();


// Only run automatic seeders in non-Testing environments.
// In Testing, seeders are controlled by the test host to ensure migrations run first.
if (!app.Environment.IsEnvironment("Testing"))
{
    using(var scope = app.Services.CreateScope())
    {
        var identitySeeder =  scope.ServiceProvider.GetRequiredService<IdentitySeeder>();

        await identitySeeder.SeedAsync();


        var permissionSeeder =  scope.ServiceProvider.GetRequiredService<PermissionSeeder>();

        await permissionSeeder.SeedAsync();
    }
}


app.Run();