
public static class StudentMapper
{
    public static StudentEditDto ToEditDto(Student student)
    {
        return new StudentEditDto
        {
            Id = student.Id,

            FirstName = student.FirstName,

            LastName = student.LastName,

            Email = student.Email,

            DateOfBirth = student.DateOfBirth,

            DepartmentId = student.DepartmentId,

            ConcurrencyToken = student.ConcurrencyToken,

            Address = student.Address == null ? new AddressDto() : new AddressDto
            {
                AddressLine1 = student.Address.AddressLine1,
                AddressLine2 = student.Address.AddressLine2,
                City = student.Address.City,
                State = student.Address.State,
                Country = student.Address.Country
            }
        };
    }

    public static void UpdateEntity(StudentEditDto dto, Student student)
    {
        student.FirstName = dto.FirstName;

        student.LastName = dto.LastName;

        student.Email = dto.Email;

        student.DateOfBirth = dto.DateOfBirth;

        student.DepartmentId = dto.DepartmentId;

        if (student.Address == null)
            {
                student.Address = new Address();
            }

        student.Address.AddressLine1 = dto.Address.AddressLine1;
        student.Address.AddressLine2 = dto.Address.AddressLine2;
        student.Address.City = dto.Address.City;
        student.Address.State = dto.Address.State;
        student.Address.Country = dto.Address.Country;
    }

    public static StudentListDto ToListDto(Student student)
    {
        return new StudentListDto
        {
            Id = student.Id,

            FullName =  $"{student.FirstName} {student.LastName}",

            Email = student.Email,

            Department =  student.Department.Name
        };
    }


    public static StudentDetailsDto ToDetailsDto(Student student)
    {
        return new StudentDetailsDto
        {
            Id = student.Id,

            FullName = $"{student.FirstName} {student.LastName}",
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            Department = student.Department.Name,
            Address = student.Address == null ? "No Address" : $"{student.Address.AddressLine1}, {student.Address.City}",

            Courses =  student.Enrollments
                        .Select(e => e.Course.Name)
                        .ToList()
        };
    }


    public static Student ToEntity(StudentCreateDto dto)
    {
        return new Student
        {
            FirstName = dto.FirstName,

            LastName = dto.LastName,

            Email = dto.Email,

            DateOfBirth = dto.DateOfBirth,

            DepartmentId = dto.DepartmentId,

            Address = new Address
            {
                AddressLine1 = dto.Address.AddressLine1,

                AddressLine2 = dto.Address.AddressLine2,

                City = dto.Address.City,

                State = dto.Address.State,

                Country = dto.Address.Country
            }
        };
    }


}