namespace Frosty.Core.Exceptions;

public sealed class FrostyModLoadException : Exception
{
    public FrostyModLoadException(string message)
        : base(message)
    {
    }
}