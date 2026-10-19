
using UniversityAdministrationSystem.Components;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;




public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Person>().UseTptMappingStrategy();

        //Automatically applying configurations from Assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }


    public DbSet<Person> People => Set<Person>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();


    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();


    //Overiding SaveChangesAsync() to generate new token whenever an update operation is performed
    //This is particularly useful for SQLite since we are not using SQL Server.
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateConcurrencyTokens();

        UpdateAuditInformation();

        return await base.SaveChangesAsync(cancellationToken);
    }



    private void UpdateAuditInformation()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:

                    entry.Entity.CreatedDateUtc = utcNow;
                    entry.Entity.CreatedBy = "System";

                    break;

                case EntityState.Modified:

                    entry.Entity.ModifiedDateUtc = utcNow;
                    entry.Entity.ModifiedBy = "System";

                    break;
            }
        }
    }

    private void UpdateConcurrencyTokens()
    {
        var modifiedPeople = ChangeTracker.Entries<Person>().Where(e => e.State == EntityState.Modified);

        foreach (var entry in modifiedPeople)
        {
            entry.Entity.ConcurrencyToken = Guid.NewGuid();
        }
    }

}
