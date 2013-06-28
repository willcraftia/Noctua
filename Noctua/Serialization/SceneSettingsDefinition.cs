#region Using

using System;
using Libra;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class SceneSettingsDefinition
    {
        public float SecondsPerDay;

        public bool TimeStopped;

        public float FixedSecondsPerDay;

        public bool FogEnabled;

        public float FogStart;

        public float FogEnd;

        public Vector3 MidnightSunDirection;

        public Vector3 MidnightMoonDirection;

        public Vector3 ShadowColor;

        public Vector3 SkyColor;

        public bool SunlightEnabled;

        public bool MoonlightEnabled;

        public TimeColor[] SunlightDiffuseColors;

        public TimeColor[] MoonlightDiffuseColors;
    }
}
