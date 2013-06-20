#region Using

using System;
using Libra;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public struct ChunkSettingsDefinition
    {
        public IntVector3 ChunkSize;

        public int VertexBuildConcurrencyLevel;

        public int UpdateBufferCountPerFrame;

        public int MinActiveRange;

        public int MaxActiveRange;

        public IntVector3 ClusterSize;

        public int ActivationCapacity;

        public int PassivationCapacity;

        public int PassivationSearchCapacity;

        public float PriorActiveDistance;

        public ChunkStoreType ChunkStoreType;
    }
}
