namespace ProjectEye.Platform.Abstractions;

public interface INotificationPort
{
    void Notify(string message);
}
