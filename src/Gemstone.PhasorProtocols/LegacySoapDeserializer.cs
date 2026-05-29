//******************************************************************************************************
//  LegacySoapDeserializer.cs - Gbtc
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Gemstone.PhasorProtocols;

/// <summary>
/// Deserializes legacy SOAP-formatted XML graphs previously produced by the .NET Framework
/// <c>System.Runtime.Serialization.Formatters.Soap.SoapFormatter</c>.
/// </summary>
/// <remarks>
/// <para>
/// .NET Core / .NET 5+ does not ship <c>SoapFormatter</c>, and existing third-party ports have
/// limitations with interface-typed object references and circular graphs. This deserializer
/// targets the specific dialect of SOAP emitted by .NET Framework's <c>SoapFormatter</c> against
/// <see cref="ISerializable"/> types — flat <c>SOAP-ENV:Body</c>, <c>id</c>/<c>href</c> reference
/// resolution, and CLR <c>nsassem</c> namespace URIs.
/// </para>
/// <para>
/// The deserializer pre-allocates every object node via <see cref="RuntimeHelpers.GetUninitializedObject"/>
/// before invoking any deserialization constructor, so circular references (parent/child) resolve to
/// stable instance identities even though some constructors may not yet have run. Each
/// <c>(<see cref="SerializationInfo"/>, <see cref="StreamingContext"/>)</c> constructor is invoked on the
/// already-allocated instance via a compiled <see cref="DynamicMethod"/>.
/// </para>
/// </remarks>
public static class LegacySoapDeserializer
{
    private static readonly XNamespace s_soapEnvNs = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace s_xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
    private const string ClrNsPrefix = "http://schemas.microsoft.com/clr/nsassem/";

    private static readonly ConcurrentDictionary<Type, Action<object, SerializationInfo, StreamingContext>> s_ctorInvokerCache = new();

    /// <summary>
    /// Deserializes a SOAP-formatted XML graph from <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">Source stream containing legacy SOAP XML.</param>
    /// <param name="binder">
    /// Optional <see cref="SerializationBinder"/> used to translate serialized type names to
    /// current runtime types. Defaults to <see cref="Serialization.LegacyBinder"/>.
    /// </param>
    /// <returns>The root object of the deserialized graph (the first element under <c>SOAP-ENV:Body</c>).</returns>
    public static object? Deserialize(Stream stream, SerializationBinder? binder = null)
    {
        binder ??= Serialization.LegacyBinder;

        XDocument doc = XDocument.Load(stream);
        XElement body = doc.Root?.Element(s_soapEnvNs + "Body")
            ?? throw new SerializationException("SOAP envelope missing Body element.");

        // Pass 1: catalog every id-tagged element in the body, resolve its CLR type
        Dictionary<string, NodeInfo> nodes = new(StringComparer.Ordinal);
        string? rootId = null;

        foreach (XElement element in body.Elements())
        {
            string? id = (string?)element.Attribute("id");

            if (id is null)
                continue;

            Type type = ResolveElementType(element, binder);

            nodes[id] = new NodeInfo(id, type, element);
            rootId ??= id;
        }

        if (rootId is null)
            throw new SerializationException("SOAP body contains no id-tagged elements.");

        // Pass 2: pre-allocate every node so cross-references have stable identities
        foreach (NodeInfo node in nodes.Values)
        {
            if (node.Type == typeof(string))
                node.Instance = node.Element.Value;
            else
                node.Instance = RuntimeHelpers.GetUninitializedObject(node.Type);
        }

        // Pass 3: build SerializationInfo for each non-string node and invoke its deserialization ctor
        StreamingContext context = new(StreamingContextStates.File);

        foreach (NodeInfo node in nodes.Values)
        {
            if (node.Type == typeof(string))
                continue;

            SerializationInfo info = BuildSerializationInfo(node, nodes, binder);
            GetCtorInvoker(node.Type)(node.Instance!, info, context);
        }

        return nodes[rootId].Instance;
    }

    private static Type ResolveElementType(XElement element, SerializationBinder binder)
    {
        (string typeName, string assemblyName) = ParseClrNamespace(element.Name.NamespaceName, element.Name.LocalName);

        return binder.BindToType(assemblyName, typeName)
            ?? throw new SerializationException(
                $"Could not resolve type '{typeName}' in assembly '{assemblyName}' for element '{element.Name.LocalName}'.");
    }

    private static (string typeName, string assemblyName) ParseClrNamespace(string namespaceUri, string elementLocalName)
    {
        if (!namespaceUri.StartsWith(ClrNsPrefix, StringComparison.Ordinal))
            throw new SerializationException($"Element '{elementLocalName}' has non-CLR namespace URI: '{namespaceUri}'.");

        string remainder = namespaceUri[ClrNsPrefix.Length..];
        int lastSlash = remainder.LastIndexOf('/');

        if (lastSlash < 0)
            throw new SerializationException($"Malformed CLR namespace URI: '{namespaceUri}'.");

        string typeNamespace = remainder[..lastSlash];
        string assemblyName = remainder[(lastSlash + 1)..];

        return ($"{typeNamespace}.{elementLocalName}", assemblyName);
    }

    private static SerializationInfo BuildSerializationInfo(NodeInfo node, Dictionary<string, NodeInfo> nodes, SerializationBinder binder)
    {
        SerializationInfo info = new(node.Type, new FormatterConverter());

        foreach (XElement field in node.Element.Elements())
        {
            (object? value, Type valueType) = ResolveFieldValue(field, nodes, binder);
            info.AddValue(field.Name.LocalName, value, valueType);
        }

        return info;
    }

    private static (object? value, Type valueType) ResolveFieldValue(XElement field, Dictionary<string, NodeInfo> nodes, SerializationBinder binder)
    {
        // xsi:null="1" marks the field as null
        XAttribute? nullAttr = field.Attribute(s_xsiNs + "null");

        if (nullAttr is not null && nullAttr.Value == "1")
            return (null, typeof(object));

        // href="#ref-X" — object reference to a pre-allocated instance
        XAttribute? hrefAttr = field.Attribute("href");

        if (hrefAttr is not null)
        {
            string refId = hrefAttr.Value.StartsWith('#') ? hrefAttr.Value[1..] : hrefAttr.Value;

            if (!nodes.TryGetValue(refId, out NodeInfo? refNode))
                throw new SerializationException($"Unresolved object reference '{hrefAttr.Value}'.");

            return (refNode.Instance, refNode.Type);
        }

        // xsi:type="prefix:LocalName" — typed inline value, usually a (flags) enum
        XAttribute? typeAttr = field.Attribute(s_xsiNs + "type");

        if (typeAttr is not null && TryResolveXsiType(field, typeAttr.Value, binder, out Type? resolvedType))
        {
            string text = field.Value;

            if (resolvedType.IsEnum)
                return (Enum.Parse(resolvedType, text), resolvedType);

            // Inline primitive value typed via xsi:type (rare for these graphs but cheap to support)
            if (resolvedType.IsPrimitive || resolvedType == typeof(string) || resolvedType == typeof(decimal))
                return (Convert.ChangeType(text, resolvedType, CultureInfo.InvariantCulture), resolvedType);

            // Fall through — let it be treated as raw text
        }

        // Plain text content (primitives store as string; FormatterConverter handles conversion to the
        // requested numeric/boolean type when the deserialization ctor calls GetInt32/GetUInt16/etc.)
        return (field.Value, typeof(string));
    }

    private static bool TryResolveXsiType(XElement field, string qualifiedTypeName, SerializationBinder binder, out Type? resolvedType)
    {
        resolvedType = null;

        int colonIdx = qualifiedTypeName.IndexOf(':');

        if (colonIdx <= 0)
            return false;

        string prefix = qualifiedTypeName[..colonIdx];
        string localName = qualifiedTypeName[(colonIdx + 1)..];
        XNamespace typeNs = field.GetNamespaceOfPrefix(prefix);

        if (typeNs is null || !typeNs.NamespaceName.StartsWith(ClrNsPrefix, StringComparison.Ordinal))
            return false;

        try
        {
            (string typeName, string assemblyName) = ParseClrNamespace(typeNs.NamespaceName, localName);
            resolvedType = binder.BindToType(assemblyName, typeName);
            return resolvedType is not null;
        }
        catch (SerializationException)
        {
            return false;
        }
    }

    private static Action<object, SerializationInfo, StreamingContext> GetCtorInvoker(Type type)
    {
        return s_ctorInvokerCache.GetOrAdd(type, BuildCtorInvoker);
    }

    private static Action<object, SerializationInfo, StreamingContext> BuildCtorInvoker(Type type)
    {
        ConstructorInfo ctor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            binder: null,
            [typeof(SerializationInfo), typeof(StreamingContext)],
            modifiers: null)
            ?? throw new SerializationException(
                $"Type '{type.FullName}' has no '(SerializationInfo, StreamingContext)' deserialization constructor.");

        DynamicMethod dm = new(
            $"Deserialize_{type.Name}",
            returnType: typeof(void),
            parameterTypes: [typeof(object), typeof(SerializationInfo), typeof(StreamingContext)],
            owner: type,
            skipVisibility: true);

        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, type);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Call, ctor);
        il.Emit(OpCodes.Ret);

        return (Action<object, SerializationInfo, StreamingContext>)
            dm.CreateDelegate(typeof(Action<object, SerializationInfo, StreamingContext>));
    }

    private sealed class NodeInfo(string id, Type type, XElement element)
    {
        public string Id { get; } = id;
        public Type Type { get; } = type;
        public XElement Element { get; } = element;
        public object? Instance { get; set; }
    }
}
