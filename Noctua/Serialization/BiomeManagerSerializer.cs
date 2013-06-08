#region Using

using System;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class BiomeManagerSerializer : ModuleSerializer<IBiomeManager>
    {
        protected override void InitializeModuleTypeRegistry()
        {
            ModuleTypeRegistry.SetTypeDefinitionName(typeof(SingleBiomeManager));

            base.InitializeModuleTypeRegistry();
        }
    }
}
