#region Using

using System;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class BiomeSerializer : ModuleSerializer<IBiome>
    {
        protected override void InitializeModuleTypeRegistry()
        {
            NoiseTypes.SetTypeDefinitionNames(ModuleTypeRegistry);
            ModuleTypeRegistry.SetTypeDefinitionName(typeof(DefaultBiome));
            ModuleTypeRegistry.SetTypeDefinitionName(typeof(DefaultBiome.Range));

            base.InitializeModuleTypeRegistry();
        }
    }
}
