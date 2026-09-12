using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Frosty.Core;

public class FrostyConfiguration
{
    public ImageSource Thumbnail { get; private set; }

    public string GamePath { get; }
    public string GameDir { get; }
    public string GameName { get; }
    public string ProfileName { get; }

    public FrostyConfiguration()
    {
        Thumbnail = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyCore;component/Images/Warning.png") as ImageSource;
    }

    public FrostyConfiguration(string profile) : this()
    {
        ProfileName = profile;
        GamePath = Config.Get<string>("GamePath", "", ConfigScope.Game, profile);
        GameDir = Path.GetDirectoryName(GamePath);

        FileVersionInfo vi = FileVersionInfo.GetVersionInfo(GamePath);
        GameName = vi.ProductName;

        // Try to extract the icon
        try
        {
            Icon sysicon = Icon.ExtractAssociatedIcon(GamePath);
            Thumbnail = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                sysicon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            sysicon.Dispose();
        }
        catch
        {

        }
    }
}