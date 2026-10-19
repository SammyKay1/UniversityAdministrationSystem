
public class NotificationService
{
    private readonly List<Notification> _notifications = [];

    public IReadOnlyList<Notification>  Notifications => _notifications;

    public event Action? OnChange;

    public void Success(string message) => Add(message, NotificationType.Success);

    public void Error(string message) => Add(message, NotificationType.Error);

    public void Warning(string message)
        => Add(message, NotificationType.Warning);

    public void Info(string message)
        => Add(message, NotificationType.Info);

    private void Add(string message, NotificationType type)
    {
        var notification = new Notification
    {
        Message = message,
        Type = type,
        AutoClose = type != NotificationType.Error,
        Duration = type switch
        {
            NotificationType.Success => TimeSpan.FromSeconds(3),

            NotificationType.Info => TimeSpan.FromSeconds(4),

            NotificationType.Warning => TimeSpan.FromSeconds(6),

            _ => TimeSpan.Zero
        }
    };

    _notifications.Add(notification);

        OnChange?.Invoke();

        if (notification.AutoClose)
        {
            _ = RemoveLater(notification);
        }
    }

    private async Task RemoveLater(Notification notification)
    {
        await Task.Delay(notification.Duration);

        Remove(notification.Id);
    }
    public void Remove(Guid id)
    {
        var item = _notifications.FirstOrDefault(x => x.Id == id);

        if (item is null)
            return;

        _notifications.Remove(item);

        OnChange?.Invoke();
    }
}