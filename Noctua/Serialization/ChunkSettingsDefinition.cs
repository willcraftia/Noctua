#region Using

using System;
using Libra;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class ChunkSettingsDefinition
    {
        public IntVector3 ChunkSize;

        public int VertexBuildConcurrencyLevel;

        public int UpdateBufferCountPerFrame;

        public int MinActiveRange;

        public int MaxActiveRange;

        public ChunkStoreType ChunkStoreType;

        public int ActivationCapacity;

        public int PassivationCapacity;

        public int PassivationSearchCapacity;

        public float PriorActiveDistance;

        public IntVector3 ClusterSize;
    }
}
