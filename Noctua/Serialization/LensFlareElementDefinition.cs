#region Using

using System;
using Libra;

#endregion

namespace Noctua.Serialization
{
    public sealed class LensFlareElementDefinition
    {
        public float Position;

        public float Scale;

        public Vector3 Color;

        public string Texture;

        public LensFlareElementDefinition() { }

        public LensFlareElementDefinition(float position, float scale, Vector3 color, string texture)
        {
            Position = position;
            Scale = scale;
            Color = color;
            Texture = texture;
        }
    }
}
