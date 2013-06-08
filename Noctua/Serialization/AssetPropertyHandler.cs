#region Using

using System;
using System.Reflection;
using Libra.IO;
using Pyxis;
using Noctua.Asset;

#endregion

namespace Noctua.Serialization
{
    public sealed class AssetPropertyHandler : IModulePropertyHandler
    {
        // 最後の IPropertyHandler として登録し、
        // それまでの IPropertyHandler で拾えなかったプロパティがアセット参照であると仮定して処理を進める。

        AssetContainer assetContainer;

        public IResource CurrentBaseResource { get; set; }

        public AssetPropertyHandler(AssetContainer assetContainer)
        {
            if (assetContainer == null) throw new ArgumentNullException("assetContainer");

            this.assetContainer = assetContainer;
        }

        public bool SetPropertyValue(object module, PropertyInfo property, string propertyValue)
        {
            if (propertyValue == null) return false;

            // リソースを解決。
            var resource = assetContainer.ResourceManager.Load(CurrentBaseResource, propertyValue);

            // プロパティ型によりアセットをロード。
            var asset = assetContainer.Load(resource, property.PropertyType);

            // モジュールのプロパティへ設定。
            property.SetValue(module, asset, null);

            return true;
        }
    }
}
