using System;

namespace Frosty.Sdk.Attributes;

/// <summary>
/// Specifies that this property should not show its array items
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class HideChildrentAttribute : Attribute
{
    public HideChildrentAttribute()
    {
    }
}