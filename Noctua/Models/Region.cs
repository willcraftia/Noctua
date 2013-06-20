#region Using

using System;
using System.Collections.Generic;
using Libra;
using Noctua.Asset;

#endregion

namespace Noctua.Models
{
    public sealed class Region : IDisposable
    {
        public AssetContainer AssetContainer { get; private set; }

        /// <summary>
        /// このリージョンに属するチャンクのためのエフェクトを取得します。
        /// タイル カタログはリージョン毎に定義され、
        /// チャンクは自身が属するリージョンのタイル カタログを参照する必要があります。
        /// このため、各リージョンは、自身に属するチャンクのための固有のチャンク エフェクトを管理します。
        /// </summary>
        public ChunkEffect ChunkEffect { get; private set; }

        public string Name { get; set; }

        public IntBoundingBox Box;

        public TileCatalog TileCatalog { get; set; }

        public BlockCatalog BlockCatalog { get; set; }

        public IBiomeManager BiomeManager { get; set; }

        public List<IChunkProcedure> ChunkProcesures { get; set; }

        public string ChunkStoreKey { get; private set; }

        public void Initialize(AssetContainer assetContainer)
        {
            if (assetContainer == null) throw new ArgumentNullException("assetManager");
            //if (chunkEffect == null) throw new ArgumentNullException("chunkEffect");

            AssetContainer = assetContainer;

            // リージョン毎にタイル カタログが異なるため、エフェクトを複製して利用。
            //ChunkEffect = new ChunkEffect(chunkEffect);

            // タイル カタログのテクスチャをチャンク エフェクトへ設定。
            //ChunkEffect.TileMap = TileCatalog.TileMap;
            //ChunkEffect.EmissiveMap = TileCatalog.EmissiveColorMap;
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Region()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                //ChunkEffect.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
