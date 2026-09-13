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
    private void App_Startup(object sender, StartupEventArgs e)
    {
        Frosty.Core.App.Logger = new Logger();
        Frosty.Core.App.ConfigPath = Path.Combine(AppContext.BaseDirectory, "editor_config.json");
        Frosty.Core.App.PluginManager = new PluginManager(Frosty.Core.App.Logger, PluginManagerType.Editor);

        ProfilesLibrary.Initialize();
        Config.Load(Frosty.Core.App.ConfigPath);
    }
}