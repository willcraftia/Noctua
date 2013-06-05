#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

#endregion

namespace Noctua.Asset
{
    public sealed class JsonAssetSerializer : IAssetSerializer
    {
        public static readonly JsonAssetSerializer Instance = new JsonAssetSerializer();

        Dictionary<Type, DataContractJsonSerializer> serializers = new Dictionary<Type, DataContractJsonSerializer>();

        JsonAssetSerializer() { }

        public T ReadAsset<T>(Stream stream) where T : IAsset
        {
            var serializer = GetSerializer(typeof(T));
            return (T) serializer.ReadObject(stream);
        }

        public void WriteAsset(Stream stream, IAsset asset)
        {
            var serializer = GetSerializer(asset.GetType());
            serializer.WriteObject(stream, asset);
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
