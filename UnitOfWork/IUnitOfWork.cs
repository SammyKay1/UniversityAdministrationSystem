
using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }

    IDepartmentRepository Departments { get; }

    ITeacherRepository Teachers { get; }

    ICourseRepository Courses { get; }

    IEnrollmentRepository Enrollments { get; }

    IDashboardRepository Dashboard { get; }
    IReportRepository Report { get; }

    Task<int> SaveChangesAsync();


    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}