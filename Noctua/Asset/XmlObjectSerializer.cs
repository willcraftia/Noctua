#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

#endregion

namespace Noctua.Asset
{
    public sealed class XmlObjectSerializer : IObjectSerializer
    {
        public static readonly XmlObjectSerializer Instance = new XmlObjectSerializer();

        Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

        public XmlReaderSettings ReaderSettings { get; private set; }

        public XmlWriterSettings WriterSettings { get; private set; }

        XmlObjectSerializer()
        {
            namespaces.Add(string.Empty, string.Empty);

            WriterSettings = new XmlWriterSettings();
            ReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };
        }

        public object ReadObject(Stream stream, Type type)
        {
            var serializer = GetSerializer(type);
            using (var reader = XmlReader.Create(stream, ReaderSettings))
            {
                return serializer.Deserialize(reader);
            }
        }

        public void WriteObject(Stream stream, object graph)
        {
            var serializer = GetSerializer(graph.GetType());
            using (var writer = XmlWriter.Create(stream, WriterSettings))
            {
                serializer.Serialize(writer, graph, namespaces);
            }
        }

        XmlSerializer GetSerializer(Type type)
        {
            XmlSerializer serializer;
            if (!serializers.TryGetValue(type, out serializer))
            {
                serializer = new XmlSerializer(type);
                serializers[type] = serializer;
            }

            return serializer;
        }
    }
}
