#region Using

using System;
using System.Collections.ObjectModel;

#endregion

namespace Noctua.Models
{
    public sealed class BiomeCatalog : KeyedCollection<byte, IBiome>
    {
        public string Name { get; set; }

        public BiomeCatalog()
        {
        }

        protected override byte GetKeyForItem(IBiome item)
        {
            return item.Index;
        }

        #region ToString

        public override string ToString()
        {
            return "{Name" + Name + "}";
        }

        #endregion
    }
}
