#region Using

using System;
using Libra.Graphics;

#endregion

namespace Noctua.Asset
{
    public sealed class MeshPart
    {
        public VertexPositionNormalTexture[] Vertices { get; private set; }

        public ushort[] Indices { get; private set; }

        public MeshPart(VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (indices == null) throw new ArgumentNullException("indices");

            Vertices = vertices;
            Indices = indices;
        }
    }
}
