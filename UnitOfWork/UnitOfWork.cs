
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IDbContextTransaction? _transaction;

    
    public IStudentRepository Students { get; }

    public IDepartmentRepository Departments { get; }

    public ITeacherRepository Teachers { get; }

    public ICourseRepository Courses { get; }

    public IEnrollmentRepository Enrollments { get; }

    public IDashboardRepository Dashboard  { get; }
    public IReportRepository Report  { get; }


    public UnitOfWork(ApplicationDbContext context, IReportRepository reportRepository, IDashboardRepository dashboardRepository, IStudentRepository students, ICourseRepository courses, IEnrollmentRepository enrollment, ITeacherRepository teachers)
    {
        _context = context;
        Students = students;
        Departments = new DepartmentRepository(context);
        Courses = new CourseRepository(context);
        Enrollments = enrollment;
        Teachers = teachers;
        Dashboard = dashboardRepository;
        Report = reportRepository;
    }

    /*
    public UnitOfWork(ApplicationDbContext context, IStudentRepository students, ITeacherRepository teachers, ICourseRepository courses, IEnrollmentRepository enrollments)
    {
        _context = context;

        Students = students;
        //Teachers = teachers;
        //Courses = courses;
        //Enrollments = enrollments;
    } */


    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }


    public async Task BeginTransactionAsync()
    {
        _transaction =
            await _context.Database.BeginTransactionAsync();
    }


    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();

            await _transaction.DisposeAsync();
        }
    }


    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();

            await _transaction.DisposeAsync();
        }
    }


    public void Dispose()
    {
        _transaction?.Dispose();

        _context.Dispose();
    }
}