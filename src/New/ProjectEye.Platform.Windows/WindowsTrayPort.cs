using ProjectEye.Platform.Abstractions;

namespace ProjectEye.Platform.Windows;

public sealed class WindowsTrayPort : ITrayPort
{
    public void Initialize()
    {
        // Task A skeleton: real tray implementation comes in Sprint-1.
    }

    public void ShowMessage(string title, string message)
    {
        // Task A skeleton: no-op placeholder.
    }
}
