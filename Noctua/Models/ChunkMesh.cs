#region Using

using System;
using Libra;
using Libra.Graphics;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    public sealed class ChunkMesh : ShadowCaster, IDisposable
    {
        public Matrix World = Matrix.Identity;

        ChunkMeshManager meshManager;

        Chunk chunk;

        ChunkEffect chunkEffect;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        // 単体でのメッシュ構築以外に、
        // 隣接メッシュとの結合によるメッシュ更新も発生しうるため、
        // バッファは動的設定としておくべきである。

        VertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        public DeviceContext DeviceContext { get; private set; }

        public int VertexCount { get; private set; }

        public int IndexCount { get; private set; }

        public ChunkMesh(string name, ChunkMeshManager meshManager, Chunk chunk)
            : base(name)
        {
            if (meshManager == null) throw new ArgumentNullException("meshManager");
            if (chunk == null) throw new ArgumentNullException("chunk");

            this.meshManager = meshManager;
            this.chunk = chunk;

            DeviceContext = meshManager.DeviceContext;
            chunkEffect = meshManager.ChunkEffect;

            occlusionQuery = DeviceContext.Device.CreateOcclusionQuery();
            occlusionQuery.Initialize();
        }

        public override void UpdateOcclusion()
        {
            Occluded = false;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete) return;

                Occluded = (occlusionQuery.PixelCount == 0);
            }

            // 前回のクエリが完了しているならば、新たなクエリを試行。

            occlusionQuery.Begin(DeviceContext);

            // レンダ ターゲットへの書き込みは行わない。
            DeviceContext.BlendState = BlendState.ColorWriteDisable;
            
            chunkEffect.Mode = ChunkEffectMode.Occlusion;
            chunkEffect.World = World;
            chunkEffect.Apply();

            DrawCore();

            occlusionQuery.End();
            occlusionQueryActive = true;

            // デフォルトへ戻す。
            DeviceContext.BlendState = null;
        }

        public override void Draw()
        {
            if (!Translucent)
            {
                // 不透明とする。
                DeviceContext.BlendState = BlendState.Opaque;
            }
            else
            {
                // アルファ ブレンドを有効にする。
                DeviceContext.BlendState = BlendState.Additive;
            }

            chunkEffect.Mode = ChunkEffectMode.Default;
            chunkEffect.World = World;
            chunkEffect.Texture = chunk.Region.TileCatalog.TileMap;
            chunkEffect.Apply();

            DrawCore();

            // デフォルトへ戻す。
            DeviceContext.BlendState = null;
        }

        public override void Draw(IEffect effect)
        {
            var effectMatrices = effect as IEffectMatrices;
            if (effectMatrices != null)
                effectMatrices.World = World;

            // 指定のエフェクトで描画。
            effect.Apply();
            DrawCore();
        }

        public void SetVertices(ChunkVertex[] vertices, int vertexCount)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (vertexCount < 0 || vertices.Length < vertexCount) throw new ArgumentOutOfRangeException("vertexCount");

            VertexCount = vertexCount;

            if (vertexCount != 0)
            {
                if (vertexBuffer != null && vertexBuffer.VertexCount != vertexCount)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }

                if (vertexBuffer == null)
                {
                    vertexBuffer = DeviceContext.Device.CreateVertexBuffer();
                    vertexBuffer.Usage = ResourceUsage.Dynamic;
                    vertexBuffer.Initialize(ChunkVertex.VertexDeclaration, vertexCount);
                }

                DeviceContext.SetData(vertexBuffer, vertices, 0, vertexCount, SetDataOptions.Discard);
            }
            else
            {
                if (vertexBuffer != null)
                {
                    vertexBuffer.Dispose();
                    vertexBuffer = null;
                }
            }
        }

        public void SetIndices(ushort[] indices, int indexCount)
        {
            if (indices == null) throw new ArgumentNullException("indices");
            if (indexCount < 0 || indices.Length < indexCount) throw new ArgumentOutOfRangeException("vertexCount");

            IndexCount = indexCount;

            if (indexCount != 0)
            {
                if (indexBuffer != null && indexBuffer.IndexCount != indexCount)
                {
                    indexBuffer.Dispose();
                    indexBuffer = null;
                }

                if (indexBuffer == null)
                {
                    indexBuffer = DeviceContext.Device.CreateIndexBuffer();
                    indexBuffer.Usage = ResourceUsage.Dynamic;
                    indexBuffer.Initialize(indexCount);
                }

                DeviceContext.SetData(indexBuffer, indices, 0, indexCount, SetDataOptions.Discard);
            }
            else
            {
                if (indexBuffer != null)
                {
                    indexBuffer.Dispose();
                    indexBuffer = null;
                }
            }
        }

        void DrawCore()
        {
            DeviceContext.SetVertexBuffer(vertexBuffer);
            DeviceContext.IndexBuffer = indexBuffer;
            DeviceContext.PrimitiveTopology = PrimitiveTopology.TriangleList;
            DeviceContext.DrawIndexed(IndexCount);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~ChunkMesh()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (vertexBuffer != null) vertexBuffer.Dispose();
                if (indexBuffer != null) indexBuffer.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
