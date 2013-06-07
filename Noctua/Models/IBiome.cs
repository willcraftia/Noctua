#region Using

using System;
using Musca;

#endregion

namespace Noctua.Models
{
    public interface IBiome
    {
        byte Index { get; set; }

        INoiseSource TerrainNoise { get; set; }

        float GetTemperature(int x, int z);

        float GetHumidity(int x, int z);

        BiomeElement GetBiomeElement(int x, int z);
    }
}
