using Frosty.Sdk.Interfaces;

namespace FrostyEditor;

public class Logger : ILogger
{
    public Logger()
    {
    }

    public void LogInfo(string message)
    {
    }

    public void LogWarning(string message)
    {
    }

    public void LogError(string message)
    {
    }

    public void LogProgress(double progress)
    {
    }

    public void Log(string message) => LogInfo(message);

    public void Log(string text, params object[] vars) => LogInfo(String.Format(text, vars));
}