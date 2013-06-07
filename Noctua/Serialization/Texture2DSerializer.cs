#region Using

using System;
using System.IO;
using Libra.IO;
using Libra.Graphics;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class Texture2DSerializer : AssetSerializer<Texture2D>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var texture2D = Device.CreateTexture2D();
            texture2D.Initialize(stream);
            return texture2D;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var texture2D = asset as Texture2D;
            texture2D.Save(DeviceContext, stream);
        }
    }
}
