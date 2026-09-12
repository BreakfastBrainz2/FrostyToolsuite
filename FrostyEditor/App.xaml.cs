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
    public static string Version = "2.0.0";
    public static FrostyConfiguration SelectedProfile;

    public static Logger Logger = new();

    private void App_Startup(object sender, StartupEventArgs e)
    {
        ProfilesLibrary.Initialize();
        Config.Load(ConfigPath);
    }
}