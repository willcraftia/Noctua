#region Using

using System;
using System.IO;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class LensFlareMeshSerializer : AssetSerializer<LensFlareMesh>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<LensFlareMeshDefinition>(stream);

            var lensFlareMesh = new LensFlareMesh(definition.Name, DeviceContext)
            {
                QuerySize = definition.QuerySize,
                LightName = definition.LightName
            };

            foreach (var flareDefinition in definition.Flares)
            {
                var flare = new LensFlare.Flare(
                    flareDefinition.Position,
                    flareDefinition.Scale,
                    new Color(flareDefinition.Color),
                    Load<Texture2D>(resource, flareDefinition.Texture));

                lensFlareMesh.Flares.Add(flare);
            }

            return lensFlareMesh;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var lensFlareMesh = asset as LensFlareMesh;

            var definition = new LensFlareMeshDefinition
            {
                QuerySize = lensFlareMesh.QuerySize,
                LightName = lensFlareMesh.LightName
            };

            definition.Flares = new LensFlareElementDefinition[lensFlareMesh.Flares.Count];

            for (int i = 0; i < lensFlareMesh.Flares.Count; i++)
            {
                var flare = lensFlareMesh.Flares[i];

                definition.Flares[i] = new LensFlareElementDefinition(
                    flare.Position,
                    flare.Scale,
                    flare.Color.ToVector3(),
                    CreateRelativeUri(resource, flare.Texture));
            }

            WriteObject(stream, definition);
        }
    }
}
