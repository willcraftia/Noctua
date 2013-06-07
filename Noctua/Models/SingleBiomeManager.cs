#region Using

using System;

#endregion

namespace Noctua.Models
{
    public sealed class SingleBiomeManager : IBiomeManager
    {
        public string Name { get; set; }

        public IBiome Biome { get; set; }

        public IBiome GetBiome(Chunk chunk)
        {
            return Biome;
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
