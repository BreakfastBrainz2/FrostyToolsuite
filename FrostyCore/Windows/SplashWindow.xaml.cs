using System.ComponentModel;
using Frosty.Controls;
using Frosty.Core;
using Frosty.Sdk;
using Frosty.Sdk.IO;
using Frosty.Sdk.Managers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Shell;
using Frosty.Core.Interfaces;
using Frosty.Core.Windows;
using Frosty.Sdk.Interfaces;
using Frosty.Sdk.Utils;

namespace Frosty.Core.Windows;

internal class SplashWindowLogger : ILogger, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public double Progress
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                NotifyPropertyChanged();
            }
        }
    }

    public string Status
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                NotifyPropertyChanged();
            }
        }
    }

    private SplashWindow parent;

    public SplashWindowLogger(SplashWindow inParent)
    {
        parent = inParent;

        // Utilize DataBindings to eliminate need for Dispatcher
        BindingOperations.SetBinding(parent.logTextBox, TextBlock.TextProperty, new Binding("Status") { Source = this });
        BindingOperations.SetBinding(parent.progressBar, ProgressBar.ValueProperty, new Binding("Progress") { Source = this });

        parent.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

        /*BindingOperations.SetBinding(parent.TaskbarItemInfo, TaskbarItemInfo.ProgressValueProperty,
            new Binding("Progress")
            {
                Converter = new FunctionBasedValueConverter(),
                ConverterParameter = new Func<object, object>(delegate(object value) { return (double)value / 100.0; }),
                Source = this,
            });*/
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void LogInfo(string message)
    {
        Status = message;
    }

    public void LogWarning(string message)
    {
        Status = message;
    }

    public void LogError(string message)
    {
        Status = message;
    }

    public void LogProgress(double progress)
    {
        Progress = progress;
    }
}

/// <summary>
/// Interaction logic for SplashWindow.xaml
/// </summary>
public partial class SplashWindow : Window
{
    private IFrostyApplication m_frostyApp;

    public SplashWindow(IFrostyApplication frostyApp)
    {
        m_frostyApp = frostyApp;

        InitializeComponent();
        versionTextBlock.Text = App.Version;
        TaskbarItemInfo = new TaskbarItemInfo();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Config.Save(App.ConfigPath);
        FrostyLogger.Logger = new SplashWindowLogger(this);

        // set base directory to the directory containing the executable
        Utils.BaseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory) ?? string.Empty;

        App.Logger!.LogInfo("Loading Profile For " + ProfilesLibrary.DisplayName);

        profileTextBlock.Text = ProfilesLibrary.DisplayName;
        //bannerImage.Source = LoadBanner(ProfilesLibrary.Banner);

        // init profile
        if (!ProfilesLibrary.Initialize(App.SelectedProfile.ProfileKey))
        {
            FrostyMessageBox.Show("There was an error when trying to load game using specified profile.", "Frosty Editor");
            Close();
            return;
        }

        // TODO: key loading here

        // generate sdk if needed
        string sdkPath = ProfilesLibrary.SdkPath;
        if (!File.Exists(sdkPath))
        {
            SdkUpdateWindow sdkWin = new(this);
            sdkWin.ShowDialog();
        }

        bool initialized = await InitGameData();
        if (!initialized)
        {
            goto failed;
        }

        m_frostyApp.OnSplashCompleted();
        //MainWindow win = new();
        //App.Current.MainWindow = win;
        //win.Show();

        App.Logger.LogInfo("Initialization complete");

        FrostyLogger.Logger = App.Logger;

        Close();
        return;

        failed:
        FrostyMessageBox.Show("Failed to initialize Frosty", "Frosty Editor");
        Close();
    }

    private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private async Task<bool> InitGameData()
    {
        await Task.Run(() =>
        {
            FrostyLogger.Logger.LogInfo("Initializing FileSystemManager");
            // init filesystem manager, this parses the layout.toc file
            if (!FileSystemManager.Initialize(App.SelectedProfile.GameDir))
            {
                return false;
            }

            FrostyLogger.Logger.LogInfo("Initializing TypeLibrary");
            // init type library, this loads the EbxTypeSdk used to properly parse ebx assets
            if (!TypeLibrary.Initialize())
            {
                return false;
            }

            FrostyLogger.Logger.LogInfo("Initializing ResourceManager");
            // init resource manager, this parses the cas.cat files if they exist for easy asset lookup
            if (!ResourceManager.Initialize())
            {
                return false;
            }

            FrostyLogger.Logger.LogInfo("Initializing AssetManager");
            // init asset manager, this parses the SuperBundles and loads all the assets
            if (!AssetManager.Initialize())
            {
                return false;
            }

            return true;
        });

        return true;
    }
}