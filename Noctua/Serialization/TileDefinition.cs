#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Noctua.Serialization
{
    [XmlRoot("Tile")]
    public struct TileDefinition
    {
        //----------------------------
        // Editor/Debug

        public string Name;

        //----------------------------
        // Texture

        public string Texture;

        public bool Translucent;
    }
}
