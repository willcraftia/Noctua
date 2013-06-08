#region Using

using System;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class ChunkProcedureSerializer : ModuleSerializer<IChunkProcedure>
    {
        protected override void InitializeModuleTypeRegistry()
        {
            NoiseTypes.SetTypeDefinitionNames(ModuleTypeRegistry);
            ModuleTypeRegistry.SetTypeDefinitionName(typeof(FlatTerrainProcedure));
            ModuleTypeRegistry.SetTypeDefinitionName(typeof(DefaultTerrainProcedure));

            base.InitializeModuleTypeRegistry();
        }
    }
}
