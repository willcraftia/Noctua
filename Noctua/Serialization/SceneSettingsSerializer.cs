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
            return definition.ToSceneSettings();
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var settings = asset as SceneSettings;
            var definition = SceneSettingsDefinition.FromSceneSettings(settings);
            WriteObject(stream, definition);
        }
    }
}
