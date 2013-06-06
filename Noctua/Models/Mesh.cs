#region Using

using System;

#endregion

namespace Noctua.Models
{
    public sealed class Mesh
    {
        public string Name { get; set; }

        public SideCollection<MeshPart> MeshParts { get; private set; }

        public Mesh()
        {
            MeshParts = new SideCollection<MeshPart>();
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
