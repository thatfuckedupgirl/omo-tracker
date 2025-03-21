using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace omo_tracker;

public partial class App : Application {
    public override void Initialize() {AvaloniaXamlLoader.Load(this);}

    public override void OnFrameworkInitializationCompleted() {
        if (File.Exists("debug")) { IsDebug = true; }
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}