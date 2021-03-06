﻿#region Using

using System;
using Libra;
using Noctua.Landscape;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    /// <summary>
    /// 一定領域に含まれるブロックを一纏めで管理するクラスです。
    /// </summary>
    public sealed class Chunk : Partition
    {
        /// <summary>
        /// ブロック インデックスに関するロック オブジェクト。
        /// </summary>
        object blockIndexLock = new object();

        /// <summary>
        /// チャンク マネージャ。
        /// </summary>
        ChunkManager chunkManager;

        /// <summary>
        /// メッシュ マネージャ。
        /// </summary>
        ChunkMeshManager meshManager;

        /// <summary>
        /// チャンクが属するリージョン。
        /// </summary>
        Region region;

        /// <summary>
        /// チャンク データ。
        /// </summary>
        /// <remarks>
        /// 初めて空ブロック以外が設定される際にインスタンスが生成されます。
        /// また、非空状態から空状態となった場合、フィールドを null に設定し、
        /// チャンク データは GC 対象となります。
        /// この仕組は、状況によっては大部分を占め得る、
        /// 空 (そら) を表すチャンクに対する大幅なメモリ節減を目的としています。
        /// </remarks>
        ChunkData data;

        /// <summary>
        /// 不透明メッシュ配列。
        /// 添字はセグメント位置です。
        /// </summary>
        ChunkMesh[, ,] opaqueMeshes;

        /// <summary>
        /// 半透明メッシュ。
        /// 添字はセグメント位置です。
        /// </summary>
        ChunkMesh[, ,] translucentMeshes;

        SideCollection<Chunk> neighbors = new SideCollection<Chunk>();

        /// <summary>
        /// チャンクのサイズを取得します。
        /// </summary>
        public IntVector3 Size
        {
            get { return chunkManager.ChunkSize; }
        }

        /// <summary>
        /// チャンクが属するリージョンを取得します。
        /// </summary>
        public Region Region
        {
            get { return region; }
        }

        /// <summary>
        /// チャンクのシーン ノードを取得します。
        /// </summary>
        public SceneNode Node { get; private set; }

        /// <summary>
        /// 非空ブロックの総数を取得します。
        /// </summary>
        public int SolidCount
        {
            get
            {
                if (data == null) return 0;
                return data.SolidCount;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="chunkManager">チャンク マネージャ。</param>
        /// <param name="position">チャンクの位置。</param>
        /// <param name="region">リージョン。</param>
        public Chunk(ChunkManager chunkManager, ChunkMeshManager meshManager, Region region, IntVector3 position)
        {
            if (chunkManager == null) throw new ArgumentNullException("manager");
            if (region == null) throw new ArgumentNullException("region");

            this.chunkManager = chunkManager;
            this.meshManager = meshManager;
            this.region = region;
            Position = position;

            opaqueMeshes = new ChunkMesh[meshManager.MeshSegments.X, meshManager.MeshSegments.Y, meshManager.MeshSegments.Z];
            translucentMeshes = new ChunkMesh[meshManager.MeshSegments.X, meshManager.MeshSegments.Y, meshManager.MeshSegments.Z];

            Node = chunkManager.CreateNode();
        }

        /// <summary>
        /// 指定のチャンク サイズで起こりうる最大の頂点数を計算します。
        /// </summary>
        /// <param name="chunkSize">チャンク サイズ。</param>
        /// <returns>最大頂点数。</returns>
        public static int CalculateMaxVertexCount(IntVector3 chunkSize)
        {
            // ブロックが交互に配置されるチャンクで頂点数が最大であると考える。
            //
            //      16 * 16 * 16 で考えた場合は以下の通り。
            //
            //          各 Y について 8 * 8 = 64 ブロック
            //          全 Y で 64 * 16 = 1024 ブロック
            //          ブロックは 4 * 6 = 24 頂点
            //          計 1024 * 24 = 24576 頂点
            //          インデックスは 4 頂点に対して 6
            //          計 (24576 / 4) * 6 = 36864 インデックス
            //
            //      16 * 256 * 16 で考えた場合は以下の通り。
            //
            //          各 Y について 8 * 8 = 64 ブロック
            //          全 Y で 64 * 256 = 16384 ブロック
            //          ブロックは 4 * 6 = 24 頂点
            //          計 16384 * 24 = 393216 頂点
            //          インデックスは 4 頂点に対して 6
            //          計 (393216 / 4) * 6 = 589824 インデックス

            int x = chunkSize.X / 2;
            int z = chunkSize.Z / 2;
            int xz = x * z;
            int blocks = xz * chunkSize.Y;
            const int perBlock = 4 * 6;
            return blocks * perBlock;
        }

        /// <summary>
        /// 指定の頂点数に対して必要となるインデックス数を計算します。
        /// </summary>
        /// <param name="vertexCount">頂点数。</param>
        /// <returns>インデックス数。</returns>
        public static int CalculateIndexCount(int vertexCount)
        {
            return (vertexCount / 4) * 6;
        }

        public Chunk GetNeighborChunk(Side side)
        {
            lock (neighbors)
            {
                return neighbors[side];
            }
        }

        public bool Contains(int x, int y, int z)
        {
            return 0 <= x && x < chunkManager.ChunkSize.X &&
                0 <= y && y < chunkManager.ChunkSize.Y &&
                0 <= z && z < chunkManager.ChunkSize.Z;
        }

        public bool Contains(IntVector3 position)
        {
            return Contains(position.X, position.Y, position.Z);
        }

        public Block GetBlock(int x, int y, int z)
        {
            var blockIndex = GetBlockIndex(x, y, z);
            if (blockIndex == Block.EmptyIndex) return null;

            return region.BlockCatalog[blockIndex];
        }

        public Block GetBlock(IntVector3 position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

        public byte GetBlockIndex(int x, int y, int z)
        {
            lock (blockIndexLock)
            {
                if (data == null) return Block.EmptyIndex;

                return data.GetBlockIndex(x, y, z);
            }
        }

        public byte GetBlockIndex(IntVector3 position)
        {
            return GetBlockIndex(position.X, position.Y, position.Z);
        }

        public void SetBlockIndex(int x, int y, int z, byte blockIndex)
        {
            lock (blockIndexLock)
            {
                if (data == null)
                {
                    // データが null で空ブロックを設定しようとする場合は、
                    // データに変更がないため、即座に処理を終えます。
                    if (blockIndex == Block.EmptyIndex) return;

                    // 非空ブロックを設定しようとする場合はデータを新規生成。
                    data = new ChunkData(chunkManager.ChunkSize);
                }

                // 同じインデックスならば更新しない。
                if (data.GetBlockIndex(x, y, z) == blockIndex) return;

                data.SetBlockIndex(x, y, z, blockIndex);

                if (data.SolidCount == 0)
                {
                    // 空になったならば GC へ。
                    // なお、頻繁に非空と空の状態を繰り返すことが稀であることを前提とし、
                    // これによる GC 負荷は少ないと判断する。
                    data = null;
                }
            }
        }

        public void SetBlockIndex(IntVector3 position, byte blockIndex)
        {
            SetBlockIndex(position.X, position.Y, position.Z, blockIndex);
        }

        public int GetRelativeBlockPositionX(int absoluteBlockPositionX)
        {
            return absoluteBlockPositionX - (Position.X * chunkManager.ChunkSize.X);
        }

        public int GetRelativeBlockPositionY(int absoluteBlockPositionY)
        {
            return absoluteBlockPositionY - (Position.Y * chunkManager.ChunkSize.Y);
        }

        public int GetRelativeBlockPositionZ(int absoluteBlockPositionZ)
        {
            return absoluteBlockPositionZ - (Position.Z * chunkManager.ChunkSize.Z);
        }

        public IntVector3 GetRelativeBlockPosition(IntVector3 absoluteBlockPosition)
        {
            return new IntVector3
            {
                X = GetRelativeBlockPositionX(absoluteBlockPosition.X),
                Y = GetRelativeBlockPositionY(absoluteBlockPosition.Y),
                Z = GetRelativeBlockPositionZ(absoluteBlockPosition.Z)
            };
        }

        public int GetAbsoluteBlockPositionX(int relativeBlockPositionX)
        {
            return Position.X * chunkManager.ChunkSize.X + relativeBlockPositionX;
        }

        public int GetAbsoluteBlockPositionY(int relativeBlockPositionY)
        {
            return Position.Y * chunkManager.ChunkSize.Y + relativeBlockPositionY;
        }

        public int GetAbsoluteBlockPositionZ(int relativeBlockPositionZ)
        {
            return Position.Z * chunkManager.ChunkSize.Z + relativeBlockPositionZ;
        }

        public IntVector3 GetAbsoluteBlockPosition(IntVector3 relativeBlockPosition)
        {
            return new IntVector3
            {
                X = GetAbsoluteBlockPositionX(relativeBlockPosition.X),
                Y = GetAbsoluteBlockPositionY(relativeBlockPosition.Y),
                Z = GetAbsoluteBlockPositionZ(relativeBlockPosition.Z)
            };
        }

        public ChunkMesh GetMesh(int segmentX, int segmentY, int segmentZ, bool translucence)
        {
            if (translucence)
            {
                return translucentMeshes[segmentX, segmentY, segmentZ];
            }
            else
            {
                return opaqueMeshes[segmentX, segmentY, segmentZ];
            }
        }

        public void SetMesh(int segmentX, int segmentY, int segmentZ, bool translucence, ChunkMesh mesh)
        {
            ChunkMesh oldMesh;
            if (translucence)
            {
                oldMesh = translucentMeshes[segmentX, segmentY, segmentZ];
                translucentMeshes[segmentX, segmentY, segmentZ] = mesh;
            }
            else
            {
                oldMesh = opaqueMeshes[segmentX, segmentY, segmentZ];
                opaqueMeshes[segmentX, segmentY, segmentZ] = mesh;
            }

            if (oldMesh != null)
                DetachMesh(oldMesh);

            if (mesh != null)
                AttachMesh(mesh);
        }

        /// <summary>
        /// リージョンが提供するチャンク ストアに永続化されている場合、
        /// チャンク ストアからチャンクをロードします。
        /// リージョンが提供するチャンク ストアに永続化されていない場合、
        /// リージョンが提供するチャンク プロシージャから自動生成します。
        /// </summary>
        protected internal override void Activate()
        {
            var newData = new ChunkData(chunkManager.ChunkSize);

            if (chunkManager.ChunkStore.GetChunk(region.ChunkStoreKey, Position, newData))
            {
                if (newData.SolidCount == 0)
                {
                    // 全てが空ブロックならば GC 回収。
                }
                else
                {
                    data = newData;
                }
            }
            else
            {
                // 永続化されていないならば自動生成。
                for (int i = 0; i < region.ChunkProcesures.Count; i++)
                    region.ChunkProcesures[i].Generate(region, this);
            }

            base.Activate();
        }

        protected internal override void OnPassivating()
        {
            // ノード グラフからチャンク ノードを削除。
            if (Node.Parent != null)
                Node.Parent.Children.Remove(Node);

            // メッシュ開放はゲーム スレッドで非アクティブ化に先駆けて行う。
            // 非アクティブ化で行う処理は永続化のみ。
            DetachAllMeshes();

            base.OnPassivating();
        }

        /// <summary>
        /// チャンクをチャンク ストアへ永続化します。
        /// </summary>
        protected internal override void Passivate()
        {
            lock (blockIndexLock)
            {
                if (data != null)
                {
                    chunkManager.ChunkStore.AddChunk(region.ChunkStoreKey, Position, data);
                }
                else
                {
                    // 空データで永続化。
                    chunkManager.ChunkStore.AddChunk(region.ChunkStoreKey, Position, chunkManager.EmptyData);
                }
            }

            base.Passivate();
        }

        protected internal override void OnNeighborActivated(Partition neighbor, Side side)
        {
            lock (neighbors)
            {
                neighbors[side] = neighbor as Chunk;

                bool allNeighborsExist = true;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    if (neighbors[i] == null)
                    {
                        allNeighborsExist = false;
                        break;
                    }
                }

                if (allNeighborsExist)
                {
                    // 隣接チャンクが揃ってからメッシュ構築を要求するが、
                    // 実際にはメッシュ構築の最中に隣接チャンクが非アクティブ化されることもある。
                    chunkManager.RequestUpdateMesh(this, ChunkMeshUpdatePriority.Normal);
                }
            }

            base.OnNeighborActivated(neighbor, side);
        }

        protected internal override void OnNeighborPassivated(Partition neighbor, Side side)
        {
            lock (neighbors)
            {
                neighbors[side] = null;
            }

            base.OnNeighborPassivated(neighbor, side);
        }

        /// <summary>
        /// 全てのメッシュを開放します。
        /// </summary>
        void DetachAllMeshes()
        {
            for (int z = 0; z < meshManager.MeshSegments.Z; z++)
            {
                for (int y = 0; y < meshManager.MeshSegments.Y; y++)
                {
                    for (int x = 0; x < meshManager.MeshSegments.X; x++)
                    {
                        if (opaqueMeshes[x, y, z] != null)
                        {
                            DetachMesh(opaqueMeshes[x, y, z]);
                            opaqueMeshes[x, y, z] = null;
                        }
                        if (translucentMeshes[x, y, z] != null)
                        {
                            DetachMesh(translucentMeshes[x, y, z]);
                            translucentMeshes[x, y, z] = null;
                        }
                    }
                }
            }
        }

        void AttachMesh(ChunkMesh mesh)
        {
            // ノードへ追加。
            Node.Objects.Add(mesh);
        }

        void DetachMesh(ChunkMesh mesh)
        {
            // ノードから削除。
            Node.Objects.Remove(mesh);

            // メッシュ マネージャへ削除要求。
            meshManager.RemoveMesh(mesh);
        }
    }
}
