using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using UTMTdrid;

namespace UndertaleModToolAvalonia;

public partial class App : Application
{
    public static IServiceProvider Services = null!;
    public static IStyle? CurrentCustomStyles = null;

    public override void Initialize()
    {
        SettingsFile settings = SettingsFile.LoadWithoutMainVM();
        if (settings != null)
        {
            Thread.CurrentThread.CurrentUICulture = Assets.Strings.Culture = settings.getCultureInfoFromSetting();
        }
        if (OperatingSystem.IsAndroid())
        {
            // Maybe a workaround for Magick.NET not running in .csx scripts for Android
            try
            {
                MagickNETWorkaround.Apply();
            }
            catch (Exception exception)
            {
            }

            var t = AssetLoader.Open(new Uri($"avares://UndertaleModToolAvalonia/Assets/sth.zip"));
            ZipExtractor.ExtractZipStream(t, AppContext.BaseDirectory);
        }

        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        
        // Dependency injection.
        ServiceCollection collection = new();
        collection.AddSingleton<MainViewModel>();

        Services = collection.BuildServiceProvider();
        MainViewModel vm = Services.GetRequiredService<MainViewModel>();
        vm.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
                WindowState = WindowState.Maximized,
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            single.MainView = new MainView
            {
                DataContext = vm
            };
        }
    }
}