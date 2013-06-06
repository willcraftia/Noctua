#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Noctua.Asset
{
    public sealed class JsonObjectSerializer : IObjectSerializer
    {
        public static readonly JsonObjectSerializer Instance = new JsonObjectSerializer();

        public JsonSerializer JsonSerializer { get; private set; }

        JsonObjectSerializer()
        {
            JsonSerializer = new JsonSerializer();
            JsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
        }

        public object ReadObject(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    return JsonSerializer.Deserialize(reader, type);
                }
                catch (JsonException e)
                {
                    throw new SerializationException("Json deserialization failed: " + type, e);
                }
            }
        }

        public void WriteObject(Stream stream, object graph)
        {
            using (var writer = new StreamWriter(stream))
            {
                try
                {
                    JsonSerializer.Serialize(writer, graph);
                }
                catch (JsonException e)
                {
                    throw new SerializationException("Json serialization failed: " + graph.GetType(), e);
                }
            }
        }
    }
}
