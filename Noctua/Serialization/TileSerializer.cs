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
    public sealed class TileSerializer : AssetSerializer<Tile>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<TileDefinition>(stream);

            var tile = new Tile
            {
                Name = definition.Name,
                Texture = Load<Texture2D>(resource, definition.Texture),
                Translucent = definition.Translucent
            };

            return tile;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var tile = asset as Tile;

            var definition = new TileDefinition
            {
                Name = tile.Name,
                Texture = CreateRelativeUri(resource, tile.Texture),
                Translucent = tile.Translucent
            };

            WriteObject(stream, definition);
        }
    }
}
