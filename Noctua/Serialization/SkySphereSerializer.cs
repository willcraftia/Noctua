#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class SkySphereSerializer : AssetSerializer<SkySphere>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<SkySphereDefinition>(stream);

            var skySphere = new SkySphere(definition.Name, DeviceContext)
            {
                SunVisible = definition.SunVisible,
                SunThreshold = definition.SunThreshold
            };

            return skySphere;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var skySphere = asset as SkySphere;

            var definition = new SkySphereDefinition
            {
                Name = skySphere.Name,
                SunVisible = skySphere.SunVisible,
                SunThreshold = skySphere.SunThreshold
            };

            WriteObject(stream, definition);
        }
    }
}
