#region Using

using System;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public interface IAssetPreStore
    {
        void PreStore(IResource resource, ResourceManager resourceManager);
    }
}
