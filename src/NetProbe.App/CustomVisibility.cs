using System.Windows;

namespace NetProbe.App;

public static class CustomVisibility
{
    public static Visibility DebugOnly
    {
#if DEBUG
        get { return Visibility.Visible; }
#else
        get { return Visibility.Collapsed; }
#endif
    }
}
