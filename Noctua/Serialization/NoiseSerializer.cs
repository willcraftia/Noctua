#region Using

using System;
using Musca;

#endregion

namespace Noctua.Serialization
{
    public sealed class NoiseSerializer : ModuleSerializer<INoiseSource>
    {
        protected override void InitializeModuleTypeRegistry()
        {
            NoiseTypes.SetTypeDefinitionNames(ModuleTypeRegistry);

            base.InitializeModuleTypeRegistry();
        }
    }
}
