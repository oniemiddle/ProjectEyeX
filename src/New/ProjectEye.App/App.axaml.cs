using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectEye.App.ViewModels;
using ProjectEye.App.Views;
using ProjectEye.Core.Abstractions;
using ProjectEye.Core.Services;
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
        var dataRoot = Path.Combine(AppContext.BaseDirectory, "Data");

        Services = new ServiceCollection()
            .AddSingleton<ITrayPort, WindowsTrayPort>()
            .AddSingleton<INotificationPort, WindowsNotificationPort>()
            .AddSingleton<IAppConfigStore>(_ => new JsonAppConfigStore(Path.Combine(dataRoot, "config.json")))
            .AddSingleton<IStatisticsStore>(_ => new JsonStatisticsStore(Path.Combine(dataRoot, "statistics.json")))
            .AddSingleton<LegacyConfigXmlImporter>()
            .AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IAppConfigStore>().LoadAsync().GetAwaiter().GetResult();
                return new FocusSessionEngine(config, provider.GetRequiredService<IStatisticsStore>());
            })
            .AddSingleton<MainWindowViewModel>()
            .BuildServiceProvider();

        var importer = Services.GetRequiredService<LegacyConfigXmlImporter>();
        var imported = false;

        try
        {
            imported = importer.TryImportFromFileAsync(Path.Combine(dataRoot, "config.xml")).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LegacyConfigXmlImporter] import failed: {ex.Message}");
        }

        if (imported)
        {
            Console.WriteLine("[LegacyConfigXmlImporter] legacy config.xml imported to config.json");
        }

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
