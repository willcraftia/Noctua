#region Using

using System;
using System.ComponentModel;
using Libra;
using Musca;
using Pyxis;

#endregion

namespace Noctua.Models
{
    public sealed class DefaultBiome
    {
        #region Range

        public sealed class Range
        {
            float minTemperature;

            float maxTemperature;

            float minHumidity;

            float maxHumidity;

            [DefaultValue(0.0f)]
            public float MinTemperature
            {
                get { return minTemperature; }
                set { minTemperature = MathHelper.Clamp(value, 0, 1); }
            }

            [DefaultValue(0.0f)]
            public float MaxTemperature
            {
                get { return maxTemperature; }
                set { maxTemperature = MathHelper.Clamp(value, 0, 1); }
            }

            [DefaultValue(0.0f)]
            public float MinHumidity
            {
                get { return minHumidity; }
                set { minHumidity = MathHelper.Clamp(value, 0, 1); }
            }

            [DefaultValue(0.0f)]
            public float MaxHumidity
            {
                get { return maxHumidity; }
                set { maxHumidity = MathHelper.Clamp(value, 0, 1); }
            }

            public bool Contains(float temperature, float humidity)
            {
                // 境界を厳密に判定するのは面倒なので簡易判定。
                return minTemperature <= temperature && temperature <= maxTemperature &&
                    minHumidity <= humidity && humidity <= maxHumidity;
            }

            #region ToString

            public override string ToString()
            {
                return "{" + minTemperature + " <= Temperature <= " + maxTemperature +
                    ", " + minHumidity + " <= Humidity <= " + maxHumidity + "}";
            }

            #endregion
        }

        #endregion

        // block unit
        public const int SizeX = 256;

        // block unit
        public const int SizeY = 256;

        // block unit
        public const int SizeZ = 256;

        //====================================================================
        //
        // 永続プロパティ
        //

        public string Name { get; set; }

        public INoiseSource TerrainNoise { get; set; }

        public INoiseSource TemperatureNoise { get; set; }

        public INoiseSource HumidityNoise { get; set; }

        public BiomeElement BaseElement { get; set; }

        public Range DesertRange { get; set; }

        public Range PlainsRange { get; set; }

        public Range SnowRange { get; set; }

        public Range MountainsRange { get; set; }

        public Range ForestRange { get; set; }

        //
        //====================================================================

        public byte Index { get; set; }

        public DefaultBiome()
        {
            BaseElement = BiomeElement.Forest;
            DesertRange = new Range
            {
                MinTemperature = 0.5f,
                MaxTemperature = 1.0f,
                MinHumidity = 0.0f,
                MaxHumidity = 0.3f
            };
            PlainsRange = new Range
            {
                MinTemperature = 0.5f,
                MaxTemperature = 1.0f,
                MinHumidity = 0.3f,
                MaxHumidity = 0.6f
            };
            SnowRange = new Range
            {
                MinTemperature = 0.0f,
                MaxTemperature = 0.3f,
                MinHumidity = 0.6f,
                MaxHumidity = 1.0f
            };
            MountainsRange = new Range
            {
                MinTemperature = 0.0f,
                MaxTemperature = 0.5f,
                MinHumidity = 0.0f,
                MaxHumidity = 0.6f
            };
            ForestRange = new Range
            {
                MinTemperature = 0.3f,
                MaxTemperature = 1.0f,
                MinHumidity = 0.6f,
                MaxHumidity = 1.0f
            };
        }

        public float GetTemperature(int x, int z)
        {
            float xf = x / (float) SizeX;
            float zf = z / (float) SizeZ;
            return MathHelper.Clamp(TemperatureNoise.Sample(xf, 0, zf), 0, 1);
        }

        public float GetHumidity(int x, int z)
        {
            float xf = x / (float) SizeX;
            float zf = z / (float) SizeZ;
            return MathHelper.Clamp(HumidityNoise.Sample(xf, 0, zf), 0, 1);
        }

        public BiomeElement GetBiomeElement(int x, int z)
        {
            var temperature = GetTemperature(x, z);
            var humidity = GetHumidity(x, z);

            if (DesertRange.Contains(temperature, humidity))
                return BiomeElement.Desert;

            if (PlainsRange.Contains(temperature, humidity))
                return BiomeElement.Plains;

            if (SnowRange.Contains(temperature, humidity))
                return BiomeElement.Snow;

            if (MountainsRange.Contains(temperature, humidity))
                return BiomeElement.Mountains;

            if (ForestRange.Contains(temperature, humidity))
                return BiomeElement.Forest;

            return BaseElement;
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
