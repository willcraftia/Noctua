#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

#endregion

namespace Noctua.Asset
{
    public sealed class JsonObjectSerializer : IObjectSerializer
    {
        public static readonly JsonObjectSerializer Instance = new JsonObjectSerializer();

        Dictionary<Type, DataContractJsonSerializer> serializers = new Dictionary<Type, DataContractJsonSerializer>();

        JsonObjectSerializer() { }

        public object ReadObject(Stream stream, Type type)
        {
            var serializer = GetSerializer(type);
            return serializer.ReadObject(stream);
        }

        public void WriteObject(Stream stream, object graph)
        {
            var serializer = GetSerializer(graph.GetType());
            serializer.WriteObject(stream, graph);
        }

        DataContractJsonSerializer GetSerializer(Type type)
        {
            DataContractJsonSerializer serializer;
            if (!serializers.TryGetValue(type, out serializer))
            {
                serializer = new DataContractJsonSerializer(type);
                serializers[type] = serializer;
            }

            return serializer;
        }
    }
}
