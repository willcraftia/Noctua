#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class GraphicsSettingsSerializer : AssetSerializer<GraphicsSettings>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var settings = ReadObject<GraphicsSettings>(stream);

            if (settings.Shadow == null)
                settings.Shadow = new GraphicsShadowSettings();

            if (settings.Dof == null)
                settings.Dof = new GraphicsDofSettings();

            return settings;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            WriteObject(stream, asset);
        }
    }
}
