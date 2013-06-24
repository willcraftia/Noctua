#region Using

using System;
using Libra;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class SceneSettingsDefinition
    {
        public Vector3 MidnightSunDirection;

        public Vector3 MidnightMoonDirection;

        public Vector3 ShadowColor;

        public DirectionalLightDefinition Sunlight;

        public DirectionalLightDefinition Moonlight;

        public TimeColor[] SkyColors;

        public TimeColor[] AmbientLightColors;

        public bool FogEnabled;

        public float FogStart;

        public float FogEnd;

        public float SecondsPerDay;

        public bool TimeStopped;

        public float FixedSecondsPerDay;
    }
}
