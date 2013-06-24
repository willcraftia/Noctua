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

        public static SceneSettingsDefinition FromSceneSettings(SceneSettings settings)
        {
            var instance = new SceneSettingsDefinition
            {
                MidnightSunDirection = settings.MidnightSunDirection,
                MidnightMoonDirection = settings.MidnightMoonDirection,
                ShadowColor = settings.ShadowColor,
                Sunlight = new DirectionalLightDefinition
                {
                    Enabled = settings.Sunlight.Enabled,
                    DiffuseColor = settings.Sunlight.DiffuseColor,
                    SpecularColor = settings.Sunlight.SpecularColor
                },
                Moonlight = new DirectionalLightDefinition
                {
                    Enabled = settings.Moonlight.Enabled,
                    DiffuseColor = settings.Moonlight.DiffuseColor,
                    SpecularColor = settings.Moonlight.SpecularColor
                },
                FogEnabled = settings.FogEnabled,
                FogStart = settings.FogStart,
                FogEnd = settings.FogEnd,
                SecondsPerDay = settings.SecondsPerDay,
                TimeStopped = settings.TimeStopped,
                FixedSecondsPerDay = settings.FixedSecondsPerDay
            };

            if (settings.SkyColors.Count != 0)
                instance.SkyColors = settings.SkyColors.ToArray();

            if (settings.AmbientLightColors.Count != 0)
                instance.AmbientLightColors = settings.AmbientLightColors.ToArray();

            return instance;
        }

        public SceneSettings ToSceneSettings()
        {
            var instance = new SceneSettings
            {
                MidnightMoonDirection = MidnightMoonDirection,
                MidnightSunDirection = MidnightSunDirection,
                ShadowColor = ShadowColor,
                FogEnabled = FogEnabled,
                FogStart = FogStart,
                FogEnd = FogEnd,
                SecondsPerDay = SecondsPerDay,
                TimeStopped = TimeStopped,
                FixedSecondsPerDay = FixedSecondsPerDay
            };

            instance.Sunlight.Enabled = Sunlight.Enabled;
            instance.Sunlight.DiffuseColor = Sunlight.DiffuseColor;
            instance.Sunlight.SpecularColor = Sunlight.SpecularColor;

            instance.Moonlight.Enabled = Moonlight.Enabled;
            instance.Moonlight.DiffuseColor = Moonlight.DiffuseColor;
            instance.Moonlight.SpecularColor = Moonlight.SpecularColor;

            if (SkyColors != null)
                instance.SkyColors.AddColors(SkyColors);

            if (AmbientLightColors != null)
                instance.AmbientLightColors.AddColors(AmbientLightColors);

            return instance;
        }
    }
}
