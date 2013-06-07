#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Libra;
using Newtonsoft.Json;

#endregion

namespace Noctua.Asset
{
    public sealed class JsonObjectSerializer : IObjectSerializer
    {
        #region Vector2Converter

        sealed class Vector2Converter : JsonConverter
        {
            public static readonly Vector2Converter Instance = new Vector2Converter();

            Vector2Converter() { }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Vector2);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value as string;
                var elements = value.Split(' ');

                var result = new Vector2();

                if (0 < elements.Length) result.X = float.Parse(elements[0]);
                if (1 < elements.Length) result.Y = float.Parse(elements[1]);

                return result;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (Vector2) value;
                writer.WriteValue(v.X + " " + v.Y);
            }
        }

        #endregion

        #region Vector3Converter

        sealed class Vector3Converter : JsonConverter
        {
            public static readonly Vector3Converter Instance = new Vector3Converter();

            Vector3Converter() { }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Vector3);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value as string;
                var elements = value.Split(' ');

                var result = new Vector3();

                if (0 < elements.Length) result.X = float.Parse(elements[0]);
                if (1 < elements.Length) result.Y = float.Parse(elements[1]);
                if (2 < elements.Length) result.Z = float.Parse(elements[2]);

                return result;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (Vector3) value;
                writer.WriteValue(v.X + " " + v.Y + " " + v.Z);
            }
        }

        #endregion

        #region Vector4Converter

        sealed class Vector4Converter : JsonConverter
        {
            public static readonly Vector4Converter Instance = new Vector4Converter();

            Vector4Converter() { }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Vector4);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value as string;
                var elements = value.Split(' ');

                var result = new Vector4();

                if (0 < elements.Length) result.X = float.Parse(elements[0]);
                if (1 < elements.Length) result.Y = float.Parse(elements[1]);
                if (2 < elements.Length) result.Z = float.Parse(elements[2]);
                if (3 < elements.Length) result.W = float.Parse(elements[3]);

                return result;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (Vector4) value;
                writer.WriteValue(v.X + " " + v.Y + " " + v.Z + " " + v.W);
            }
        }

        #endregion

        public static readonly JsonObjectSerializer Instance = new JsonObjectSerializer();

        JsonSerializer jsonSerializer;

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

        // 型を特殊な形式へ変換してシリアライズすることは止める。
        // ファイル サイズを小さくしたり、可読性を向上させることはできなくなるが、
        // 他の言語から読み込む際に変換を実装したくない。

        JsonObjectSerializer()
        {
            jsonSerializer = new JsonSerializer();
            jsonSerializer.TypeNameHandling = TypeNameHandling.Auto;
            jsonSerializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            //jsonSerializer.Converters.Add(Vector2Converter.Instance);
            //jsonSerializer.Converters.Add(Vector3Converter.Instance);
            //jsonSerializer.Converters.Add(Vector4Converter.Instance);
            Indent = false;
            IndentChar = ' ';
            indentation = 1;
        }

        public object ReadObject(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    return jsonSerializer.Deserialize(reader, type);
                }
                catch (JsonException e)
                {
                    throw new SerializationException("Json deserialization failed: " + type, e);
                }
            }
        }

        public void WriteObject(Stream stream, object graph)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = (Indent) ? Formatting.Indented : Formatting.None;
                jsonWriter.IndentChar = IndentChar;
                jsonWriter.Indentation = indentation;

                try
                {
                    jsonSerializer.Serialize(jsonWriter, graph);
                }
                catch (JsonException e)
                {
                    throw new SerializationException("Json serialization failed: " + graph.GetType(), e);
                }
            }
        }
    }
}
