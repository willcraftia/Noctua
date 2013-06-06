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

        public virtual void Initialize(AssetContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");

            Container = container;
        }

        public abstract object ReadAsset(Stream stream, IResource resource);

        public abstract void WriteAsset(Stream stream, IResource resource, object asset);
    }
}
