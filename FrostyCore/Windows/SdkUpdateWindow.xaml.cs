using Frosty.Controls;
using Frosty.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Frosty.Sdk.IO;
using Frosty.Sdk.Sdk;

namespace Frosty.Core.Windows;

public enum SdkUpdateTaskState
{
    Inactive,
    Active,
    CompletedSuccessful,
    CompletedFail
}

public class SdkUpdateTask : INotifyPropertyChanged
{
    public delegate bool TaskDelegate(SdkUpdateTask task, object state);

    public string DisplayName { get => displayName; set { displayName = value; NotifyPropertyChanged(); } }
    public SdkUpdateTaskState State { get => state; set { state = value; NotifyPropertyChanged(); } }
    public string StatusMessage { get => statusMessage; set { statusMessage = value; NotifyPropertyChanged(); } }
    public string FailMessage { get => failMessage; set { failMessage = value; NotifyPropertyChanged(); } }
    public TaskDelegate Task { get; set; }

    private string displayName;
    private SdkUpdateTaskState state;
    private string statusMessage;
    private string failMessage;

    public SdkUpdateTask()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SdkUpdateState
{
    public Process Process;
}

/// <summary>
/// Interaction logic for SdkUpdateWindow.xaml
/// </summary>
public partial class SdkUpdateWindow : FrostyDockableWindow
{
    public string ProfileName => ProfilesLibrary.DisplayName;
    private SdkUpdateTask failedTask = null;

    public SdkUpdateWindow(Window owner)
    {
        InitializeComponent();
        Owner = owner;
    }

    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        pageOne.Visibility = Visibility.Collapsed;
        pageTwo.Visibility = Visibility.Visible;

        // tasks to execute async
        List<SdkUpdateTask> tasks = new()
        {
            new SdkUpdateTask() { DisplayName = "Waiting for process to become active", Task = OnDetectRunningProcess },
            new SdkUpdateTask() { DisplayName = "Scanning for type info offset", Task = OnFindTypeInfoOffset }
        };
        tasksListBox.ItemsSource = tasks;

        SdkUpdateState state = new();

        foreach (var task in tasks)
        {
            // set current task to active and execute
            task.State = SdkUpdateTaskState.Active;
            await Task.Run(() => { task.Task(task, state); });

            if (task.State == SdkUpdateTaskState.CompletedFail)
            {
                failedTask = task;
                break;
            }
        }

        successMessage.Visibility = (failedTask == null) ? Visibility.Visible : Visibility.Collapsed;
        failMessage.Text = (failedTask != null) ? failedTask.FailMessage : "";
        finishButton.IsEnabled = true;
    }

    private void FinishButton_Click(object sender, RoutedEventArgs e)
    {
        //Application.Current.Shutdown();
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// Waits for the game process to become active before completing
    /// </summary>
    private bool OnDetectRunningProcess(SdkUpdateTask task, object state)
    {
        Process foundProcess = null;
        SdkUpdateState updateState = state as SdkUpdateState;

        while (true)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string processFilename = process.MainModule?.ModuleName;
                    if (string.IsNullOrEmpty(processFilename))
                        continue;

                    FileInfo fi = new(processFilename);
                    if (fi.Name.IndexOf(ProfilesLibrary.ProfileName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundProcess = process;
                        break;
                    }
                }
                catch (Exception)
                {
                    if (process.ProcessName.IndexOf(ProfilesLibrary.ProfileName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        task.FailMessage = string.Format("Unable to access the specified process");
                        task.StatusMessage = process.ProcessName;
                        task.State = SdkUpdateTaskState.CompletedFail;
                        return false;
                    }
                }
            }

            if (foundProcess != null)
            {
                while (foundProcess.MainWindowHandle == IntPtr.Zero)
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                updateState.Process = foundProcess;
                task.StatusMessage = foundProcess.ProcessName;
                break;
            }
        }

        task.State = SdkUpdateTaskState.CompletedSuccessful;
        return true;
    }

    /// <summary>
    /// Locates the offset to the first type info object in the active games memory
    /// </summary>
    private bool OnFindTypeInfoOffset(SdkUpdateTask task, object state)
    {
        SdkUpdateState updateState = state as SdkUpdateState;
        TypeSdkGenerator typeSdkGenerator = new();

        task.StatusMessage = "Dumping types from process";
        if (!typeSdkGenerator.DumpTypes(updateState.Process))
        {
            task.State = SdkUpdateTaskState.CompletedFail;
            task.FailMessage = "Failed to dump types";
            return false;
        }

        task.StatusMessage = "Creating SDK";
        if (!typeSdkGenerator.CreateSdk(ProfilesLibrary.SdkPath))
        {
            task.State = SdkUpdateTaskState.CompletedFail;
            task.FailMessage = "Failed to create SDK";
            return false;
        }

        task.State = SdkUpdateTaskState.CompletedSuccessful;
        task.StatusMessage = "Successfully created SDK";
        return true;
    }
}