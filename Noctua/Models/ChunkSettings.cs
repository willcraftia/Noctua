#region Using

using System;
using Libra;
using Noctua.Landscape;

#endregion

namespace Noctua.Models
{
    public sealed class ChunkSettings
    {
        IntVector3 chunkSize = new IntVector3(16);

        int vertexBuildConcurrencyLevel = 2;

        int updateBufferCountPerFrame = 32;

        int minActiveRange = 10;

        int maxActiveRange = 11;

        public IntVector3 ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value.X < 1 || value.Y < 1 || value.Z < 1 ||
                    value.X % ChunkMeshManager.MeshSize.X != 0 ||
                    value.Y % ChunkMeshManager.MeshSize.Y != 0 ||
                    value.Z % ChunkMeshManager.MeshSize.Z != 0)
                    throw new ArgumentOutOfRangeException("value");

                // 最大配置で ushort の限界を越えるようなサイズは拒否。
                var maxVertices = Chunk.CalculateMaxVertexCount(chunkSize);
                var maxIndices = Chunk.CalculateIndexCount(maxVertices);
                if (ushort.MaxValue < maxIndices)
                    throw new ArgumentException("The indices over the limit of ushort needed.", "value");

                chunkSize = value;
            }
        }

        public int VertexBuildConcurrencyLevel
        {
            get { return vertexBuildConcurrencyLevel; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                vertexBuildConcurrencyLevel = value;
            }
        }

        public int UpdateBufferCountPerFrame
        {
            get { return updateBufferCountPerFrame; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                updateBufferCountPerFrame = value;
            }
        }

        public ChunkStoreType ChunkStoreType { get; set; }

        public int MinActiveRange
        {
            get { return minActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                minActiveRange = value;
            }
        }

        public int MaxActiveRange
        {
            get { return maxActiveRange; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                maxActiveRange = value;
            }
        }

        public PartitionManager.Settings PartitionManager { get; private set; }

        public ChunkSettings()
        {
            PartitionManager = new PartitionManager.Settings();
        }
    }
}
