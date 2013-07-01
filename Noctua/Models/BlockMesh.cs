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

        // TODO
        //
        // タイルによるテクスチャ座標の調整を行わないため、
        // もはやプロトタイプからインスタンスを生成する必要が無いのでは？

        public static BlockMesh Create(Block block)
        {
            var mesh = new BlockMesh();

            for (int i = 0; i < Side.Count; i++)
            {
                var prototype = block.MeshPrototype.MeshParts[i];
                if (prototype == null)
                    continue;

                mesh.MeshParts[i] = Create(prototype);
            }

            return mesh;
        }

        static MeshPart Create(MeshPart prototype)
        {
            // 全てのメッシュで共通であるため配列を共有。
            return new MeshPart(prototype.Vertices, prototype.Indices);
        }
    }
}
