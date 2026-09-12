using Frosty.Sdk.Attributes;
using Frosty.Sdk.Ebx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Frosty.Sdk.Sdk.TypeFlags;

namespace Frosty.Sdk.IO.Dbx;

public interface IDbxDataWriter : IDisposable
{
    void Close();
    void Create(string inPath);

    void BeginDocument();
    void EndDocument();

    void BeginPartition(Guid inAssetGuid, Guid inPrimaryInstanceGuid);
    void EndPartition();

    void BeginInstance(AssetClassGuid guid, string type, string? id = null);
    void EndInstance();

    void BeginArray(string? name);
    void EndArray();

    void BeginValueType(string? name);
    void EndValueType();

    void BeginValue(string? name, bool isArrayElement);
    void EndValue();

    void WriteAttribute(string name, string value);

    void WritePrimitive(object value);
    void WriteString(string value);
    void WriteRef(string refId, string? partitionGuid);
}
