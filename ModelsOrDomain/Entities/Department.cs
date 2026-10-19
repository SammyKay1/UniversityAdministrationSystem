using System.ComponentModel.DataAnnotations;

public class Department : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(10)]
    public string Code { get; set; } = "";

    [StringLength(100)]
    public string Email { get; set; } = "";

    [StringLength(500)]
    public string Description { get; set; } = "";

    public ICollection<Student> Students { get; set; }
        = new List<Student>();

    public ICollection<Teacher> Teachers { get; set; }
        = new List<Teacher>();
}