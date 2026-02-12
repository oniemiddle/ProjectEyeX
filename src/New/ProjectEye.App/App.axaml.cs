using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ProjectEye.App.ViewModels;
using ProjectEye.App.Views;
using ProjectEye.Core;
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

    public override async void OnFrameworkInitializationCompleted()
    {
        var dataRoot = Path.Combine(AppContext.BaseDirectory, "Data");

        Services = new ServiceCollection()
            .AddSingleton<ITrayPort, WindowsTrayPort>()
            .AddSingleton<INotificationPort, WindowsNotificationPort>()
            .AddSingleton<IAppConfigStore>(_ => new JsonAppConfigStore(Path.Combine(dataRoot, "config.json")))
            .AddSingleton<IStatisticsStore>(_ => new JsonStatisticsStore(Path.Combine(dataRoot, "statistics.json")))
            .AddSingleton<LegacyConfigXmlImporter>()
            .AddSingleton(provider => new FocusSessionEngine(new AppConfig(), provider.GetRequiredService<IStatisticsStore>()))
            .AddSingleton<MainWindowViewModel>()
            .BuildServiceProvider();

        var configStore = Services.GetRequiredService<IAppConfigStore>();
        var importer = Services.GetRequiredService<LegacyConfigXmlImporter>();
        var engine = Services.GetRequiredService<FocusSessionEngine>();

        try
        {
            var imported = await importer.TryImportFromFileAsync(Path.Combine(dataRoot, "config.xml"));
            if (imported)
            {
                Console.WriteLine("[LegacyConfigXmlImporter] legacy config.xml imported to config.json");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LegacyConfigXmlImporter] import failed: {ex.Message}");
        }

        var config = await configStore.LoadAsync();
        engine.UpdateConfig(config);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = Services.GetRequiredService<MainWindowViewModel>();
            await viewModel.InitializeAsync();

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
