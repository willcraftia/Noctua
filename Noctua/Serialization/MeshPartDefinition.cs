#region Using

using System;
using System.Xml.Serialization;
using Libra.Graphics;

#endregion

namespace Noctua.Serialization
{
    public struct MeshPartDefinition
    {
        [XmlArrayItem("Vertex")]
        public VertexPositionNormalTexture[] Vertices;

        [XmlArrayItem("Index")]
        public ushort[] Indices;
    }
}
