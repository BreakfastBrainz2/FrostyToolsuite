using Frosty.Sdk.Attributes;
using Frosty.Sdk.Ebx;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YamlDotNet.Core.Tokens;

namespace Frosty.Sdk.IO.Dbx;

public sealed class DbxXmlWriter : IDbxDataWriter
{
    private readonly XmlWriterSettings m_settings = new() { Indent = true, IndentChars = "\t", NewLineChars = "\n" };
    private XmlWriter m_xmlWriter;

    public DbxXmlWriter(string inPath)
    {
        m_xmlWriter = XmlWriter.Create(inPath, m_settings);
    }

    public DbxXmlWriter(Stream inStream)
    {
        m_xmlWriter = XmlWriter.Create(inStream, m_settings);
    }

    public void Close()
    {
        m_xmlWriter.Close();
    }

    public void Create(string inPath)
    {
        Close();
        m_xmlWriter = XmlWriter.Create(inPath, m_settings);
    }

    public void BeginDocument()
    {
        m_xmlWriter.WriteStartDocument();
    }

    public void EndDocument()
    {
        m_xmlWriter.WriteEndElement();
    }

    public void BeginPartition(Guid inAssetGuid, Guid inPrimaryInstanceGuid)
    {
        m_xmlWriter!.WriteStartElement("partition");
        WriteAttribute("guid", inAssetGuid.ToString());
        WriteAttribute("primaryInstance", inPrimaryInstanceGuid.ToString());
    }

    public void EndPartition()
    {
        m_xmlWriter!.WriteEndElement();
    }

    public void BeginInstance(AssetClassGuid guid, string type, string? id = null)
    {
        m_xmlWriter.WriteStartElement("instance");
        if (id is not null)
        {
            WriteAttribute("id", id);
        }
        WriteAttribute("guid", guid.ToString());
        WriteAttribute("type", type);
        WriteAttribute("exported", guid.IsExported.ToString());
    }

    public void EndInstance()
    {
        m_xmlWriter.WriteEndElement();
    }

    public void BeginElement(string elementName, string? name)
    {
        m_xmlWriter.WriteStartElement(elementName);
        if (!string.IsNullOrEmpty(name))
        {
            WriteAttribute("name", name);
        }
    }

    public void EndElement()
    {
        m_xmlWriter.WriteEndElement();
    }

    public void BeginArray(string? name)
    {
        BeginElement("array", name);
    }

    public void EndArray()
    {
        EndElement();
    }

    public void BeginValueType(string? name)
    {
        BeginElement("complex", name);
    }

    public void EndValueType()
    {
        EndElement();
    }

    public void BeginValue(string? name, bool isArrayElement)
    {
        if (isArrayElement)
        {
            BeginElement("item", name);
        }
        else
        {
            BeginElement("field", name);
        }
    }

    public void EndValue()
    {
        EndElement();
    }

    public void WriteAttribute(string name, string value)
    {
        m_xmlWriter!.WriteAttributeString(name, value);
    }

    public void WritePrimitive(object value)
    {
        m_xmlWriter.WriteValue(value);
    }

    public void WriteString(string value)
    {
        m_xmlWriter.WriteValue(value);
    }

    public void WriteRef(string refId, string? partitionGuid)
    {
        WriteAttribute("ref", refId);
        if (partitionGuid != null)
        {
            WriteAttribute("partitionGuid", partitionGuid);
        }
    }

    public void Dispose()
    {
        m_xmlWriter.Dispose();
    }
}