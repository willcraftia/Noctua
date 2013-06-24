#region Using

using System;
using System.ComponentModel;
using Musca;

#endregion

namespace Noctua.Models
{
    public sealed class SelectTerrainDensity : NamedObject, INoiseSource
    {
        INoiseSource source;

        [DefaultValue(null)]
        public INoiseSource Source
        {
            get { return source; }
            set { source = value; }
        }

        public float Sample(float x, float y, float z)
        {
            // Source が返す値がブロック空間上でのスケールに従い、
            // ハイトマップとしての値を返すことを前提としている。
            var height = source.Sample(x, y, z);

            // ハイトマップが示す高さ以下ならば密度 1 (ブロック有り)、
            // ハイトマップが示す高さより上ならば密度 0 (ブロック無し)。
            return (y <= height) ? 1 : 0;
        }
    }
}
