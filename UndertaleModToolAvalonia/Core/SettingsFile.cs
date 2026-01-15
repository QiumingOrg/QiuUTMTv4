using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged.SourceGenerator;
using Semi.Avalonia;

namespace UndertaleModToolAvalonia;

public partial class SettingsFile
{
    public MainViewModel MainVM = null!;

    public SettingsFile()
    {
    }

    public SettingsFile(IServiceProvider serviceProvider)
    {
        MainVM = serviceProvider.GetRequiredService<MainViewModel>();
    }

    public static SettingsFile LoadWithoutMainVM()
    {

        SettingsFile? settings = null;

        string roamingAppData =
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QiuUTMTv4");

        // Load Settings.json
        string settingsPath = Path.Join(roamingAppData, "Settings.json");

        if (File.Exists(settingsPath))
        {
            try
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonSerializer.Deserialize<SettingsFile>(json, new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                });

                if (settings is not null)
                {
                    settings.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?.?";
                }
            }
            catch (Exception e)
            {}
        }

        return settings;
    }

    public static SettingsFile Load(IServiceProvider serviceProvider)
    {
        MainViewModel mainVM = serviceProvider.GetRequiredService<MainViewModel>();

        SettingsFile? settings = null;

        string roamingAppData =
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QiuUTMTv4");

        // Load Settings.json
        string settingsPath = Path.Join(roamingAppData, "Settings.json");

        if (File.Exists(settingsPath))
        {
            try
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonSerializer.Deserialize<SettingsFile>(json, new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                });

                if (settings is not null)
                {
                    // Check for upgrades here.
                    settings.MainVM = mainVM;
                    settings.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?.?";
                }
            }
            catch (Exception e)
            {
                mainVM.LazyErrorMessages.Add(
                    $"Error when loading settings file:\n{e.Message}\nDefault settings loaded.");
            }
        }

        settings ??= new SettingsFile(serviceProvider);

        // Load Styles.xaml
        string stylesPath = Path.Join(roamingAppData, "Styles.xaml");

        if (File.Exists(stylesPath))
        {
            try
            {
                string xaml = File.ReadAllText(stylesPath);
                Styles styles = AvaloniaRuntimeXamlLoader.Parse<Styles>(xaml);

                if (App.CurrentCustomStyles is not null)
                    App.Current!.Styles.Remove(App.CurrentCustomStyles);

                App.CurrentCustomStyles = styles;
                App.Current!.Styles.Add(styles);
            }
            catch (Exception e)
            {
                mainVM.LazyErrorMessages.Add($"Error when loading styles file:\n{e.Message}");
            }
        }

        return settings;
    }

    public async void Save()
    {
        string roamingAppData =
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QiuUTMTv4");
        Directory.CreateDirectory(roamingAppData);

        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
        {
            WriteIndented = true,
        });

        try
        {
            File.WriteAllText(Path.Join(roamingAppData, "Settings.json"), json);
        }
        catch (Exception e)
        {
            await MainVM.ShowMessageDialog($"Error when saving settings file: {e.Message}");
        }
    }

    public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?.?";

    public enum ThemeValue
    {
        SystemDefault = 0,
        Light = 1,
        Dark = 2,
    }

    public enum LanguageValue
    {
        AutoDetect = 0,
        Zh = 1,
        En = 2
    }

    [Notify] private ThemeValue _Theme;

    [Notify] private LanguageValue _Language;

    void OnThemeChanged()
    {
        if (App.Current is not null)
        {
            App.Current.RequestedThemeVariant = Theme switch
            {
                ThemeValue.SystemDefault => ThemeVariant.Default,
                ThemeValue.Light => ThemeVariant.Light,
                ThemeValue.Dark => ThemeVariant.Dark,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public void OnLanguageChanged()
    {
        if (App.Current is not null)
        {
            CultureInfo culture = getCultureInfoFromSetting();
            Thread.CurrentThread.CurrentUICulture = Assets.Strings.Culture = culture;
            SemiTheme.OverrideLocaleResources(Application.Current, culture);
            //Console.WriteLine(culture);
            Save();
        }
    }

    public CultureInfo getCultureInfoFromSetting()
    {
        return Language switch
        {
            LanguageValue.AutoDetect => Thread.CurrentThread.CurrentCulture,
            LanguageValue.Zh => new CultureInfo("zh-CN", false),
            LanguageValue.En => new CultureInfo("en", false),
            _ => Thread.CurrentThread.CurrentCulture,
        };
    }

    public bool OpenNewResourceAfterCreatingIt { get; set; } = false;
    public bool EnableSyntaxHighlighting { get; set; } = true;
    public bool AutomaticallyCompileAndDecompileCodeOnLostFocus { get; set; } = true;

    public bool EnableRoomGridByDefault { get; set; } = false;
    public uint DefaultRoomGridWidth { get; set; } = 20;
    public uint DefaultRoomGridHeight { get; set; } = 20;

    public string InstanceIdPrefix { get; set; } = "inst_";

    public bool EnableQiuUtmtV3ScriptEngine { get; set; } = true;

    public Underanalyzer.Decompiler.DecompileSettings DecompileSettings { get; set; } = new();
}