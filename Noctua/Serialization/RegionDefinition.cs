#region Using

using System;
using System.Xml.Serialization;
using Libra;

#endregion

namespace Noctua.Serialization
{
    [XmlRoot("Region")]
    public struct RegionDefinition
    {
        public string Name;

        public IntBoundingBox Box;

        public string TileCatalog;

        public string BlockCatalog;

        public string BiomeManager;

        public string ChunkBundle;

        // URI
        [XmlArrayItem("Procedure")]
        public string[] ChunkProcedures;
    }
}
