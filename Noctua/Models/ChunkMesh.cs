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
        static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None,
            Name = "ChunkMesh.ColorWriteDisable"
        };

        static readonly DepthStencilState DepthWriteWithLessEqual = new DepthStencilState
        {
            DepthFunction = ComparisonFunction.LessEqual,
            Name = "ChunkMesh.DepthWriteWithLessEqual"
        };

        static readonly DepthStencilState DepthReadWithLessEquals = new DepthStencilState
        {
            DepthWriteEnable = false,
            DepthFunction = ComparisonFunction.LessEqual,
            Name = "ChunkMesh.DepthReadWithLessEquals"
        };

        public Matrix World = Matrix.Identity;

        ChunkMeshManager meshManager;

        Chunk chunk;

        DeviceContext deviceContext;

        ChunkEffect chunkEffect;

        OcclusionQuery occlusionQuery;

        bool occlusionQueryActive;

        // 単体でのメッシュ構築以外に、
        // 隣接メッシュとの結合によるメッシュ更新も発生しうるため、
        // バッファは動的設定としておくべきである。

        VertexBuffer vertexBuffer;

        IndexBuffer indexBuffer;

        public int VertexCount { get; private set; }

        public int IndexCount { get; private set; }

        public ChunkMesh(string name, ChunkMeshManager meshManager, Chunk chunk)
            : base(name)
        {
            if (meshManager == null) throw new ArgumentNullException("meshManager");
            if (chunk == null) throw new ArgumentNullException("chunk");

            this.meshManager = meshManager;
            this.chunk = chunk;

            deviceContext = meshManager.DeviceContext;
            chunkEffect = meshManager.ChunkEffect;

            occlusionQuery = deviceContext.Device.CreateOcclusionQuery();
            occlusionQuery.Initialize();
        }

        bool depthWritten;

        public override void UpdateOcclusion()
        {
            depthWritten = false;
            Occluded = false;

            if (occlusionQueryActive)
            {
                if (!occlusionQuery.IsComplete) return;

                Occluded = (occlusionQuery.PixelCount == 0);
            }

            // 前回のクエリが完了しているならば、新たなクエリを試行。

            occlusionQuery.Begin(deviceContext);

            if (!Translucent)
            {
                // 不透明ならば深度を書き込む。
                deviceContext.DepthStencilState = DepthWriteWithLessEqual;
                depthWritten = true;
            }
            else
            {
                // 半透明ならば深度を読み取るのみとする。
                deviceContext.DepthStencilState = DepthStencilState.DepthRead;
            }

            // レンダ ターゲットへの書き込みは行わない。
            deviceContext.BlendState = ColorWriteDisable;
            
            chunkEffect.Mode = ChunkEffectMode.Occlusion;
            chunkEffect.World = World;
            chunkEffect.Apply(deviceContext);

            DrawCore();

            occlusionQuery.End();
            occlusionQueryActive = true;

            // デフォルトへ戻す。
            deviceContext.DepthStencilState = null;
            deviceContext.BlendState = null;
        }

        public override void Draw()
        {
            if (Occluded) return;

            if (!Translucent)
            {
                if (depthWritten)
                {
                    // オクルージョン クエリで深度書き込み済みならば読み取りのみとする。
                    deviceContext.DepthStencilState = DepthReadWithLessEquals;
                }
                else
                {
                    // さもなくば書き込む。
                    deviceContext.DepthStencilState = DepthWriteWithLessEqual;
                }

                // 不透明とする。
                deviceContext.BlendState = BlendState.Opaque;
            }
            else
            {
                // 半透明ならば深度を読み取るのみとする。
                deviceContext.DepthStencilState = DepthStencilState.DepthRead;

                // アルファ ブレンドを有効にする。
                deviceContext.BlendState = BlendState.Additive;
            }

            chunkEffect.Mode = ChunkEffectMode.Default;
            chunkEffect.World = World;
            chunkEffect.Texture = chunk.Region.TileCatalog.TileMap;
            chunkEffect.Apply(deviceContext);

            DrawCore();

            // デフォルトへ戻す。
            deviceContext.DepthStencilState = null;
            deviceContext.BlendState = null;
        }

        public override void Draw(IEffect effect)
        {
            if (Occluded) return;

            var effectMatrices = effect as IEffectMatrices;
            if (effectMatrices != null)
                effectMatrices.World = World;

            // 指定のエフェクトで描画。
            effect.Apply(deviceContext);
            DrawCore();
        }

        public void SetVertices(VertexPositionNormalColorTexture[] vertices, int vertexCount)
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
                    vertexBuffer = deviceContext.Device.CreateVertexBuffer();
                    vertexBuffer.Usage = ResourceUsage.Dynamic;
                    vertexBuffer.Initialize(VertexPositionNormalColorTexture.VertexDeclaration, vertexCount);
                }

                vertexBuffer.SetData(deviceContext, vertices, 0, vertexCount, SetDataOptions.Discard);
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
                    indexBuffer = deviceContext.Device.CreateIndexBuffer();
                    indexBuffer.Usage = ResourceUsage.Dynamic;
                    indexBuffer.Initialize(indexCount);
                }

                indexBuffer.SetData(deviceContext, indices, 0, indexCount, SetDataOptions.Discard);
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
            deviceContext.SetVertexBuffer(vertexBuffer);
            deviceContext.IndexBuffer = indexBuffer;
            deviceContext.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.DrawIndexed(IndexCount);
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
