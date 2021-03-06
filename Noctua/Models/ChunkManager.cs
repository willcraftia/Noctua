﻿#region Using

using System;
using Libra;
using Libra.Graphics;
using Noctua.Landscape;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    /// <summary>
    /// チャンクをパーティションとして管理するパーティション マネージャの実装です。
    /// </summary>
    public sealed class ChunkManager : PartitionManager
    {
        /// <summary>
        /// メッシュ サイズ。
        /// </summary>
        public static readonly IntVector3 MeshSize = new IntVector3(16);

        /// <summary>
        /// チャンク サイズ。
        /// </summary>
        /// <remarks>
        /// チャンク サイズは、メッシュ サイズの等倍でなければなりません。
        /// </remarks>
        public readonly IntVector3 ChunkSize;

        /// <summary>
        /// チャンクのノード名の自動決定で利用する連番の記録。
        /// </summary>
        int nodeIdSequence;

        /// <summary>
        /// メッシュ マネージャ。
        /// </summary>
        ChunkMeshManager meshManager;

        /// <summary>
        /// デバイス コンテキストを取得します。
        /// </summary>
        public DeviceContext DeviceContext { get; private set; }

        /// <summary>
        /// リージョン マネージャを取得します。
        /// </summary>
        public RegionManager RegionManager { get; private set; }

        /// <summary>
        /// ワールド マネージャを取得します。
        /// </summary>
        public WorldManager WorldManager { get; private set; }

        /// <summary>
        /// シーン マネージャを取得します。
        /// </summary>
        public SceneManager SceneManager { get; private set; }

        /// <summary>
        /// チャンク ノードを関連付けるためのノード。
        /// </summary>
        public SceneNode BaseNode { get; private set; }

        /// <summary>
        /// チャンク メッシュの数を取得します。
        /// </summary>
        public int MeshCount { get; private set; }

        /// <summary>
        /// 頂点ビルダの総数を取得します。
        /// </summary>
        public int TotalVertexBuilderCount
        {
            get { return meshManager.TotalVertexBuilderCount; }
        }

        /// <summary>
        /// 未使用の頂点ビルダの数を取得します。
        /// </summary>
        public int FreeVertexBuilderCount
        {
            get { return meshManager.FreeVertexBuilderCount; }
        }

        /// <summary>
        /// 使用中の頂点ビルダの数を取得します。
        /// </summary>
        public int ActiveVertexBuilderCount
        {
            get { return TotalVertexBuilderCount - FreeVertexBuilderCount; }
        }

        ///// <summary>
        ///// 頂点の総数を取得します。
        ///// </summary>
        public int TotalVertexCount
        {
            get { return meshManager.TotalVertexCount; }
        }

        /// <summary>
        /// インデックスの総数を取得します。
        /// </summary>
        public int TotalIndexCount
        {
            get { return meshManager.TotalIndexCount; }
        }

        /// <summary>
        /// 処理全体を通じて最も大きな頂点バッファのサイズを取得します。
        /// </summary>
        public int MaxVertexCount
        {
            get { return meshManager.MaxVertexCount; }
        }

        /// <summary>
        /// 処理全体を通じて最も大きなインデックス バッファのサイズを取得します。
        /// </summary>
        public int MaxIndexCount
        {
            get { return meshManager.MaxIndexCount; }
        }

        public IChunkStore ChunkStore { get; private set; }

        /// <summary>
        /// 空データを取得します。
        /// </summary>
        internal ChunkData EmptyData { get; private set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="settings">チャンク設定。</param>
        /// <param name="regionManager">リージョン マネージャ。</param>
        /// <param name="sceneManager">シーン マネージャ。</param>
        public ChunkManager(
            ChunkSettings settings,
            RegionManager regionManager,
            SceneManager sceneManager)
            : base(settings.PartitionManager)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");

            ChunkSize = settings.ChunkSize;
            RegionManager = regionManager;
            WorldManager = regionManager.WorldManager;
            SceneManager = sceneManager;
            DeviceContext = sceneManager.DeviceContext;

            switch (settings.ChunkStoreType)
            {
                //case ChunkStoreType.Storage:
                //    ChunkStore = StorageChunkStore.Instance;
                //    break;
                default:
                    ChunkStore = NullChunkStore.Instance;
                    break;
            }

            EmptyData = new ChunkData(ChunkSize);

            BaseNode = sceneManager.CreateSceneNode("ChunkRoot");
            sceneManager.RootNode.Children.Add(BaseNode);

            meshManager = new ChunkMeshManager(this, settings.VertexBuildConcurrencyLevel, settings.UpdateBufferCountPerFrame);
        }

        public IntVector3 GetChunkPositionByBlockPosition(IntVector3 blockPosition)
        {
            return new IntVector3
            {
                X = (int) Math.Floor(blockPosition.X / (double) ChunkSize.X),
                Y = (int) Math.Floor(blockPosition.Y / (double) ChunkSize.Y),
                Z = (int) Math.Floor(blockPosition.Z / (double) ChunkSize.Z),
            };
        }

        public Chunk GetChunkByBlockPosition(IntVector3 blockPosition)
        {
            var chunkPosition = GetChunkPositionByBlockPosition(blockPosition);

            return GetChunk(chunkPosition);
        }

        public Chunk GetChunk(IntVector3 chunkPosition)
        {
            return GetPartition(chunkPosition) as Chunk;
        }

        public void RequestUpdateMesh(Chunk chunk, ChunkMeshUpdatePriority priority)
        {
            meshManager.RequestUpdateMesh(chunk, priority);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・指定の位置を含むリージョンがある場合、アクティブ化可能であると判定。
        /// 
        /// </summary>
        protected override bool CanActivate(IntVector3 position)
        {
            if (RegionManager.GetRegionByChunkPosition(position) == null) return false;

            return base.CanActivate(position);
        }

        /// <summary>
        /// 指定の位置にあるチャンクのインスタンスを生成します。
        /// </summary>
        protected override Partition Create(IntVector3 position)
        {
            // 対象リージョンの取得。
            var region = RegionManager.GetRegionByChunkPosition(position);
            if (region == null)
                throw new InvalidOperationException(string.Format("No region exists for a chunk '{0}'.", position));

            return new Chunk(this, meshManager, region, position);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        /// 
        /// ・新たなメッシュ更新の開始。
        /// ・チャンク タスクの実行。
        /// ・メッシュ更新完了の監視。
        /// 
        /// </summary>
        protected override void UpdateOverride(Matrix view, Matrix projection)
        {
            // 更新が必要なチャンクを探索して更新要求を追加。
            // ただし、クローズが開始したら行わない。
            //if (!Closing)
            //{
            //}

            // メッシュ マネージャ更新。
            // メッシュ構築に関する更新の他に、
            // メッシュが使用するカメラ設定の更新も行われる。
            meshManager.Update(view, projection);

            base.UpdateOverride(view, projection);
        }

        /// <summary>
        /// このクラスの実装では、以下の処理を行います。
        ///
        /// ・チャンクのメッシュ更新を要求。
        /// ・チャンクのノードをノード グラフへ追加。
        /// 
        /// </summary>
        /// <param name="partition"></param>
        protected override void OnActivated(Partition partition)
        {
            var chunk = partition as Chunk;

            // ノードを追加。
            BaseNode.Children.Add(chunk.Node);

            base.OnActivated(partition);
        }

        internal SceneNode CreateNode()
        {
            nodeIdSequence++;
            return new SceneNode(SceneManager, "Chunk" + nodeIdSequence);
        }
    }
}
