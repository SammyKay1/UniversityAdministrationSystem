
public class AdminLayoutState
{
    public bool IsSidebarCollapsed { get; private set; }

    public bool IsMobileDrawerOpen { get; private set; }

    public event Action? OnChange;

    public void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;

        OnChange?.Invoke();
    }

    public void SetSidebarCollapsed(bool collapsed)
    {
        IsSidebarCollapsed = collapsed;

        OnChange?.Invoke();
    }

    public void ToggleMobileDrawer()
    {
        IsMobileDrawerOpen = !IsMobileDrawerOpen;

        OnChange?.Invoke();
    }

    public void SetMobileDrawerOpen(bool open)
    {
        IsMobileDrawerOpen = open;

        OnChange?.Invoke();
    }

    public void CloseMobileDrawer()
    {
        if (IsMobileDrawerOpen)
        {
            IsMobileDrawerOpen = false;

            OnChange?.Invoke();
        }
    }
}