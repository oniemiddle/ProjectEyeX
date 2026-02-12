using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectEye.App.ViewModels;
using ProjectEye.App.Views;
using ProjectEye.Platform.Abstractions;
using ProjectEye.Platform.Windows;

namespace ProjectEye.App;

public partial class App : Application
{
    public ServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = new ServiceCollection()
            .AddSingleton<ITrayPort, WindowsTrayPort>()
            .AddSingleton<INotificationPort, WindowsNotificationPort>()
            .AddSingleton<MainWindowViewModel>()
            .BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
