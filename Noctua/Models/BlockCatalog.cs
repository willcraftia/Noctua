#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Noctua.Models
{
    public sealed class BlockCatalog : KeyedCollection<byte, Block>
    {
        public string Name { get; set; }

        public byte DirtIndex { get; set; }

        public byte GrassIndex { get; set; }

        public byte MantleIndex { get; set; }

        public byte SandIndex { get; set; }

        public byte SnowIndex { get; set; }

        public byte StoneIndex { get; set; }

        public Block Dirt { get { return this[DirtIndex]; } }

        public Block Grass { get { return this[GrassIndex]; } }

        public Block Mantle { get { return this[MantleIndex]; } }

        public Block Sand { get { return this[SandIndex]; } }

        public Block Snow { get { return this[SnowIndex]; } }

        public Block Stone { get { return this[StoneIndex]; } }

        protected override byte GetKeyForItem(Block item)
        {
            return item.Index;
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
