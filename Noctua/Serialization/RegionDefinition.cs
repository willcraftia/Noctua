#region Using

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Libra;

#endregion

namespace Noctua.Serialization
{
    [XmlRoot("Region")]
    public struct RegionDefinition
    {
        [DefaultValue(null)]
        public string Name;

        public IntBoundingBox Box;

        // URI
        [DefaultValue(null)]
        public string TileCatalog;

        // URI
        [DefaultValue(null)]
        public string BlockCatalog;

        [DefaultValue(null)]
        public string BiomeManager;

        // URI
        [DefaultValue(null)]
        public string ChunkBundle;

        // URI
        [XmlArrayItem("Procedure")]
        [DefaultValue(null)]
        public string[] ChunkProcedures;
    }
}
