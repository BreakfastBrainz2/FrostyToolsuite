using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Frosty.Core;
using Frosty.Sdk;

namespace FrostyEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string ConfigPath = Path.Combine(AppContext.BaseDirectory, "editor_config.json");

    private void App_Startup(object sender, StartupEventArgs e)
    {
        ProfilesLibrary.Initialize();
        Config.Load(ConfigPath);
    }
}