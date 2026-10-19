
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

public abstract class Person : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    [Required]
    public string Email { get; set; } = "";

    public DateTime DateOfBirth { get; set; }

    public Address? Address { get; set; }

    //For implementing concurrency
    public Guid ConcurrencyToken { get; set; } = Guid.NewGuid(); 




    

}