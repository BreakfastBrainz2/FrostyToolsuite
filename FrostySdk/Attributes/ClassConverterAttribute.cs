using System;

namespace Frosty.Sdk.Attributes;

/// <summary>
/// Specifies that the class requires a converter to convert to/from some custom type
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ClassConverterAttribute : Attribute
{
    public Type Type { get; set; }
    public ClassConverterAttribute(string inType) { Type = null; }
    public ClassConverterAttribute(Type inType) { Type = inType; }
}