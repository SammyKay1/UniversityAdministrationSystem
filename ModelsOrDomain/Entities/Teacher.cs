
using System.Collections;
using System.Globalization;
using UniversityAdministrationSystem.Components.Pages.Courses;

public class Teacher : Person
{
    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    
        
    
        
    
        
    

    
}