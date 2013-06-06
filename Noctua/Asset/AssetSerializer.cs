#region Using

using System;
using System.IO;
using Libra.Graphics;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public abstract class AssetSerializer
    {
        public abstract Type AssetType { get; }

        protected AssetContainer Container { get; private set; }

        protected Device Device { get; private set; }

        protected DeviceContext DeviceContext { get; private set; }

        protected ResourceManager ResourceManager { get; private set; }

        protected IObjectSerializer ObjectSerializer { get; private set; }

        public virtual void Initialize(AssetContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            Container = container;
            Device = container.DeviceContext.Device;
            DeviceContext = container.DeviceContext;
            ResourceManager = container.ResourceManager;
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
            if (stream == null) throw new ArgumentNullException("stream");
            if (type == null) throw new ArgumentNullException("type");

            return ObjectSerializer.ReadObject(stream, type);
        }

        protected void WriteObject(Stream stream, object graph)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (graph == null) throw new ArgumentNullException("graph");

            ObjectSerializer.WriteObject(stream, graph);
        }

        protected T Load<T>(IResource baseResource, string externalAssetUri)
        {
            if (baseResource == null) throw new ArgumentNullException("baseResource");

            if (string.IsNullOrEmpty(externalAssetUri)) return default(T);

            var resource = ResourceManager.Load(baseResource, externalAssetUri);
            return Container.Load<T>(resource);
        }

        protected string CreateRelativeUri(IResource baseResource, object externalAsset)
        {
            if (baseResource == null) throw new ArgumentNullException("baseResource");

            if (externalAsset == null) return null;

            // 外部アセットのリソースを取得。
            var externalResource = Container.GetResource(externalAsset);

            // 可能ならば相対 URI を生成 (さもなくば絶対 URI)。
            return ResourceManager.CreateRelativeUri(baseResource, externalResource);
        }
    }

    public abstract class AssetSerializer<TAsset> : AssetSerializer
    {
        public override Type AssetType
        {
            get { return typeof(TAsset); }
        }
    }
}
