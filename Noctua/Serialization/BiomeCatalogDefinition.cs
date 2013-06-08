#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Noctua.Serialization
{
    [XmlRoot("BiomeCatalog")]
    public struct BiomeCatalogDefinition
    {
        public string Name;

        [XmlArrayItem("Entry")]
        public IndexedUriDefinition[] Entries;
    }
}
