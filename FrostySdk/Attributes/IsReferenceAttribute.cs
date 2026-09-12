using System;

namespace Frosty.Sdk.Attributes;

/// <summary>
/// Specifies that this property is only a reference (does not increment ref-count)
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class IsReferenceAttribute : Attribute
{
    public IsReferenceAttribute()
    {
    }
}