namespace ProjectEye.Platform.Abstractions;

public interface ITrayPort
{
    void Initialize();
    void ShowMessage(string title, string message);
}
