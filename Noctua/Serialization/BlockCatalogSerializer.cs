#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class BlockCatalogSerializer : AssetSerializer<BlockCatalog>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<BlockCatalogDefinition>(stream);

            var blockCatalog = new BlockCatalog
            {
                Name        = definition.Name,
                DirtIndex   = definition.Dirt,
                GrassIndex  = definition.Grass,
                MantleIndex = definition.Mantle,
                SandIndex   = definition.Sand,
                SnowIndex   = definition.Snow,
                StoneIndex  = definition.Stone
            };

            foreach (var entry in definition.Entries)
            {
                var block = Load<Block>(resource, entry.Uri);
                if (block != null)
                {
                    block.Index = entry.Index;
                    blockCatalog.Add(block);
                }
            }

            return blockCatalog;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var blockCatalog = asset as BlockCatalog;

            var definition = new BlockCatalogDefinition
            {
                Name    = blockCatalog.Name,
                Entries = new IndexedUriDefinition[blockCatalog.Count],
                Dirt    = blockCatalog.DirtIndex,
                Grass   = blockCatalog.GrassIndex,
                Mantle  = blockCatalog.MantleIndex,
                Sand    = blockCatalog.SandIndex,
                Snow    = blockCatalog.SnowIndex,
                Stone   = blockCatalog.StoneIndex
            };

            for (int i = 0; i < blockCatalog.Count; i++)
            {
                var block = blockCatalog[i];
                definition.Entries[i] = new IndexedUriDefinition
                {
                    Index = block.Index,
                    Uri = CreateRelativeUri(resource, block)
                };
            }

            WriteObject(stream, definition);
        }
    }
}
