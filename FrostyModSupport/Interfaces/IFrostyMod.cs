using System.Collections.Generic;
using Frosty.ModSupport.Mod;

namespace Frosty.ModSupport.Interfaces;

public interface IFrostyMod
{
    FrostyModDetails ModDetails { get; }
    IEnumerable<string> Warnings { get; }
    bool HasWarnings { get; }
    string Filename { get; }
    string Path { get; }
}