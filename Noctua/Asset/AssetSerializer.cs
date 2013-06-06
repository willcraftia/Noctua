#region Using

using System;
using System.IO;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public abstract class AssetSerializer
    {
        public abstract Type AssetType { get; }

        protected AssetContainer Container { get; private set; }

        protected IObjectSerializer ObjectSerializer { get; private set; }

        public virtual void Initialize(AssetContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            Container = container;
            ObjectSerializer = container.ObjectSerializer;
        }

        public abstract object ReadAsset(Stream stream, IResource resource);

        public abstract void WriteAsset(Stream stream, IResource resource, object asset);

        protected T ReadObject<T>(Stream stream)
        {
            return (T) ReadObject(stream, typeof(T));
        }

        protected object ReadObject(Stream stream, Type type)
        {
            return ObjectSerializer.ReadObject(stream, type);
        }

        protected void WriteObject(Stream stream, object graph)
        {
            ObjectSerializer.WriteObject(stream, graph);
        }
    }
}
