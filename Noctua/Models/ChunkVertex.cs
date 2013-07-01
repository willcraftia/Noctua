#region Using

using System;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    public struct ChunkVertex : IVertexType, IEquatable<ChunkVertex>
    {
        // メモ
        //
        // TileIndex を short で定義し、シェーダから int で受け取ろうとしたが、
        // これは成功しなかった。
        // 恐らく、VertexElement でサイズを指定できるようにし、
        // VertexDeclaration では short ではなく int のサイズで頂点サイズを算出すれば、
        // short 指定でも問題は発生しないと思われるが、面倒であるため int 指定とする。
        // ただし、その分だけ頂点サイズが増加する。

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            VertexElement.SVPosition,
            VertexElement.Normal,
            VertexElement.Color,
            VertexElement.TexCoord,
            new VertexElement("TILE_INDEX", 0, VertexFormat.Int));

        public Vector3 Position;

        public Vector3 Normal;

        public Color Color;

        public Vector2 TexCoord;

        public int TileIndex;

        public ChunkVertex(Vector3 position, Vector3 normal, Color color, Vector2 texCoord, int tileIndex)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoord = texCoord;
            TileIndex = tileIndex;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        #region Equatable

        public static bool operator ==(ChunkVertex value1, ChunkVertex value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(ChunkVertex value1, ChunkVertex value2)
        {
            return !value1.Equals(value2);
        }

        public bool Equals(ChunkVertex other)
        {
            return Position == other.Position && Normal == other.Normal &&
                Color == other.Color && TexCoord == other.TexCoord &&
                TileIndex == other.TileIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((ChunkVertex) obj);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Normal.GetHashCode() ^
                Color.GetHashCode() ^ TexCoord.GetHashCode() ^
                TileIndex.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return "{Position:" + Position + " Normal:" + Normal +
                " Color:" + Color + " TexCoord:" + TexCoord +
                " TileIndex:" + TileIndex + "}";
        }

        #endregion
    }
}
