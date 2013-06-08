#region Using

using System;

#endregion

namespace Noctua.Models
{
    public interface IChunkProcedure
    {
        void Generate(Region region, Chunk chunk);
    }
}
