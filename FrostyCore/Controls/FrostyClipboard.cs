using Frosty.Sdk;
using Frosty.Sdk.Attributes;
using Frosty.Sdk.Ebx;
using Frosty.Sdk.IO;
using Frosty.Sdk.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Frosty.Sdk.Managers.Entries;
using Frosty.Sdk.Sdk;
using Frosty.Sdk.Utils;

namespace Frosty.Core.Controls;
// ...
// Clipboard
// ...

public class FrostyClipboard : INotifyPropertyChanged
{
    private static FrostyClipboard _instance;
    private FrostyClipboard() { }
    public static FrostyClipboard Current => _instance ?? (_instance = new FrostyClipboard());

    private object Data;

    public bool HasData { get => Data != null; set { } }
    public void SetData(object data)
    {
        Type dataType = data.GetType();
        Dictionary<object, object> oldNewMapping = new();

        Data = DeepCopyValue(data, null, null, ref oldNewMapping);
        RaisePropertyChanged("HasData");
    }
    public bool IsType(Type type)
    {
        return (Data != null && Data.GetType() == type);
    }
    public object GetData(EbxPartition asset, EbxAssetEntry entry)
    {
        Dictionary<object, object> oldNewMapping = new();
        object copyOfData = DeepCopyValue(Data, asset, entry, ref oldNewMapping);
        return copyOfData;
    }
    public object GetData()
    {
        Dictionary<object, object> oldNewMapping = new();
        object copyOfData = DeepCopyValue(Data, null, null, ref oldNewMapping);
        return copyOfData;
    }
    public void Clear()
    {
        Data = null;
    }

    private object DeepCopy(object data, EbxPartition asset, EbxAssetEntry entry, ref Dictionary<object, object> oldNewMapping)
    {
        Type dataType = data.GetType();
        if (dataType.IsPrimitive || dataType.IsValueType)
            return data;

        dynamic newData;
        if (dataType.GetCustomAttribute<EbxTypeMetaAttribute>().Flags.GetTypeEnum() == TypeFlags.TypeEnum.Class)
        {
            if (oldNewMapping.ContainsKey(data))
                return oldNewMapping[data];

            newData = TypeLibrary.CreateObject(dataType.Name);
            oldNewMapping.Add(data, newData);

            AssetClassGuid guid = ((dynamic)data).GetInstanceGuid();
            if (guid.IsExported)
            {
                if (asset != null)
                {
                    Type t = newData.GetType();
                    guid = new AssetClassGuid(Utils.GenerateDeterministicGuid(asset.Instances, entry.Guid), -1);
                    newData.SetInstanceGuid(guid);
                }
                else
                {
                    guid = new AssetClassGuid(Guid.NewGuid(), -1);
                    newData.SetInstanceGuid(guid);
                }
            }
            else
            {
                guid = new AssetClassGuid(-1);
                newData.SetInstanceGuid(guid);
            }

            if (asset != null)
                asset.AddObject(newData);
        }
        else
        {
            newData = TypeLibrary.CreateObject(dataType.Name);
            // the type most likely comes from a plugin if CreateObject returns null, so just create a new instance here
            if (newData == null)
            {
                newData = Activator.CreateInstance(dataType);
            }
        }

        foreach (PropertyInfo pi in dataType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (pi.Name.StartsWith("__"))
                continue;

            dynamic oldValue = pi.GetValue(data);
            pi.SetValue(newData, DeepCopyValue(oldValue, asset, entry, ref oldNewMapping));
        }
        return newData;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private object DeepCopyValue(object obj, EbxPartition asset, EbxAssetEntry entry, ref Dictionary<object, object> oldNewMapping)
    {
        Type objType = obj.GetType();
        if (objType == typeof(PointerRef))
        {
            PointerRef currentPr = (PointerRef)obj;
            PointerRef newPr;

            switch (currentPr.Type)
            {
                case PointerRefType.External:
                    newPr = new PointerRef(currentPr.External);
                    break;
                case PointerRefType.Internal:
                {
                    dynamic newObj = DeepCopy(currentPr.Internal, asset, entry, ref oldNewMapping);

                    newPr = new PointerRef(newObj);
                }
                    break;
                default:
                    newPr = new PointerRef();
                    break;
            }
            return newPr;
        }

        if (objType.GetInterface("IList") != null)
        {
            IList oldList = (IList)obj;
            IList newList = (IList)Activator.CreateInstance(objType);

            newList.Clear();
            for (int i = 0; i < oldList.Count; i++)
                newList.Add(DeepCopyValue(oldList[i], asset, entry, ref oldNewMapping));

            return newList;
        }

        return DeepCopy(obj, asset, entry, ref oldNewMapping);
    }
}