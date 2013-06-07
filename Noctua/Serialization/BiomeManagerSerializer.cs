#region Using

using System;
using System.IO;
using Libra.IO;
using Pyxis;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class BiomeManagerSerializer : AssetSerializer<IBiomeManager>
    {
        public const string ModuleName = "Target";

        static ModuleTypeRegistory moduleTypeRegistory = new ModuleTypeRegistory();

        ModuleInfoManager moduleInfoManager;

        ModuleFactory moduleFactory;

        ModuleBundleBuilder moduleBundleBuilder;

        AssetPropertyHandler assetPropertyHandler;

        static BiomeManagerSerializer()
        {
            moduleTypeRegistory.SetTypeDefinitionName(typeof(SingleBiomeManager));
        }

        public BiomeManagerSerializer()
        {
            moduleInfoManager = new ModuleInfoManager(moduleTypeRegistory);
            moduleFactory = new ModuleFactory(moduleInfoManager);
            moduleBundleBuilder = new ModuleBundleBuilder(moduleInfoManager);
        }

        public override void Initialize(AssetContainer container)
        {
            assetPropertyHandler = new AssetPropertyHandler(container);
            moduleFactory.PropertyHandlers.Add(assetPropertyHandler);

            base.Initialize(container);
        }

        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<ModuleBundleDefinition>(stream);

            assetPropertyHandler.CurrentBaseResource = resource;
            moduleFactory.Build(definition);

            var biomeManager = moduleFactory[ModuleName] as IBiomeManager;

            moduleFactory.Clear();
            assetPropertyHandler.CurrentBaseResource = null;

            return biomeManager;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var biomeManager = asset as IBiomeManager;

            moduleBundleBuilder.Add(ModuleName, biomeManager);
            var definition = moduleBundleBuilder.Build();

            WriteObject(stream, definition);
        }
    }
}
