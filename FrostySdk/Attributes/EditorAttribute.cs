using System;

namespace Frosty.Sdk.Attributes;

/// <summary>
/// Sets the type of property grid editor to use for the property
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class EditorAttribute : Attribute
{
    public Type EditorType { get; set; }
    public EditorAttribute(Type type)
    {
        EditorType = type;
    }
}