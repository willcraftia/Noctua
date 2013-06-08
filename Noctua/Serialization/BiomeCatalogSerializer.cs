#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class BiomeCatalogSerializer : AssetSerializer<BiomeCatalog>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<BiomeCatalogDefinition>(stream);

            var biomeCatalog = new BiomeCatalog()
            {
                Name = definition.Name
            };

            foreach (var entry in definition.Entries)
            {
                var biome = Load<IBiome>(resource, entry.Uri);
                if (biome != null)
                {
                    biome.Index = entry.Index;
                    biomeCatalog.Add(biome);
                }
            }

            return biomeCatalog;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var biomeCatalog = asset as BiomeCatalog;

            var definition = new BiomeCatalogDefinition
            {
                Name = biomeCatalog.Name,
                Entries = new IndexedUriDefinition[biomeCatalog.Count]
            };

            for (int i = 0; i < biomeCatalog.Count; i++)
            {
                var biome = biomeCatalog[i];
                definition.Entries[i] = new IndexedUriDefinition
                {
                    Index = biome.Index,
                    Uri = CreateRelativeUri(resource, biome)
                };
            }

            WriteObject(stream, definition);
        }
    }
}
