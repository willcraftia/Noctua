#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class TileCatalogSerializer : AssetSerializer<TileCatalog>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<TileCatalogDefinition>(stream);

            var tileCatalog = new TileCatalog(Device)
            {
                Name = definition.Name
            };

            foreach (var entry in definition.Entries)
            {
                var tile = Load<Tile>(resource, entry.Uri);
                if (tile != null)
                {
                    tile.Index = entry.Index;
                    tileCatalog.Add(tile);
                }
            }

            // TODO
            //tileCatalog.DrawMaps(DeviceContext);

            return tileCatalog;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var tileCatalog = asset as TileCatalog;

            var definition = new TileCatalogDefinition
            {
                Name = tileCatalog.Name,
                Entries = new IndexedUriDefinition[tileCatalog.Count]
            };

            for (int i = 0; i < tileCatalog.Count; i++)
            {
                var tile = tileCatalog[i];
                definition.Entries[i] = new IndexedUriDefinition
                {
                    Index = tile.Index,
                    Uri = CreateRelativeUri(resource, tile)
                };
            }

            ObjectSerializer.WriteObject(stream, definition);
        }
    }
}
