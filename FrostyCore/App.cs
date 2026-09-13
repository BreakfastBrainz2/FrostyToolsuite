using System.Windows;
using Frosty.Core.Interfaces;
using Frosty.Sdk.Interfaces;

namespace Frosty.Core;

public sealed class App
{
    public static string ConfigPath;
    public static string Version = "2.0.0";
    public static FrostyConfiguration SelectedProfile;
    public static PluginManager PluginManager;

    public static ILogger? Logger;
    public static IEditorWindow? EditorWindow => Application.Current.MainWindow as IEditorWindow;
}