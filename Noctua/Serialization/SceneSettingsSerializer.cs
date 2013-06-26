#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class SceneSettingsSerializer : AssetSerializer<SceneSettings>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<SceneSettingsDefinition>(stream);

            var settings = new SceneSettings
            {
                MidnightSunDirection = definition.MidnightSunDirection,
                MidnightMoonDirection = definition.MidnightMoonDirection,
                ShadowColor = definition.ShadowColor,
                FogEnabled = definition.FogEnabled,
                FogStart = definition.FogStart,
                FogEnd = definition.FogEnd,
                SecondsPerDay = definition.SecondsPerDay,
                TimeStopped = definition.TimeStopped,
                FixedSecondsPerDay = definition.FixedSecondsPerDay
            };

            settings.Sunlight.Enabled = definition.Sunlight.Enabled;
            settings.Sunlight.DiffuseColor = definition.Sunlight.DiffuseColor;
            settings.Sunlight.SpecularColor = definition.Sunlight.SpecularColor;

            settings.Moonlight.Enabled = definition.Moonlight.Enabled;
            settings.Moonlight.DiffuseColor = definition.Moonlight.DiffuseColor;
            settings.Moonlight.SpecularColor = definition.Moonlight.SpecularColor;

            if (definition.SkyColors != null)
                settings.SkyColors.AddColors(definition.SkyColors);

            if (definition.AmbientLightColors != null)
                settings.AmbientLightColors.AddColors(definition.AmbientLightColors);

            return settings;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var settings = asset as SceneSettings;

            var definition = new SceneSettingsDefinition
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
                definition.SkyColors = settings.SkyColors.ToArray();

            if (settings.AmbientLightColors.Count != 0)
                definition.AmbientLightColors = settings.AmbientLightColors.ToArray();

            WriteObject(stream, definition);
        }
    }
}
