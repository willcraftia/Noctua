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
                SecondsPerDay = definition.SecondsPerDay,
                TimeStopped = definition.TimeStopped,
                FixedSecondsPerDay = definition.FixedSecondsPerDay,
                FogEnabled = definition.FogEnabled,
                FogStart = definition.FogStart,
                FogEnd = definition.FogEnd,
                MidnightSunDirection = definition.MidnightSunDirection,
                MidnightMoonDirection = definition.MidnightMoonDirection,
                ShadowColor = definition.ShadowColor,
                SkyColor = definition.SkyColor,
                SunlightEnabled = definition.SunlightEnabled,
                MoonlightEnabled = definition.MoonlightEnabled
            };

            if (definition.SunlightDiffuseColors != null)
                settings.SunlightDiffuseColors.AddColors(definition.SunlightDiffuseColors);

            if (definition.MoonlightDiffuseColors != null)
                settings.MoonlightDiffuseColors.AddColors(definition.MoonlightDiffuseColors);

            return settings;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var settings = asset as SceneSettings;

            var definition = new SceneSettingsDefinition
            {
                SecondsPerDay = settings.SecondsPerDay,
                TimeStopped = settings.TimeStopped,
                FixedSecondsPerDay = settings.FixedSecondsPerDay,
                FogEnabled = settings.FogEnabled,
                FogStart = settings.FogStart,
                FogEnd = settings.FogEnd,
                MidnightSunDirection = settings.MidnightSunDirection,
                MidnightMoonDirection = settings.MidnightMoonDirection,
                ShadowColor = settings.ShadowColor,
                SkyColor = settings.SkyColor,
                SunlightEnabled = settings.SunlightEnabled,
                MoonlightEnabled = settings.MoonlightEnabled,
            };

            if (settings.SunlightDiffuseColors.Count != 0)
                definition.SunlightDiffuseColors = settings.SunlightDiffuseColors.ToArray();

            if (settings.MoonlightDiffuseColors.Count != 0)
                definition.MoonlightDiffuseColors = settings.MoonlightDiffuseColors.ToArray();

            WriteObject(stream, definition);
        }
    }
}
