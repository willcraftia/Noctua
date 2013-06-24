#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Landscape;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class ChunkSettingsSerializer : AssetSerializer<ChunkSettings>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<ChunkSettingsDefinition>(stream);

            var settings = new ChunkSettings
            {
                ChunkSize = definition.ChunkSize,
                VertexBuildConcurrencyLevel = definition.VertexBuildConcurrencyLevel,
                UpdateBufferCountPerFrame = definition.UpdateBufferCountPerFrame,
                MinActiveRange = definition.MinActiveRange,
                MaxActiveRange = definition.MaxActiveRange,
            };

            settings.PartitionManager.PartitionSize = definition.ChunkSize.ToVector3();
            settings.PartitionManager.ActivationCapacity = definition.ActivationCapacity;
            settings.PartitionManager.PassivationCapacity = definition.PassivationCapacity;
            settings.PartitionManager.PassivationSearchCapacity = definition.PassivationSearchCapacity;
            settings.PartitionManager.PriorActiveDistance = definition.PriorActiveDistance;
            settings.PartitionManager.ClusterSize = definition.ClusterSize;
            settings.PartitionManager.MinActiveVolume = new DefaultActiveVolume { Radius = definition.MinActiveRange };
            settings.PartitionManager.MaxActiveVolume = new DefaultActiveVolume { Radius = definition.MaxActiveRange };

            return settings;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var settings = asset as ChunkSettings;

            var definition = new ChunkSettingsDefinition
            {
                ChunkSize = settings.ChunkSize,
                VertexBuildConcurrencyLevel = settings.VertexBuildConcurrencyLevel,
                UpdateBufferCountPerFrame = settings.UpdateBufferCountPerFrame,
                MinActiveRange = settings.MinActiveRange,
                MaxActiveRange = settings.MaxActiveRange,
                ChunkStoreType = settings.ChunkStoreType,

                ActivationCapacity = settings.PartitionManager.ActivationCapacity,
                PassivationCapacity = settings.PartitionManager.PassivationCapacity,
                PassivationSearchCapacity = settings.PartitionManager.PassivationSearchCapacity,
                PriorActiveDistance = settings.PartitionManager.PriorActiveDistance,
                ClusterSize = settings.PartitionManager.ClusterSize
            };

            WriteObject(stream, definition);
        }
    }
}
