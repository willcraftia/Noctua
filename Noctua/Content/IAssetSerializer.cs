#region Using

using System;
using Libra.IO;

#endregion

namespace Noctua.Content
{
    public interface IAssetSerializer
    {
        IAsset Deserialize(IResource resource);

        void Serialize(IResource resource, IAsset asset);
    }
}
