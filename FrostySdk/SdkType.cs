using System;
using System.Diagnostics;
using System.Reflection;
using Frosty.Sdk.Attributes;
using Frosty.Sdk.Ebx;
using Frosty.Sdk.Interfaces;
using Frosty.Sdk.Sdk;

namespace Frosty.Sdk;

public class SdkType : IType
{
    public string Name { get; }
    public uint NameHash { get; }
    public Guid Guid { get; }
    public uint Signature { get; }

    public Type Type { get; }
    // for arrays
    public Type? ElementType { get; }

    public bool IsArray { get; }

    private static readonly string s_collectionName = "ObservableCollection`1";

    public SdkType(Type inType)
    {
        Type = inType;
        IsArray = inType.Name == s_collectionName;

        if(IsArray)
        {
            ElementType = inType.GenericTypeArguments[0] == typeof(PointerRef) ? TypeLibrary.GetType("DataContainer")!.Type : inType.GenericTypeArguments[0];
            Debug.Assert(ElementType != null);

            Name = ElementType.GetCustomAttribute<ArrayNameAttribute>()?.Name ?? ElementType.Name + "-Array";
            NameHash = ElementType.GetCustomAttribute<ArrayHashAttribute>()?.Hash ?? uint.MaxValue;
            Guid = ElementType.GetCustomAttribute<ArrayGuidAttribute>()?.Guid ?? Guid.Empty;
            Signature = ElementType.GetCustomAttribute<SignatureAttribute>()?.Signature ?? uint.MaxValue;
        }
        else
        {
            Name = inType.GetCustomAttribute<DisplayNameAttribute>()?.Name ?? inType.Name;
            NameHash = inType.GetCustomAttribute<NameHashAttribute>()?.Hash ?? uint.MaxValue;
            Guid = inType.GetCustomAttribute<GuidAttribute>()?.Guid ?? Guid.Empty;
            Signature = inType.GetCustomAttribute<SignatureAttribute>()?.Signature ?? uint.MaxValue;
        }
    }

    public bool IsSubClassOf(IType inType)
    {
        return Type.IsSubclassOf(inType.Type);
    }

    public TypeFlags GetFlags()
    {
        Type type = Type;
        if (Type.Name == s_collectionName)
        {
            type = Type.GenericTypeArguments[0];
        }
        TypeFlags? flags = type.GetCustomAttribute<EbxTypeMetaAttribute>()?.Flags;
        if (!flags.HasValue)
        {
            // should only be PointerRef for writing boxed class values
            return new TypeFlags(TypeFlags.TypeEnum.Class, TypeFlags.CategoryEnum.Class);
        }
        return new TypeFlags(flags.Value.GetTypeEnum(), flags.Value.GetCategoryEnum());
    }
}