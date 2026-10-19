public class Permission : AuditableEntity
{
    public int Id { get; set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    private Permission() { }

    public Permission(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}