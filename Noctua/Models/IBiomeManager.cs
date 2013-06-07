#region Using

using System;

#endregion

namespace Noctua.Models
{
    public interface IBiomeManager
    {
        IBiome GetBiome(Chunk chunk);
    }
}
