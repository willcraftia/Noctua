#region Using

using System;

#endregion

namespace Noctua.Asset
{
    public sealed class Mesh
    {
        public string Name { get; set; }

        public SideCollection<MeshPart> MeshParts { get; private set; }

        public Mesh()
        {
            MeshParts = new SideCollection<MeshPart>();
        }
    }
}
