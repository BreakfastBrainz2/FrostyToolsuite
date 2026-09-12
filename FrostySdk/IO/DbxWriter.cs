using Frosty.Sdk.Attributes;
using Frosty.Sdk.Ebx;
using Frosty.Sdk.Interfaces;
using Frosty.Sdk.IO.Dbx;
using Frosty.Sdk.Managers;
using Frosty.Sdk.Managers.Entries;
using Microsoft.VisualBasic.FileIO;
using Octokit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using YamlDotNet.Core.Tokens;
using static Frosty.Sdk.Sdk.TypeFlags;

namespace Frosty.Sdk.IO;

public sealed class EbxField
{
    public string Name { get; private set; }
    public Type Type { get; private set; }
    public PropertyInfo Property { get; private set; }
    public EbxFieldMetaAttribute Meta { get; private set; }

    public bool IsTransient { get; private set; }
    public bool IsHidden { get; private set; }

    public TypeEnum FieldType { get; private set; }
    public CategoryEnum FieldCategory { get; private set; }

    public Type? DeclaringType => Property.DeclaringType;

    public EbxField(PropertyInfo property)
    {
        Name = property.Name;
        Type = property.PropertyType;
        Property = property;
        Meta = property.GetCustomAttribute<EbxFieldMetaAttribute>()!;

        IsTransient = property.GetCustomAttribute<IsTransientAttribute>() != null;
        IsHidden = property.GetCustomAttribute<IsHiddenAttribute>() != null;

        FieldType = Meta.Flags.GetTypeEnum();
        FieldCategory = Meta.Flags.GetCategoryEnum();
    }

    public object? GetValue(object target)
    {
        return Property.GetValue(target);
    }
}

public static class DbxTypeCache
{
    private static readonly Dictionary<Type, List<EbxField>> s_cache = new();

    public static List<EbxField> GetMembers(object inObj)
    {
        Type objType = inObj.GetType();

        if(s_cache.TryGetValue(objType, out var values))
        {
            return values;
        }

        List<EbxField> members = new();

        var properties = GetAllProperties(objType, true, true);

        foreach(var pi in properties)
        {
            members.Add(new EbxField(pi));
        }

        s_cache.Add(objType, members);
        return members;
    }

    private static List<PropertyInfo> GetAllProperties(Type classType, bool checkBaseTypes = false, bool shouldSort = false)
    {
        List<PropertyInfo> props = new();
        GetAllProperties(classType, ref props, checkBaseTypes, shouldSort);
        return props;
    }

    private static void GetAllProperties(Type classType, ref List<PropertyInfo> properties, bool checkBaseTypes = false, bool shouldSort = false)
    {
        PropertyInfo[] currentTypeProps = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (PropertyInfo pi in currentTypeProps)
        {
            if (pi.Name.Equals("__InstanceGuid") || pi.Name.Equals("__Id"))
            {
                continue;
            }

            properties.Add(pi);
        }

        if (checkBaseTypes)
        {
            Type? baseType = classType.BaseType;
            if (baseType is not null)
            {
                GetAllProperties(baseType, ref properties, checkBaseTypes, shouldSort);
            }
        }

        if (shouldSort)
        {
            properties.Sort((p1, p2) =>
            {
                int index1 = p1.GetCustomAttribute<FieldIndexAttribute>()?.Index ?? -1;
                int index2 = p2.GetCustomAttribute<FieldIndexAttribute>()?.Index ?? -1;

                return index1.CompareTo(index2);
            });
        }
    }
}

public enum DbxFormat
{
    Xml,
    Yaml
}

public sealed class DbxWriter : IDisposable
{
    private string m_filePath;
    private IDbxDataWriter? m_dataWriter;

    public DbxWriter()
    {
        m_filePath = string.Empty;
    }

    public DbxWriter(string inFilePath)
    {
        m_filePath = inFilePath;
        m_dataWriter = new DbxYamlWriter(inFilePath);
    }

    public DbxWriter(Stream inStream)
    {
        m_filePath = string.Empty;
        m_dataWriter = new DbxYamlWriter(inStream);
    }

    public void Write(EbxPartition inPartition)
    {
        if (!inPartition.IsValid)
        {
            return;
        }

        WriteAsset(inPartition);
    }

    public void Write(EbxPartition inPartition, string inFilePath)
    {
        if(!inPartition.IsValid)
        {
            return;
        }

        m_dataWriter?.Close();
        m_dataWriter?.Create(inFilePath);

        m_filePath = inFilePath;

        WriteAsset(inPartition);
    }

    public void Dispose()
    {
        m_dataWriter?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Writes the given ebx object to the dbx as an instance.
    /// </summary>
    /// <param name="ebxObj"></param>
    private void WriteInstance(object ebxObj)
    {
        Debug.Assert(ebxObj.GetType().IsClass, "DbxWriter: instance isn't a class?");

        AssetClassGuid guid = ((dynamic)ebxObj).GetInstanceGuid();
        Type ebxType = ebxObj.GetType();

        m_dataWriter!.BeginInstance(guid, $"{ebxType.Namespace}.{ebxType.GetName()}");

        foreach (var ebxField in DbxTypeCache.GetMembers(ebxObj))
        {
            object? fieldValue = ebxField.GetValue(ebxObj);
            if (fieldValue is null)
            {
                continue;
            }

            WriteValue(ebxField, fieldValue, false);
        }

        m_dataWriter.EndInstance();
    }

    private void WriteValue(EbxField ebxField, object obj, bool isArrayElement)
    {
        if(obj is ICollection collection)
        {
            WriteArray(ebxField, obj);
        }
        else
        {
            WriteValue(ebxField.Name, ebxField.FieldType, obj, isArrayElement);
        }
    }

    private void WriteValue(string? name, TypeEnum fieldType, object obj, bool isArrayElement)
    {
        switch (fieldType)
        {
            case TypeEnum.Boolean:
            case TypeEnum.Int8:
            case TypeEnum.UInt8:
            case TypeEnum.Int16:
            case TypeEnum.UInt16:
            case TypeEnum.Int32:
            case TypeEnum.UInt32:
            case TypeEnum.Int64:
            case TypeEnum.UInt64:
            case TypeEnum.Float32:
            case TypeEnum.Float64:
            case TypeEnum.CString:
            case TypeEnum.String:
            case TypeEnum.Guid:
            case TypeEnum.ResourceRef:
            case TypeEnum.Sha1:
            case TypeEnum.FileRef:
                WritePrimitive(name, obj, isArrayElement);
                break;
            case TypeEnum.Enum:
                WriteEnum(name, (Enum)obj, isArrayElement);
                break;
            case TypeEnum.Class:
                WriteClassRef(name, (PointerRef)obj, isArrayElement);
                break;
            case TypeEnum.Struct:
                WriteValueType(name, obj);
                break;
            case TypeEnum.TypeRef:
                WriteTypeRef(name, (TypeRef)obj, isArrayElement);
                break;
            case TypeEnum.Delegate:
                WriteDelegate(name, (IDelegate)obj, isArrayElement);
                break;
            case TypeEnum.BoxedValueRef:
                WriteBoxedValueRef(name, (BoxedValueRef)obj, isArrayElement);
                break;
            default:
                throw new NotImplementedException($"DbxWriter: unimplemented field type {fieldType}");
        }
    }

    private void WriteDelegate(string? name, IDelegate value, bool isArrayElement)
    {
        m_dataWriter!.BeginValue(name, isArrayElement);
        m_dataWriter.WriteAttribute("function", value.FunctionType?.Name ?? "null");
        m_dataWriter.EndValue();
    }

    private void WriteClassRef(string? name, PointerRef value, bool isArrayElement)
    {
        m_dataWriter!.BeginValue(name, isArrayElement);
        if (value.Type == PointerRefType.Internal)
        {
            AssetClassGuid classGuid = value.Internal!.GetInstanceGuid();
            m_dataWriter!.WriteRef(classGuid.ToString(), null);
        }
        else if (value.Type == PointerRefType.External)
        {
            EbxAssetEntry? entry = AssetManager.GetEbxAssetEntry(value.External.PartitionGuid);
            if (entry is not null)
            {
                m_dataWriter!.WriteRef($"{entry.Name}/{value.External.InstanceGuid}", entry.Guid.ToString());
            }
            else
            {
                m_dataWriter!.WriteRef("null", "null");
            }
        }
        else
        {
            m_dataWriter!.WriteRef("null", null);
        }
        m_dataWriter.EndValue();
    }

    private void WriteTypeRef(string? name, TypeRef typeRef, bool isArrayElement)
    {
        m_dataWriter!.BeginValue(name, isArrayElement);
        m_dataWriter.WriteAttribute("typeName", typeRef.Name ?? string.Empty);
        m_dataWriter.EndValue();
    }

    private void WriteBoxedValueRef(string? name, BoxedValueRef boxedValue, bool isArrayElement)
    {
        m_dataWriter!.BeginValue(name, isArrayElement);
        if(boxedValue.Value is not null)
        {
            TypeEnum type = boxedValue.Type;
            if(boxedValue.Category is CategoryEnum.Array)
            {
                type = TypeEnum.Array;
            }

            WriteValue(name, type, boxedValue.Value, false);
        }
        m_dataWriter.EndValue();
    }

    private void WriteValueType(string? name, object value)
    {
        m_dataWriter!.BeginValueType(name);
        foreach(var ebxField in DbxTypeCache.GetMembers(value))
        {
            WriteValue(ebxField, ebxField.GetValue(value)!, false);
        }
        m_dataWriter.EndValueType();
    }

    private void WriteArray(EbxField ebxField, object arrayObj)
    {
        Type memberType = ebxField.Type.GenericTypeArguments[0];
        bool isRef = memberType.Name == "PointerRef";

        if (isRef)
        {
            memberType = ebxField.Meta.BaseType!;
        }

        EbxTypeMetaAttribute memberMeta = memberType.GetCustomAttribute<EbxTypeMetaAttribute>()!;

        string typeDisplayName = memberType.GetName();

        ICollection elements = (ICollection)arrayObj;

        m_dataWriter!.BeginArray(ebxField.Name);
        foreach(object elem in elements)
        {
            WriteValue(null, memberMeta.Flags.GetTypeEnum(), elem, true);
        }
        m_dataWriter.EndArray();
    }

    private void WriteEnum(string? name, Enum value, bool isArrayElement)
    {
        m_dataWriter!.BeginValue(name, isArrayElement);
        m_dataWriter.WriteString(value.ToString());
        m_dataWriter.EndValue();
    }

    private void WritePrimitive(string? name, object value, bool isArrayElement)
    {
        m_dataWriter!.BeginValue(name, isArrayElement);

        object actualVal = value is IPrimitive prim ? prim.ToActualType() : value;

        m_dataWriter.WritePrimitive(actualVal);
        m_dataWriter.EndValue();
    }

    private void WriteAsset(EbxPartition inPartition)
    {
        m_dataWriter!.BeginDocument();

        m_dataWriter.BeginPartition(inPartition.PartitionGuid, inPartition.PrimaryInstanceGuid);

        foreach (object ebxObj in inPartition.instances)
        {
            WriteInstance(ebxObj);
        }

        m_dataWriter.EndPartition();
    }
}
