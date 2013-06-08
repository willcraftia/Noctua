#region Using

using System;
using System.ComponentModel;
using Libra;

#endregion

namespace Noctua.Models
{
    public sealed class DefaultTerrainProcedure : IChunkProcedure
    {
        [DefaultValue(null)]
        public string Name { get; set; }

        public void Generate(Region region, Chunk chunk)
        {
            if (region == null) throw new ArgumentNullException("region");
            if (chunk == null) throw new ArgumentNullException("chunk");

            var chunkSize = chunk.Size;

            var blockCatalog = region.BlockCatalog;

            // バイオームを取得。
            // 選択されるブロックはバイオームに従う。
            var biome = region.BiomeManager.GetBiome(chunk);

            for (int x = 0; x < chunkSize.X; x++)
            {
                // チャンク空間における相対ブロック位置をブロック空間の位置へ変換。
                var absoluteX = chunk.GetAbsoluteBlockPositionX(x);

                for (int z = 0; z < chunkSize.Z; z++)
                {
                    // チャンク空間における相対ブロック位置をブロック空間の位置へ変換。
                    var absoluteZ = chunk.GetAbsoluteBlockPositionZ(z);

                    // この XZ  におけるバイオーム要素を取得。
                    var biomeElement = biome.GetBiomeElement(absoluteX, absoluteZ);

                    bool topBlockExists = false;
                    for (int y = chunkSize.Y - 1; 0 <= y; y--)
                    {
                        // チャンク空間における相対ブロック位置をブロック空間の位置へ変換。
                        var absoluteY = chunk.GetAbsoluteBlockPositionY(y);

                        // 地形密度を取得。
                        var density = biome.TerrainNoise.Sample(absoluteX, absoluteY, absoluteZ);

                        byte blockIndex = Block.EmptyIndex;
                        if (0 < density)
                        {
                            // 密度 1 はブロック有り

                            if (!topBlockExists)
                            {
                                // トップ ブロックを検出。
                                blockIndex = GetBlockIndexAtTop(blockCatalog, biomeElement);

                                topBlockExists = true;
                            }
                            else
                            {
                                blockIndex = GetBlockIndexBelowTop(blockCatalog, biomeElement);
                            }
                        }
                        else
                        {
                            // 密度 0 はブロック無し

                            // トップ ブロックを見つけていた場合はそれを OFF とする。
                            topBlockExists = false;
                        }

                        chunk.SetBlockIndex(x, y, z, blockIndex);
                    }
                }
            }
        }

        byte GetBlockIndexAtTop(BlockCatalog blockCatalog, BiomeElement biomeElement)
        {
            switch (biomeElement)
            {
                case BiomeElement.Desert:
                    return blockCatalog.SandIndex;
                case BiomeElement.Forest:
                    return blockCatalog.DirtIndex;
                case BiomeElement.Mountains:
                    return blockCatalog.StoneIndex;
                case BiomeElement.Plains:
                    return blockCatalog.GrassIndex;
                case BiomeElement.Snow:
                    return blockCatalog.SnowIndex;
            }

            throw new InvalidOperationException();
        }

        byte GetBlockIndexBelowTop(BlockCatalog blockCatalog, BiomeElement biomeElement)
        {
            switch (biomeElement)
            {
                case BiomeElement.Desert:
                    return blockCatalog.SandIndex;
                case BiomeElement.Forest:
                    return blockCatalog.DirtIndex;
                case BiomeElement.Mountains:
                    return blockCatalog.StoneIndex;
                case BiomeElement.Plains:
                    return blockCatalog.DirtIndex;
                case BiomeElement.Snow:
                    return blockCatalog.SnowIndex;
            }

            throw new InvalidOperationException();
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
