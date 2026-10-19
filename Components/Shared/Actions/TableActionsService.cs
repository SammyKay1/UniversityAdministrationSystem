
public class TableActionsService
{
    public event Action? CloseRequested;


    public void CloseAll()
    {
        CloseRequested?.Invoke();
    }
}