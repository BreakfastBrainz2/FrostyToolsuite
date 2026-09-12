using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using Frosty.Controls;
using Frosty.Core;
using Frosty.Sdk;
using FrostyEditor;

namespace FrostyEditor.Windows;

/// <summary>
/// Interaction logic for PrelaunchWindow2.xaml
/// </summary>
public partial class PrelaunchWindow2 : FrostyDockableWindow
{
    private List<FrostyConfiguration> configs = new();
    private FrostyConfiguration defaultConfig;

    public PrelaunchWindow2()
    {
        InitializeComponent();
    }

    private void LaunchConfig(string profile)
    {
        // load profiles
        if (!ProfilesLibrary.Initialize(profile))
        {
            FrostyMessageBox.Show("There was an error when trying to load game using specified profile.", "Frosty Editor");
            Close();
            return;
        }

        // launch splash
        //SplashWindow splash = new SplashWindow();
        //App.Current.MainWindow = splash;
        //splash.Show();
        //Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshConfigurationList();

        RemoveConfigButton.IsEnabled = false;
        LaunchConfigButton.IsEnabled = false;

        string defaultConfigName = Config.Get<string>("DefaultProfile", null);

        if (!string.IsNullOrEmpty(defaultConfigName))
        {
            defaultConfig = configs.Find(x => x.ProfileName == defaultConfigName);
        }

        ConfigList.SelectedItem = defaultConfig;
    }

    private async void LaunchConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (ConfigList.SelectedIndex == -1)
            return;

        if (ConfigList.SelectedItem is FrostyConfiguration config)
        {
            LaunchConfig(config.ProfileName);
            await Task.Delay(1);
            Close();
        }
        ConfigList.SelectedIndex = -1;
    }

    private void ConfigList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RemoveConfigButton.IsEnabled = true;
        LaunchConfigButton.IsEnabled = true;
    }

    private async void ConfigList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ConfigList.SelectedIndex == -1)
            return;

        if (ConfigList.SelectedItem is FrostyConfiguration config)
        {
            LaunchConfig(config.ProfileName);
            await Task.Delay(1);
            Close();
        }
        ConfigList.SelectedIndex = -1;
    }

    private void NewConfigButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog
        {
            Filter = "*.exe (Game Executable)|*.exe",
            Title = "Choose Game Executable"
        };

        if (ofd.ShowDialog() == false)
            return;

        FileInfo fi = new FileInfo(ofd.FileName);

        string key = Path.GetFileNameWithoutExtension(fi.Name);
        // try to load game profile
        if (!ProfilesLibrary.HasProfile(key))
        {
            FrostyMessageBox.Show("There was an error when trying to load game using specified profile.", "Frosty Editor");
            return;
        }

        // make sure config doesnt already exist
        foreach (FrostyConfiguration config in configs)
        {
            if (config.ProfileName == key)
            {
                FrostyMessageBox.Show("That game already has a configuration.");
                return;
            }
        }

        if (ProfilesLibrary.HasAntiCheat)
            FrostyMessageBox.Show("This game contains EasyAntiCheat and cannot automatically generate an sdk. We will not support nor assist anyone who attempts to bypass it.", "Warning");

        // create
        Config.AddGame(key, fi.FullName);
        configs.Add(new FrostyConfiguration(key));
        Config.Save(App.ConfigPath);

        ConfigList.Items.Refresh();
    }

    private void ScanForGamesButton_Click(object sender, RoutedEventArgs e)
    {
        using (RegistryKey lmKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node"))
        {
            int totalCount = 0;

            IterateSubKeys(lmKey, ref totalCount);
        }

        ConfigList.Items.Refresh();
    }

    private void IterateSubKeys(RegistryKey subKey, ref int totalCount)
    {
        foreach (string subKeyName in subKey.GetSubKeyNames())
        {
            try
            {
                IterateSubKeys(subKey.OpenSubKey(subKeyName), ref totalCount);
            }
            catch (Exception)
            {
                continue;
            }
        }

        foreach (string subKeyValue in subKey.GetValueNames())
        {
            if (subKeyValue.IndexOf("Install Dir", StringComparison.OrdinalIgnoreCase) != -1)
            {
                string installDir = subKey.GetValue("Install Dir") as string;
                if (string.IsNullOrEmpty(installDir))
                    continue;
                if (!Directory.Exists(installDir))
                    continue;

                foreach (string filename in Directory.EnumerateFiles(installDir, "*.exe"))
                {
                    FileInfo fi = new FileInfo(filename);
                    string nameWithoutExt = fi.Name.Replace(fi.Extension, "");

                    if (ProfilesLibrary.HasProfile(nameWithoutExt))
                    {
                        foreach (FrostyConfiguration config in configs)
                        {
                            if (config.ProfileName == fi.Name.Remove(fi.Name.Length - 4))
                                return;
                        }

                        Config.AddGame(fi.Name.Remove(fi.Name.Length - 4), fi.DirectoryName);
                        configs.Add(new FrostyConfiguration(fi.Name.Remove(fi.Name.Length - 4)));

                        totalCount++;
                    }
                }
            }
        }
    }

    private void RemoveConfigButton_Click(object sender, RoutedEventArgs e)
    {
        if (FrostyMessageBox.Show("Are you sure you want to delete this configuration?", "Frosty Editor", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            FrostyConfiguration selectedItem = ConfigList.SelectedItem as FrostyConfiguration;

            Config.RemoveGame(selectedItem.ProfileName);

            configs.Remove(selectedItem);
            ConfigList.Items.Refresh();

            ConfigList.SelectedIndex = 0;
            Config.Save(App.ConfigPath);
        }
    }

    private void RefreshConfigurationList()
    {
        configs.Clear();

        foreach (string profile in Config.GameProfiles)
        {
            FrostyConfiguration config = new(profile);
            if (File.Exists(config.GamePath))
            {
                configs.Add(config);
            }
            else
            {
                Config.RemoveGame(profile);
            }
        }
        Config.Save(App.ConfigPath);

        ConfigList.ItemsSource = configs;
    }
}