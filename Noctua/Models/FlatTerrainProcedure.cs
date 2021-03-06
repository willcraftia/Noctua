﻿#region Using

using System;
using System.ComponentModel;

#endregion

namespace Noctua.Models
{
    public sealed class FlatTerrainProcedure : IChunkProcedure
    {
        public const int DefaultHeight = 256;

        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(DefaultHeight)]
        public int Height { get; set; }

        public FlatTerrainProcedure()
        {
            Height = DefaultHeight;
        }

        public void Generate(Region region, Chunk chunk)
        {
            //var chunkSize = chunk.Size;
            //var chunkPosition = chunk.Position;
            //var biome = Region.BiomeManager.GetBiome(chunk);

            //for (int x = 0; x < chunkSize.X; x++)
            //{
            //    for (int z = 0; z < chunkSize.Z; z++)
            //    {
            //        var biomeElement = GetBiomeElement(chunk, biome, x, z);

            //        for (int y = 0; y < chunkSize.Y; y++)
            //            Generate(chunk, ref chunkSize, ref chunkPosition, x, y, z, biomeElement);
            //    }
            //}

            throw new NotImplementedException();
        }

        //BiomeElement GetBiomeElement(Chunk chunk, IBiome biome, int x, int z)
        //{
        //    var absoluteX = chunk.GetAbsoluteBlockPositionX(x);
        //    var absoluteZ = chunk.GetAbsoluteBlockPositionZ(z);
        //    return biome.GetBiomeElement(absoluteX, absoluteZ);
        //}

        //void Generate(Chunk chunk, ref VectorI3 chunkSize, ref VectorI3 chunkPosition, int x, int y, int z, BiomeElement biomeElement)
        //{
        //    var h = chunkPosition.Y * chunkSize.Y + y;

        //    byte index = Block.EmptyIndex;

        //    if (Height == h)
        //    {
        //        // Horizon.
        //        switch (biomeElement)
        //        {
        //            case BiomeElement.Desert:
        //                index = Region.BlockCatalog.SandIndex;
        //                break;
        //            case BiomeElement.Forest:
        //                index = Region.BlockCatalog.DirtIndex;
        //                break;
        //            case BiomeElement.Mountains:
        //                index = Region.BlockCatalog.StoneIndex;
        //                break;
        //            case BiomeElement.Plains:
        //                index = Region.BlockCatalog.GrassIndex;
        //                break;
        //            case BiomeElement.Snow:
        //                index = Region.BlockCatalog.SnowIndex;
        //                break;
        //        }
        //    }
        //    else if (h < Height)
        //    {
        //        // Below the horizon.
        //        switch (biomeElement)
        //        {
        //            case BiomeElement.Desert:
        //                index = Region.BlockCatalog.SandIndex;
        //                break;
        //            case BiomeElement.Forest:
        //                index = Region.BlockCatalog.DirtIndex;
        //                break;
        //            case BiomeElement.Mountains:
        //                index = Region.BlockCatalog.StoneIndex;
        //                break;
        //            case BiomeElement.Plains:
        //                index = Region.BlockCatalog.DirtIndex;
        //                break;
        //            case BiomeElement.Snow:
        //                index = Region.BlockCatalog.SnowIndex;
        //                break;
        //        }
        //    }

        //    var position = new VectorI3(x, y, z);
        //    chunk.SetBlockIndex(ref position, index);
        //}

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
