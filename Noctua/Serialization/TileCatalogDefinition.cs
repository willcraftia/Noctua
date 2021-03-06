﻿#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Noctua.Serialization
{
    [XmlRoot("TileCatalog")]
    public struct TileCatalogDefinition
    {
        public string Name;

        [XmlArrayItem("Entry")]
        public IndexedUriDefinition[] Entries;
    }
}
