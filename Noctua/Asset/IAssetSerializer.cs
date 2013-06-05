#region Using

using System;
using System.IO;

#endregion

namespace Noctua.Asset
{
    public interface IAssetSerializer
    {
        T ReadAsset<T>(Stream stream) where T : IAsset;

        void WriteAsset(Stream stream, IAsset asset);
    }
}
