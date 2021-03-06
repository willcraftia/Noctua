﻿#region Using

using System;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    /// <summary>
    /// チャンク メッシュの頂およびインデックス情報を管理するクラスです。
    /// このクラスで管理する頂点は VertexPositionNormalColorTexture、インデックスは ushort です。
    /// </summary>
    public sealed class ChunkVertices
    {
        /// <summary>
        /// 頂点情報。
        /// </summary>
        ChunkVertex[] vertices;

        /// <summary>
        /// インデックス情報。
        /// </summary>
        ushort[] indices;

        /// <summary>
        /// 全頂点を内包する BoundingBox。
        /// </summary>
        BoundingBox box;

        /// <summary>
        /// BoundingBox のコーナを取得するために用いるバッファ。
        /// </summary>
        Vector3[] corners = new Vector3[8];

        /// <summary>
        /// 設定可能な頂点数を取得します。
        /// </summary>
        public int VertexCapacity { get; private set; }

        /// <summary>
        /// 設定可能なインデックス数を取得します。
        /// </summary>
        public int IndexCapacity { get; private set; }

        /// <summary>
        /// 頂点数を取得します。
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// インデックス数を取得します。
        /// </summary>
        public int IndexCount { get; private set; }

        /// <summary>
        /// バッファへ反映したか否かを示す値を取得します。
        /// </summary>
        /// <value>
        /// true (バッファへ反映した場合)、false (それ以外の場合)。
        /// </value>
        public bool Consumed { get; set; }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        public ChunkVertices()
        {
            VertexCapacity = Chunk.CalculateMaxVertexCount(ChunkMeshManager.MeshSize);
            IndexCapacity = Chunk.CalculateIndexCount(VertexCapacity);

            if (ushort.MaxValue < IndexCapacity)
                throw new ArgumentException("The indices over the limit of ushort needed.", "chunkSize");

            vertices = new ChunkVertex[VertexCapacity];
            indices = new ushort[IndexCapacity];
        }

        /// <summary>
        /// チャンク メッシュへ頂点およびインデックス情報を設定します。
        /// </summary>
        /// <param name="mesh">チャンク メッシュ。</param>
        public void Populate(ChunkMesh mesh)
        {
            if (mesh == null) throw new ArgumentNullException("mesh");

            // メッシュの BoundingBox。
            var transform = mesh.World;
            Vector3.Transform(ref box.Min, ref transform, out mesh.Box.Min);
            Vector3.Transform(ref box.Max, ref transform, out mesh.Box.Max);

            // メッシュの BoundingSphere。
            mesh.Box.GetCorners(corners);
            mesh.Sphere = BoundingSphere.CreateFromPoints(corners);

            mesh.SetVertices(vertices, VertexCount);
            mesh.SetIndices(indices, IndexCount);
        }

        /// <summary>
        /// インデックスを追加します。
        /// インデックスの追加の前には、必ず対応する頂点を追加していなければなりません。
        /// </summary>
        /// <param name="index">インデックス。</param>
        public void AddIndex(ushort index)
        {
            indices[IndexCount++] = (ushort) (VertexCount + index);
        }

        /// <summary>
        /// 頂点を追加します。
        /// </summary>
        /// <param name="vertex">頂点。</param>
        public void AddVertex(ref ChunkVertex vertex)
        {
            vertices[VertexCount++] = vertex;

            // AABB を更新。
            Vector3.Max(ref box.Max, ref vertex.Position, out box.Max);
            Vector3.Min(ref box.Min, ref vertex.Position, out box.Min);
        }

        /// <summary>
        /// 頂点とインデックスの情報を初期化します。
        /// </summary>
        public void Clear()
        {
            VertexCount = 0;
            IndexCount = 0;
            box = BoundingBox.Empty;
            Consumed = false;
        }
    }
}
