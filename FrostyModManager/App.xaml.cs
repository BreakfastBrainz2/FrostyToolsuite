using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Frosty.Core;
using Frosty.Core.Interfaces;
using Frosty.Core.Windows;
using Frosty.Sdk;
using FrostyModManager.Windows;

namespace FrostyModManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IFrostyApplication
{
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        Frosty.Core.App.Logger = new Logger();
        Frosty.Core.App.ConfigPath = Path.Combine(AppContext.BaseDirectory, "modmanager_config.json");
        Frosty.Core.App.PluginManager = new PluginManager(Frosty.Core.App.Logger, PluginManagerType.ModManager);

        ProfilesLibrary.Initialize();
        Config.Load(Frosty.Core.App.ConfigPath);

        PrelaunchWindow2 win = new(this);
        MainWindow = win;
        win.Show();
    }

    public void OnPrelaunchCompleted()
    {
        SplashWindow splash = new(this);
        MainWindow = splash;
        splash.Show();
    }

    public void OnSplashCompleted()
    {
        MainWindow win = new();
        MainWindow = win;
        win.Show();
    }
}