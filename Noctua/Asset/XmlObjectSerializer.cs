#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
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

        XmlReaderSettings readerSettings;

        int indentation;

        public bool Indent { get; set; }

        public char IndentChar { get; set; }

        public int Indentation
        {
            get { return indentation; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                indentation = value;
            }
        }

        XmlObjectSerializer()
        {
            namespaces.Add(string.Empty, string.Empty);

            readerSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };
            Indent = false;
            IndentChar = ' ';
            indentation = 1;
        }

        public object ReadObject(Stream stream, Type type)
        {
            var serializer = GetSerializer(type);
            using (var reader = XmlReader.Create(stream, readerSettings))
            {
                return serializer.Deserialize(reader);
            }
        }

        public void WriteObject(Stream stream, object graph)
        {
            var serializer = GetSerializer(graph.GetType());

            // .NET 2.0 以降、XmlWriter.Create が推奨される方法であるが、
            // JsonObjectSerializer のインタフェースへ合わせるために、
            // XmlTextWriter を直接インスタンス化して利用。
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.Formatting = (Indent) ? Formatting.Indented : Formatting.None;
                writer.IndentChar = IndentChar;
                writer.Indentation = indentation;

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
