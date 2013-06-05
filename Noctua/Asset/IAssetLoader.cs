#region Using

using System;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public interface IAssetLoader
    {
        T Load<T>(IResource resource) where T : IAsset;
    }
}
