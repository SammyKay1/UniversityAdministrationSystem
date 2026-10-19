
public class DialogService : IDialogService
{
    private TaskCompletionSource<bool>? _taskSource;


    public event Action? OnShow;


    public string Title { get; private set; } = "";

    public string Message { get; private set; } = "";

    public Task<bool> ConfirmAsync(string title, string message)
    {
        Title = title;
        Message = message;

        _taskSource = new TaskCompletionSource<bool>();

        OnShow?.Invoke();

        return _taskSource.Task;
    }


    public void Confirm()
    {
        _taskSource?.TrySetResult(true);
    }


    public void Cancel()
    {
        _taskSource?.TrySetResult(false);
    }
}