#region Using

using System;
using System.IO;
using Libra;

#endregion

namespace Noctua.Models
{
    public interface IChunkStore
    {
        bool GetChunk(IntVector3 position, ChunkData data);

        void AddChunk(IntVector3 position, ChunkData data);

        void DeleteChunk(IntVector3 position);

        void ClearChunks();

        //
        // ChunkBundle の反映は、エディタとゲームで共通。
        // ChunkBundle の取得は、エディタのため。
        //

        void GetChunkBundle(Stream chunkBundleStream);

        void AddChunkBundle(Stream chunkBundleStream);
    }
}
