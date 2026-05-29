//******************************************************************************************************
//  LegacySoapSerializer.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/28/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Gemstone.PhasorProtocols;

/// <summary>
/// Serializes <see cref="ISerializable"/> object graphs as legacy SOAP-formatted XML, matching the
/// wire format produced by the .NET Framework <c>System.Runtime.Serialization.Formatters.Soap.SoapFormatter</c>.
/// </summary>
/// <remarks>
/// <para>
/// Pairs with <see cref="LegacySoapDeserializer"/> to round-trip phasor protocol configuration frames
/// in .NET Core / .NET 5+ without dependence on the third-party SoapFormatter NuGet port.
/// </para>
/// <para>
/// Discovers the object graph breadth-first by invoking each instance's
/// <see cref="ISerializable.GetObjectData"/>, assigns sequential <c>ref-N</c> identifiers, and emits a
/// flat <c>SOAP-ENV:Body</c> with one element per object. Object-typed field values become <c>href</c>
/// references, enums get inline <c>xsi:type</c> annotations with their CLR namespace declared inline,
/// nulls become <c>xsi:null="1"</c>, and primitives are written as text content.
/// </para>
/// </remarks>
public static class LegacySoapSerializer
{
    private const string ClrNsPrefix = "http://schemas.microsoft.com/clr/nsassem/";
    private const string SoapEnvNs = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string SoapEncNs = "http://schemas.xmlsoap.org/soap/encoding/";
    private const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";
    private const string XsdNs = "http://www.w3.org/2001/XMLSchema";
    private const string ClrSoapNs = "http://schemas.microsoft.com/soap/encoding/clr/1.0";

    // The original SoapFormatter writes UTF-8 without a BOM; match that.
    private static readonly Encoding s_outputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Serializes <paramref name="root"/> and its reachable graph as legacy SOAP XML.
    /// </summary>
    /// <param name="stream">Destination stream for the XML payload. Not closed by this method.</param>
    /// <param name="root">Root object — must be marked <see cref="SerializableAttribute"/> and (almost always) implement <see cref="ISerializable"/>.</param>
    /// <param name="binder">
    /// Optional <see cref="SerializationBinder"/> used to translate runtime types to legacy emitted names.
    /// Defaults to <see cref="Serialization.LegacyBinder"/>, which emits <c>GSF.*</c> names so output remains
    /// readable by .NET Framework consumers (PMU Connection Tester, older openPDC builds). Pass a custom
    /// binder (or one whose <c>BindToName</c> returns the runtime type unchanged) to emit native names.
    /// </param>
    public static void Serialize(Stream stream, object root, SerializationBinder? binder = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(root);

        binder ??= Serialization.LegacyBinder;

        StreamingContext context = new(StreamingContextStates.File);

        // Pass 1: BFS the object graph, assign ref-Ns, capture SerializationInfo per node
        Dictionary<object, string> idMap = new(ReferenceEqualityComparer.Instance);
        List<NodeInfo> graph = [];
        Queue<object> queue = new();
        int nextId = 1;

        idMap[root] = $"ref-{nextId++}";
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            object obj = queue.Dequeue();
            Type type = obj.GetType();

            if (!type.IsSerializable)
                throw new SerializationException($"Type '{type.FullName}' is not marked [Serializable].");

            if (obj is not ISerializable serializable)
                throw new SerializationException(
                    $"Type '{type.FullName}' does not implement ISerializable. LegacySoapSerializer only supports ISerializable graphs.");

            SerializationInfo info = new(type, new FormatterConverter());
            serializable.GetObjectData(info, context);

            // Enqueue any newly-discovered object references for later processing
            foreach (SerializationEntry entry in info.GetValues())
            {
                if (!IsObjectReference(entry.Value))
                    continue;

                object child = entry.Value!;

                if (idMap.ContainsKey(child))
                    continue;

                idMap[child] = $"ref-{nextId++}";
                queue.Enqueue(child);
            }

            graph.Add(new NodeInfo(idMap[obj], obj, info));
        }

        // Pass 2: emit
        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = string.Empty,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = true,
            CloseOutput = false,
            Encoding = s_outputEncoding,
        };

        // Wrap the stream in a writer with the BOM-less encoding so XmlWriter honors it.
        using StreamWriter streamWriter = new(stream, s_outputEncoding, bufferSize: 4096, leaveOpen: true);
        using XmlWriter writer = XmlWriter.Create(streamWriter, settings);
        WriteEnvelope(writer, graph, idMap, binder, new PrefixManager());
        writer.Flush();
    }

    private static bool IsObjectReference(object? value)
    {
        if (value is null)
            return false;

        Type t = value.GetType();
        return !t.IsPrimitive && !t.IsEnum && t != typeof(string) && t != typeof(decimal);
    }

    private static void WriteEnvelope(XmlWriter writer, List<NodeInfo> graph, Dictionary<object, string> idMap, SerializationBinder binder, PrefixManager prefixes)
    {
        writer.WriteStartElement("SOAP-ENV", "Envelope", SoapEnvNs);
        writer.WriteAttributeString("xmlns", "xsi", null, XsiNs);
        writer.WriteAttributeString("xmlns", "xsd", null, XsdNs);
        writer.WriteAttributeString("xmlns", "SOAP-ENC", null, SoapEncNs);
        writer.WriteAttributeString("xmlns", "clr", null, ClrSoapNs);
        writer.WriteAttributeString("encodingStyle", SoapEnvNs, SoapEncNs);

        writer.WriteStartElement("SOAP-ENV", "Body", SoapEnvNs);

        foreach (NodeInfo node in graph)
            WriteNode(writer, node, idMap, binder, prefixes);

        writer.WriteEndElement(); // Body
        writer.WriteEndElement(); // Envelope
    }

    private static void WriteNode(XmlWriter writer, NodeInfo node, Dictionary<object, string> idMap, SerializationBinder binder, PrefixManager prefixes)
    {
        Type type = node.Obj.GetType();
        (string nsUri, string localName) = ResolveClrName(type, binder);
        string prefix = prefixes.GetOrAssign(nsUri);

        writer.WriteStartElement(prefix, localName, nsUri);
        writer.WriteAttributeString("id", node.Id);

        foreach (SerializationEntry entry in node.Info.GetValues())
            WriteField(writer, entry, idMap, binder, prefixes);

        writer.WriteEndElement();
    }

    private static void WriteField(XmlWriter writer, SerializationEntry entry, Dictionary<object, string> idMap, SerializationBinder binder, PrefixManager prefixes)
    {
        writer.WriteStartElement(entry.Name);

        object? value = entry.Value;

        if (value is null)
        {
            writer.WriteAttributeString("xsi", "null", XsiNs, "1");
        }
        else if (value is string s)
        {
            writer.WriteString(s);
        }
        else
        {
            Type valueType = value.GetType();

            if (valueType.IsEnum)
            {
                // Resolve the enum's CLR namespace and look up (or assign) its document-wide prefix. Using a
                // per-document prefix table — instead of a fixed "a2" on every inline element — prevents the
                // legacy .NET Framework SoapFormatter (PMU Connection Tester) from misbinding xsi:type when
                // two distinct namespaces would otherwise reuse the same prefix on adjacent inline elements.
                (string enumNsUri, string enumLocalName) = ResolveClrName(valueType, binder);
                string enumPrefix = prefixes.GetOrAssign(enumNsUri);
                writer.WriteAttributeString("xmlns", enumPrefix, null, enumNsUri);
                writer.WriteAttributeString("xsi", "type", XsiNs, $"{enumPrefix}:{enumLocalName}");
                writer.WriteString(value.ToString());
            }
            else if (valueType.IsPrimitive || value is decimal)
            {
                writer.WriteString(FormatPrimitive(value));
            }
            else
            {
                // Object reference — must already be in id map
                if (!idMap.TryGetValue(value, out string? refId))
                    throw new SerializationException($"Object reference for field '{entry.Name}' was not discovered during graph traversal.");

                writer.WriteAttributeString("href", $"#{refId}");
            }
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Resolves the legacy-format CLR namespace URI and element local name for <paramref name="type"/>,
    /// honoring any name remapping the configured <paramref name="binder"/> provides.
    /// </summary>
    private static (string nsUri, string localName) ResolveClrName(Type type, SerializationBinder binder)
    {
        binder.BindToName(type, out string? mappedAssembly, out string? mappedTypeName);

        string fullTypeName = mappedTypeName ?? type.FullName ?? type.Name;
        string assemblyName = mappedAssembly
            ?? type.Assembly.GetName().Name
            ?? throw new SerializationException($"Type '{type.FullName}' is in an assembly with no simple name.");

        int lastDot = fullTypeName.LastIndexOf('.');
        string typeNs = lastDot > 0 ? fullTypeName[..lastDot] : string.Empty;
        string localName = lastDot > 0 ? fullTypeName[(lastDot + 1)..] : fullTypeName;

        return ($"{ClrNsPrefix}{typeNs}/{assemblyName}", localName);
    }

    private static string FormatPrimitive(object value)
    {
        return value switch
        {
            bool b => b ? "true" : "false",
            float f => f.ToString("R", CultureInfo.InvariantCulture),
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private sealed class NodeInfo(string id, object obj, SerializationInfo info)
    {
        public string Id { get; } = id;
        public object Obj { get; } = obj;
        public SerializationInfo Info { get; } = info;
    }

    /// <summary>
    /// Assigns and remembers a document-wide unique <c>aN</c> prefix per distinct CLR namespace URI so the
    /// legacy .NET Framework SoapFormatter's flat prefix→URI binding stays unambiguous across the file.
    /// </summary>
    private sealed class PrefixManager
    {
        private readonly Dictionary<string, string> _uriToPrefix = new(StringComparer.Ordinal);
        private int _next = 1;

        public string GetOrAssign(string uri)
        {
            if (!_uriToPrefix.TryGetValue(uri, out string? prefix))
            {
                prefix = $"a{_next++}";
                _uriToPrefix[uri] = prefix;
            }

            return prefix;
        }
    }
}
