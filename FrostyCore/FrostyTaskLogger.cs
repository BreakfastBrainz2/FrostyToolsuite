using Frosty.Core.Windows;
using Frosty.Sdk.Interfaces;

namespace Frosty.Core;

internal class FrostyTaskLogger : ILogger
{
    private FrostyTaskWindow task;

    public FrostyTaskLogger(FrostyTaskWindow inTask)
    {
        task = inTask;
    }

    public void LogInfo(string message)
    {
        task.Update(message);
    }

    public void LogWarning(string message)
    {
    }

    public void LogError(string message)
    {
    }

    public void LogProgress(double progress)
    {
        task.Update(null, progress);
    }
}