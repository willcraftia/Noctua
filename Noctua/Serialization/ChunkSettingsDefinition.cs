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

        public IntVector3 ClusterSize;

        public int MinActiveRange;

        public int MaxActiveRange;

        public float PriorActiveDistance;

        public int VertexBuildConcurrencyLevel;

        public int UpdateBufferCountPerFrame;

        public ChunkStoreType ChunkStoreType;

        public int ActivationCapacity;

        public int PassivationCapacity;

        public int PassivationSearchCapacity;
    }
}
