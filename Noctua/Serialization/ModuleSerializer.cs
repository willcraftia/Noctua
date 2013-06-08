#region Using

using System;
using System.IO;
using Libra.IO;
using Pyxis;
using Noctua.Asset;

#endregion

namespace Noctua.Serialization
{
    public class ModuleSerializer<T> : AssetSerializer<T>
    {
        AssetPropertyHandler assetPropertyHandler;

        protected virtual string ModuleName
        {
            get { return "Target"; }
        }

        protected ModuleTypeRegistry ModuleTypeRegistry { get; private set; }

        protected ModuleInfoManager ModuleInfoManager { get; private set; }

        protected ModuleFactory ModuleFactory { get; private set; }

        protected ModuleBundleBuilder ModuleBundleBuilder { get; private set; }

        protected ModuleSerializer()
        {
            ModuleTypeRegistry = new ModuleTypeRegistry();
            ModuleInfoManager = new ModuleInfoManager(ModuleTypeRegistry);
            ModuleFactory = new ModuleFactory(ModuleInfoManager);
            ModuleBundleBuilder = new ModuleBundleBuilder(ModuleInfoManager);
        }

        public override void Initialize(AssetContainer container)
        {
            base.Initialize(container);

            InitializeModuleTypeRegistry();

            assetPropertyHandler = new AssetPropertyHandler(container);
            ModuleFactory.PropertyHandlers.Add(assetPropertyHandler);
        }

        protected virtual void InitializeModuleTypeRegistry() { }

        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<ModuleBundleDefinition>(stream);

            assetPropertyHandler.CurrentBaseResource = resource;
            ModuleFactory.Build(definition);

            var asset = ModuleFactory[ModuleName];

            ModuleFactory.Clear();
            assetPropertyHandler.CurrentBaseResource = null;

            return asset;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            ModuleBundleBuilder.Add(ModuleName, asset);

            var definition = ModuleBundleBuilder.Build();

            WriteObject(stream, definition);
        }
    }
}
