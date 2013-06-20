#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class RegionSerializer : AssetSerializer<Region>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<RegionDefinition>(stream);

            var region = new Region
            {
                Name = definition.Name,
                Box = definition.Box,
                TileCatalog = Load<TileCatalog>(resource, definition.TileCatalog),
                BlockCatalog = Load<BlockCatalog>(resource, definition.BlockCatalog),
                BiomeManager = Load<IBiomeManager>(resource, definition.BiomeManager),
                //ChunkBundleResource = Load(resource, definition.ChunkBundle),
            };
            region.ChunkProcesures = LoadChunkProcedures(resource, definition.ChunkProcedures);

            return region;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var region = asset as Region;

            var definition = new RegionDefinition
            {
                Name = region.Name,
                Box = region.Box,
                TileCatalog = CreateRelativeUri(resource, region.TileCatalog),
                BlockCatalog = CreateRelativeUri(resource, region.BlockCatalog),
                BiomeManager = CreateRelativeUri(resource, region.BiomeManager),
                //ChunkBundle = CreateRelativeUri(resource, region.ChunkBundleResource),
                ChunkProcedures = ToChunkProcedureUris(resource, region.ChunkProcesures)
            };

            WriteObject(stream, definition);
        }

        IChunkStore CreateChunkStore(ChunkStoreTypes chunkStoreType, IResource resource)
        {
            switch (chunkStoreType)
            {
                //case ChunkStoreTypes.Storage:
                //    return new StorageChunkStore(resource);
                default:
                    return NullChunkStore.Instance;
            }
        }

        ChunkStoreTypes ToChunkStoreType(IChunkStore chunkStore)
        {
            //if (chunkStore is StorageChunkStore)
            //    return ChunkStoreTypes.Storage;

            return ChunkStoreTypes.None;
        }

        List<IChunkProcedure> LoadChunkProcedures(IResource baseResource, string[] chunkProcedureUris)
        {
            if (chunkProcedureUris == null || chunkProcedureUris.Length == 0)
                return new List<IChunkProcedure>(0);

            var list = new List<IChunkProcedure>(chunkProcedureUris.Length);
            for (int i = 0; i < chunkProcedureUris.Length; i++)
            {
                var procedure = Load<IChunkProcedure>(baseResource, chunkProcedureUris[i]);
                list.Add(procedure);
            }

            return list;
        }

        string[] ToChunkProcedureUris(IResource baseResource, List<IChunkProcedure> procedures)
        {
            if (procedures == null || procedures.Count == 0)
                return null;

            var uris = new string[procedures.Count];
            for (int i = 0; i < procedures.Count; i++)
            {
                var procedureResource = Container.GetResource(procedures[i]);
                uris[i] = CreateRelativeUri(baseResource, procedureResource);
            }

            return uris;
        }
    }
}
