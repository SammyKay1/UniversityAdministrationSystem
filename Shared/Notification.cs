
public class Notification
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Message { get; set; } = "";

    public NotificationType Type { get; set; }

    public bool AutoClose { get; set; }

    public TimeSpan Duration { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}