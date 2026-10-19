
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public class StudentRepository : Repository<Student>, IStudentRepository
{

    //private readonly ApplicationDbContext _context;
    public StudentRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {

    }


    /*
    public void Add(Student student)
    {
        student.Id = students.Any() ? students.Max(s => s.Id + 1) : 1;
        _context.Students.Add(student);
    }  */

    public void Delete(int id)
    {
        var student = GetById(id);

        if (student != null)
        {
            _context.Students.Remove(student);
        }
    }

    /*public List<Student> GetAll()
    {
        
        return students;
    }   */

    /*
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students.ToListAsync();
    } */

    public Student? GetById(int id)
    {
        return _context.Students.FirstOrDefault(s => s.Id == id);
    }

    /*
    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
    } */


    /*
    public void Update(Student student)
    {
        var existing = GetById(student.Id);

        if (existing == null)   
        {
            return;
        }

        existing.FirstName = student.FirstName;
        existing.LastName = student.LastName;
        existing.Course = student.Course;
        existing.Age = student.Age;
    }
        */
    public async Task<StudentDetailsDto?> GetStudentDetailsAsync(int id)
    {
        return await _context.Students
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new StudentDetailsDto
                {
                    Id = s.Id,
                    FullName = s.FirstName + " " + s.LastName,
                    Email = s.Email,
                    DateOfBirth = s.DateOfBirth,
                    Department = s.Department.Name,
                    Address = s.Address != null ? s.Address.AddressLine1 + ", " + s.Address.City : "No Address",
                    Courses = s.Enrollments.Select(e => e.Course.Name).ToList()

                })
                .FirstOrDefaultAsync();
    }

    public async Task<Student?> GetForEditAsync(int id)
    {
        return await _context.Students.FirstOrDefaultAsync( s => s.Id == id);
    }



    public async Task<IEnumerable<StudentListDto>> GetStudentsAsync()
    {
        return await _context.Students.AsNoTracking().Select(s => new StudentListDto
        {
            Id = s.Id,
            FullName = s.FirstName + " " + s.LastName,
            Department = s.Department.Name,
            Status = s.Status

        }).ToListAsync();
    }

    /*public async Task<PagedResult<StudentListDto>> SearchAsync(StudentSearchCriteria criteria)
    {
        //start building the query
        var query = _context.Students.AsNoTracking()
                        .AsQueryable();

        
        //Search by Name
        if (!string.IsNullOrWhiteSpace(criteria.Name))  
        {
            query = query.Where(s => 
                            s.FirstName.Contains(criteria.Name) ||
                            s.LastName.Contains(criteria.Name));
        }

        
        //Search by department id
        if (criteria.DepartmentId.HasValue)
        {
            query = query.Where(s => s.DepartmentId == criteria.DepartmentId);
        }



        //Search by student status
        if (criteria.Status.HasValue)
        {
            query = query.Where(s => s.Status == criteria.Status);
        }


        //Search by age
        /*if (criteria.Age.HasValue)
        {
            query = query.Where(s => s.Age == criteria.Age);
        } 


        //Sorting by selection:-
        //Suppose the user selects sort by:
        //Name, Age, Department

        query = criteria.SortBy switch
        {
            "Name" => criteria.Descending ? query.OrderByDescending(s => s.FirstName) : query.OrderBy(s => s.FirstName),
            //"Age" => criteria.Descending ? query.OrderByDescending(s => s.Age) : query.OrderBy(s => s.Age),
            "Department" => criteria.Descending ? query.OrderByDescending(s => s.Department.Name) : query.OrderBy(s => s.Department.Name),
            _ => query.OrderBy(s => s.Id)
            
        };


        //Count before pagination
        var totalRecords = await query.CountAsync();

        //Pagination
        query = query.Skip((criteria.Page -1) * criteria.PageSize)
                        .Take(criteria.PageSize);

        
        //Projection
        var students = await query
                                .Select(s => new StudentListDto
                                {
                                    Id = s.Id,
                                    FullName = s.FirstName + " " + s.LastName,
                                    Department = s.Department.Name,
                                    Status = s.Status
                                })
                                .ToListAsync();


        //Return a paged result
        return new PagedResult<StudentListDto>
        {
          Items = students,
          TotalRecords = totalRecords,
          CurrentPage = criteria.Page,
          PageSize = criteria.PageSize  
        };
        
    }  */

    /*private readonly List<Student> students = new()
    {
        new Student
        {
            Id = 1,
            Name = "Samuel",
            Course = "Computer Science",
            Age = 78
        },
        new Student{Id=2, Name="Lon", Course="Data Science", Age=30},
        new Student{Id=3, Name="Kuttin", Course="Computer Architecture", Age=59},
        new Student{Id=4, Name="Kojo", Course="C# Fundermentals", Age=25},
        new Student{Id=5, Name="Manu", Course="Software Engineering", Age=90}
    }; */

    /*
        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            return await _context.Students
                .Where(s => s.IsActive)
                .ToListAsync();
        }

    */
    public async Task<Student?> GetStudentWithCoursesAsync(int id)
    {
        return await _context.Students
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<StudentListDto>> GetStudentListAsync()
    {
        return await _context.Students.AsNoTracking()
        .Select(s => new StudentListDto
        {
            Id = s.Id,

            FullName = s.FirstName + " " + s.LastName,

            Email = s.Email,

            Department = s.Department.Name,
            Status = s.Status
        })
        .ToListAsync();
    }

    public async Task<Student?> GetByIdForUpdateAsync(int id)
    {
        return await _context.Students.Include(s => s.Address).FirstOrDefaultAsync(s => s.Id == id);
    }


    public async Task<StudentEditDto?> GetEditDtoAsync(int id)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentEditDto
            {
                Id = s.Id,

                FirstName = s.FirstName,

                LastName = s.LastName,

                Email = s.Email,

                DateOfBirth = s.DateOfBirth,

                DepartmentId = s.DepartmentId,

                ConcurrencyToken = s.ConcurrencyToken,

                Address = new AddressDto
                {
                    AddressLine1 = s.Address.AddressLine1,

                    AddressLine2 = s.Address.AddressLine2,

                    City = s.Address.City,

                    State = s.Address.State,

                    Country = s.Address.Country
                }

            })
            .FirstOrDefaultAsync();
    }
public Task SoftDeleteAsync(Student student)
    {
        student.IsDeleted = true;
        student.DeletedDateUtc = DateTime.UtcNow;
        student.DeletedBy = "system";
        student.Archive();

        return Task.CompletedTask;
    }

    public async Task<bool> HasActiveEnrollmentsAsync(int studentId)
    {
        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active);
    }

    public async Task<StudentEnrollmentHeaderDto?> GetEnrollmentHeaderAsync(int studentId)
    {
        return await _context.Students
        .AsNoTracking()
        .Where(s => s.Id == studentId)
        .Select(s => new StudentEnrollmentHeaderDto
        {
            Id = s.Id,

            FullName = s.FirstName + " " + s.LastName,

            Department = s.Department.Name,

            Status = s.Status
        })
        .FirstOrDefaultAsync();
    }

    public async Task<StudentTranscriptDto?> GetTranscriptAsync(int studentId)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == studentId)
            .Select(s => new StudentTranscriptDto
            {
                Header = new StudentTranscriptHeaderDto
                {
                    StudentName = s.FirstName + " " + s.LastName,

                    Department = s.Department.Name,

                    CompletedCourses = s.Enrollments.Count(e =>
                        e.Status == EnrollmentStatus.Completed),

                    ActiveCourses = s.Enrollments.Count(e =>
                        e.Status == EnrollmentStatus.Active),

                    DroppedCourses = s.Enrollments.Count(e =>
                        e.Status == EnrollmentStatus.Dropped),

                    GPA = 0
                },

                Courses = s.Enrollments
                    .OrderBy(e => e.EnrollmentDate)
                    .Select(e => new TranscriptCourseDto
                    {
                        CourseCode = e.Course.CourseCode,

                        CourseName = e.Course.Name,

                        Grade = e.Grade,

                        Status = e.Status,

                        EnrollmentDate = e.EnrollmentDate
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public Task<PagedResult<StudentListDto>> SearchAsync(StudentSearchCriteria criteria)
    {
        throw new NotImplementedException();
    }


    public async Task<PagedResult<StudentListDto>> GetPagedAsync(PagedRequest request)
    {
        var query = _context.Students.Where(s => !s.IsDeleted);

        //Including searching
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(s =>

                s.FirstName.ToLower().Contains(search)

                ||

                s.LastName.ToLower().Contains(search)

                ||

                s.Email.ToLower().Contains(search)

                ||

                s.Department.Name.ToLower().Contains(search));
        }

        var total = await query.CountAsync();

        //Sorting by column
        query = request.SortColumn switch
        {
            "firstname" => request.SortDescending
                ? query.OrderByDescending(x => x.FirstName)
                : query.OrderBy(x => x.FirstName),

            "lastname" => request.SortDescending
                ? query.OrderByDescending(x => x.LastName)
                : query.OrderBy(x => x.LastName),

            "email" => request.SortDescending
                ? query.OrderByDescending(x => x.Email)
                : query.OrderBy(x => x.Email),

            _ => query.OrderBy(x => x.LastName)
        };

        var students = await query

            .Skip((request.PageNumber - 1)  * request.PageSize)

            .Take(request.PageSize)

            .Select(s =>  new StudentListDto
                {
                    Id = s.Id,

                    FullName = s.FirstName + " " + s.LastName,

                    Email = s.Email,

                    Department = s.Department.Name,

                    Status = s.Status
                })

            .ToListAsync();

        return new PagedResult<StudentListDto>
        {
            Items = students,

            TotalCount = total,

            PageNumber = request.PageNumber,

            PageSize = request.PageSize
        };
    }


}