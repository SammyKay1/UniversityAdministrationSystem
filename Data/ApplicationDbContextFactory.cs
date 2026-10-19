using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


public class ApplicationDbContextFactory: IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>();

        options.UseSqlite(
            "Data Source=University.db");

        return new ApplicationDbContext(options.Options);
    }
}
