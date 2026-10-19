using System.ComponentModel.DataAnnotations;

public class Student : Person
{
    
    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

   public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

   public StudentStatus Status { get; private set; } = StudentStatus.Active;

   public void Archive()
   {
       Status = StudentStatus.Archived;
   }

   
   public bool CanEnroll()
    {
        return Status == StudentStatus.Active;
    }

    



}