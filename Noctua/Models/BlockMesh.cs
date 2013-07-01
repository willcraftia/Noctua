#region Using

using System;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    public sealed class BlockMesh
    {
        public SideCollection<MeshPart> MeshParts { get; private set; }

        BlockMesh()
        {
            MeshParts = new SideCollection<MeshPart>();
        }

        public static BlockMesh Create(Block block)
        {
            var mesh = new BlockMesh();

            for (int i = 0; i < Side.Count; i++)
            {
                var prototype = block.MeshPrototype.MeshParts[i];
                if (prototype == null)
                    continue;

                mesh.MeshParts[i] = Create(prototype, block.Tiles[i]);
            }

            return mesh;
        }

        static MeshPart Create(MeshPart prototype, Tile tile)
        {
            var newVertices = new VertexPositionNormalTexture[prototype.Vertices.Length];
            Array.Copy(prototype.Vertices, newVertices, newVertices.Length);

            for (int j = 0; j < newVertices.Length; j++)
            {
                if (tile == null)
                {
                    newVertices[j].TexCoord = Vector2.Zero;
                }
                else
                {
                    newVertices[j].TexCoord = tile.GetTexCoord(newVertices[j].TexCoord);
                }
            }

            // 全てのメッシュで共通であるため配列を共有。
            return new MeshPart(newVertices, prototype.Indices);
        }
    }
}
