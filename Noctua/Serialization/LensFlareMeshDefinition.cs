#region Using

using System;
using System.Xml.Serialization;

#endregion

namespace Noctua.Serialization
{
    [XmlRoot("LensFlareMesh")]
    public sealed class LensFlareMeshDefinition
    {
        public string Name;

        public float QuerySize;

        [XmlArrayItem("Flare")]
        public LensFlareElementDefinition[] Flares;

        public string GlowTexture;

        public float GlowSize;

        public string LightName;
    }
}
