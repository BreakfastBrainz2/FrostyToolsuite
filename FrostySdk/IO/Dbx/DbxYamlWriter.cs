using Frosty.Sdk.Ebx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Frosty.Sdk.IO.Dbx;

public sealed class DbxYamlWriter : IDbxDataWriter
{
    private TextWriter m_textWriter;
    private IEmitter m_emitter;

    private readonly Stack<ContainerType> m_containers = new();

    private enum ContainerType
    {
        Mapping,
        Sequence
    }

    public DbxYamlWriter(string inPath)
    {
        m_textWriter = new StreamWriter(inPath);
        m_emitter = new Emitter(m_textWriter);
    }

    public DbxYamlWriter(Stream inStream)
    {
        m_textWriter = new StreamWriter(inStream);
        m_emitter = new Emitter(m_textWriter);
    }

    public void Close()
    {
        m_textWriter.Close();
    }

    public void Create(string inPath)
    {
        Close();
        m_textWriter = new StreamWriter(inPath);
        m_emitter = new Emitter(m_textWriter);
    }

    public void BeginDocument()
    {
        m_emitter.Emit(new StreamStart());
        m_emitter.Emit(new DocumentStart());
    }

    public void EndDocument()
    {
        while (m_containers.Count > 0)
        {
            EndContainer();
        }

        m_emitter.Emit(new DocumentEnd(true));
        m_emitter.Emit(new StreamEnd());
    }

    public void BeginPartition(Guid inAssetGuid, Guid inPrimaryInstanceGuid)
    {
        BeginMapping();

        WriteStringValue("guid", inAssetGuid.ToString());
        WriteStringValue("primaryInstance", inPrimaryInstanceGuid.ToString());

        EmitScalar("instances");
        BeginSequence();
    }

    private void BeginMapping()
    {
        m_emitter.Emit(new MappingStart(
            AnchorName.Empty,
            TagName.Empty,
            false,
            MappingStyle.Block));

        m_containers.Push(ContainerType.Mapping);
    }

    private void BeginSequence()
    {
        m_emitter.Emit(new SequenceStart(
            AnchorName.Empty,
            TagName.Empty,
            false,
            SequenceStyle.Block));

        m_containers.Push(ContainerType.Sequence);
    }

    public void EndPartition()
    {
        EndContainer(); // instances
        EndContainer(); // partition
    }

    public void BeginInstance(AssetClassGuid guid, string type, string? id = null)
    {
        BeginMapping();

        if (id != null)
        {
            WriteStringValue("id", id);
        }

        WriteStringValue("guid", guid.ToString());
        WriteStringValue("type", type);
        WriteBoolValue("exported", guid.IsExported);
    }

    public void EndInstance()
    {
        EndContainer();
    }

    public void BeginElement(string elementName, string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            EmitScalar(name);
        }
        else
        {
            EmitScalar(elementName);
        }

        BeginMapping();
    }

    public void EndElement()
    {
        EndContainer();
    }

    public void BeginArray(string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            EmitScalar(name);
        }

        BeginSequence();
    }

    public void EndArray()
    {
        EndContainer();
    }

    public void BeginValueType(string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            EmitScalar(name);
        }

        BeginMapping();
    }

    public void EndValueType()
    {
        EndContainer();
    }

    public void BeginValue(string? name, bool isArrayElement)
    {
        if (isArrayElement)
        {
            return;
        }

        if (!string.IsNullOrEmpty(name))
        {
            EmitScalar(name);
        }
    }

    public void EndValue()
    {
    }

    public void WriteAttribute(string name, string value)
    {
        WriteStringValue(name, value);
    }

    public void WritePrimitive(object value)
    {
        EmitScalar(value.ToString()!);
    }

    public void WriteString(string value)
    {
        EmitScalar(value);
    }

    public void WriteRef(string refId, string? partitionGuid)
    {
        BeginMapping();

        WriteStringValue("ref", refId);

        if (partitionGuid is not null)
        {
            WriteStringValue("partitionGuid", partitionGuid);
        }

        EndContainer();
    }

    public void Dispose()
    {
        m_textWriter.Dispose();
    }

    private void WriteStringValue(string name, string value)
    {
        EmitScalar(name);
        EmitScalar(value);
    }

    private void WriteBoolValue(string name, bool value)
    {
        EmitScalar(name);

        m_emitter.Emit(new Scalar(
            AnchorName.Empty,
            TagName.Empty,
            value ? "true" : "false",
            ScalarStyle.Plain,
            true,
            false));
    }

    private void EmitScalar(string value)
    {
        m_emitter.Emit(new Scalar(
            AnchorName.Empty,
            TagName.Empty,
            value,
            ScalarStyle.Any,
            true,
            false));
    }

    private void EndContainer()
    {
        if (m_containers.Count == 0)
        {
            throw new InvalidOperationException("no containers to end");
        }

        ContainerType type = m_containers.Pop();

        switch (type)
        {
            case ContainerType.Mapping:
                m_emitter.Emit(new MappingEnd());
                break;

            case ContainerType.Sequence:
                m_emitter.Emit(new SequenceEnd());
                break;
        }
    }
}