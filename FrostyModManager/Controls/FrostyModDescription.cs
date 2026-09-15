using Frosty.Core.Mod;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frosty.ModSupport;
using Frosty.ModSupport.Mod;

namespace FrostyModManager.Controls;

public class ScreenshotButtonEventArgs : RoutedEventArgs
{
    public ImageSource Screenshot { get; }

    public ScreenshotButtonEventArgs(ImageSource inSource)
    {
        Screenshot = inSource;
    }
}

public class AffectedFileInfo
{
    public string Name { get; }
    public string Type { get; }
    public bool IsAdded { get; }
    public bool IsModified { get; }

    public AffectedFileInfo(string n, string t, bool a, bool m)
    {
        Name = n;
        Type = t;
        IsAdded = a;
        IsModified = m;
    }
}

[TemplatePart(Name = PART_ScreenshotPanel, Type = typeof(StackPanel))]
[TemplatePart(Name = PART_ModIcon, Type = typeof(Image))]
[TemplatePart(Name = PART_ModFilesListBox, Type = typeof(ListBox))]
[TemplatePart(Name = PART_LoadingText, Type = typeof(TextBlock))]
public class FrostyModDescription : Control
{
    private const string PART_ScreenshotPanel = "PART_ScreenshotPanel";
    private const string PART_ModIcon = "PART_ModIcon";
    private const string PART_ModFilesListBox = "PART_ModFilesListBox";
    private const string PART_LoadingText = "PART_LoadingText";

    private StackPanel screenshotPanel;
    private Image modIcon;
    private ListBox modFilesListBox;
    private TextBlock loadingText;

    #region -- Properties --

    #region -- Mod --
    public static readonly DependencyProperty ModProperty = DependencyProperty.Register("Mod", typeof(FrostyModDetails), typeof(FrostyModDescription), new FrameworkPropertyMetadata(null));
    public FrostyModDetails Mod
    {
        get => (FrostyModDetails)GetValue(ModProperty);
        set => SetValue(ModProperty, value);
    }
    #endregion

    #endregion

    public delegate void ScreenshotButtonEventHandler(object sender, ScreenshotButtonEventArgs e);

    private event ScreenshotButtonEventHandler screenshotClicked;
    public event ScreenshotButtonEventHandler ScreenshotClicked
    {
        add => screenshotClicked += value;
        remove => screenshotClicked -= value;
    }

    static FrostyModDescription()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostyModDescription), new FrameworkPropertyMetadata(typeof(FrostyModDescription)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        screenshotPanel = GetTemplateChild(PART_ScreenshotPanel) as StackPanel;
        modIcon = GetTemplateChild(PART_ModIcon) as Image;
        modFilesListBox = GetTemplateChild(PART_ModFilesListBox) as ListBox;
        loadingText = GetTemplateChild(PART_LoadingText) as TextBlock;

        Loaded += FrostyModDescription_Loaded;
    }

    private async void FrostyModDescription_Loaded(object sender, RoutedEventArgs e)
    {
        if (Mod != null)
        {
            // icon
            //if (Mod.ModDetails.Icon != null)
            //    modIcon.Visibility = Visibility.Visible;

            // screenshots
            /*int idx = 0;
            foreach (ImageSource screenshot in Mod.ModDetails.Screenshots)
            {
                Image screenshotImage = screenshotPanel.Children[idx++] as Image;

                screenshotImage.Source = screenshot;
                screenshotImage.Visibility = Visibility.Visible;
                screenshotImage.MouseDown += (o, e2) =>
                {
                    if (e2.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                        return;

                    Image img = o as Image;
                    screenshotClicked?.Invoke(this, new ScreenshotButtonEventArgs(img.Source));
                };
            }*/

            List<AffectedFileInfo> affectedFiles = new List<AffectedFileInfo>();

            loadingText.Visibility = Visibility.Collapsed;
            modFilesListBox.ItemsSource = affectedFiles;
        }
    }
}