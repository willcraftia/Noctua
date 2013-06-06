#region Using

using System;
using System.IO;

#endregion

namespace Noctua.Asset
{
    public interface IObjectSerializer
    {
        object ReadAsset(Stream stream, Type type);

        void WriteAsset(Stream stream, object asset);
    }
}
