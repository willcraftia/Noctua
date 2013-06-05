#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Libra.IO;
using Libra.Logging;

#endregion

namespace Noctua.Content
{
    public sealed class AssetManager : IDisposable
    {
        #region AssetCollection

        sealed class AssetCollection : KeyedCollection<IResource, IAsset>
        {
            protected override IResource GetKeyForItem(IAsset item)
            {
                return item.Resource;
            }
        }

        #endregion

        static readonly Logger logger = new Logger(typeof(AssetManager).Name);

        ResourceManager resourceManager;

        AssetCollection assets = new AssetCollection();

        Dictionary<Type, IAssetSerializer> serializerMap = new Dictionary<Type, IAssetSerializer>();

        public AssetManager(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            this.resourceManager = resourceManager;
        }

        public void RegisterLoader(Type type, IAssetSerializer serializer)
        {
            serializerMap[type] = serializer;

            var assetManagerAware = serializer as IAssetManagerAware;
            if (assetManagerAware != null) assetManagerAware.AssetManager = this;
        }

        public bool Unregister(Type type)
        {
            IAssetSerializer serializer;
            if (serializerMap.TryGetValue(type, out serializer))
            {
                var assetManagerAware = serializer as IAssetManagerAware;
                if (assetManagerAware != null) assetManagerAware.AssetManager = null;

                serializerMap.Remove(type);
                return true;
            }

            return false;
        }

        public T Load<T>(string uri) where T : IAsset
        {
            var resource = resourceManager.Load(uri);
            return Load<T>(resource);
        }

        public T Load<T>(IResource resource) where T : IAsset
        {
            if (resource == null) throw new ArgumentNullException("resource");

            IAsset asset;
            if (assets.Contains(resource))
            {
                asset = assets[resource];
            }
            else
            {
                asset = LoadNew(resource, typeof(T));
            }

            return (T) asset;
        }

        IAsset LoadNew(IResource resource, Type type)
        {
            logger.Info("LoadNew: {0}", resource);

            var asset = GetSerializer(type).Deserialize(resource);

            // キャッシュ。
            assets.Add(asset);

            return asset;
        }

        public void Unload(IResource resource)
        {
            logger.Info("Unload: {0}", resource);

            if (assets.Contains(resource))
            {
                var asset = assets[resource];
                
                DisposeIfNeeded(asset);
                assets.Remove(asset);
            }
        }

        public void Unload()
        {
            if (assets.Count == 0) return;

            while (0 < assets.Count)
                Unload(assets[assets.Count - 1].Resource);
        }

        // ※永続化に関する注意
        //
        // アセット キャッシュは、アセットと URI が一対一関連であることを前提とする。
        // 既に URI に関連付く永続化済みアセットを、異なる URI で永続化したい場合には、
        // アセットを複製してから永続化を試行するものとする。
        //
        // 既に URI に関連付いているアセットは、その URI でのみ永続化できる。
        //
        // URI を明示して永続化する場合、マネージャはまだ URI に関連付いていないアセットを要求する。
        // URI を明示して永続化する際に、その URI に関連付くアセットが存在する場合、
        // マネージャは永続化を拒否する。
        // これは、仮に URI に関連付くアセットが、永続化しようとするアセットと同一であったとしても、
        // 永続化を拒否する。
        //
        // マネージャからロードしたアセットを上書きする場合、
        // 仮に URI が示すリソースが外部で削除されていたとしても、新規生成として成功する。
        //
        // 読み取り専用コンテナを示すリソースである場合、
        // いずれのケースにおいてもマネージャからの永続化は失敗する。
        //
        // あるアセット A を参照するアセット B があり、
        // アセット A を複製してアセット A' を永続化した場合、
        // アセット B は依然としてアセット A を参照する。
        // アセット B が参照するアセット A を、
        // 自動的にアセット A' へ変更する仕組みをマネージャは提供しない。

        public void Save(IAsset asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (asset.Resource == null) throw new ArgumentException("Resource is unknown.", "asset");

            logger.Info("Save: {0}", asset.Resource);

            // リソースが null ではないことを前提とするため、
            // アセットがキャッシュに存在する前提となる。

            // シリアライズ。
            var serializer = GetSerializer(asset.GetType());
            serializer.Serialize(asset.Resource, asset);
        }

        public void SaveAs(IAsset asset, string uri)
        {
            var resource = resourceManager.Load(uri);
            SaveAs(asset, resource);
        }

        public void SaveAs(IAsset asset, IResource resource)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (asset == null) throw new ArgumentNullException("asset");

            // 既にリソースと関連付いているアセットの永続化は拒否。
            if (asset.Resource != null) throw new ArgumentException(
                string.Format("Asset is associated with the resource '{0}'.", asset.Resource), "asset");

            // 読み取り専用リソースへの永続化は拒否。
            if (resource.ReadOnly) throw new ArgumentException(
                string.Format("Resource '{0}' is read-only.", resource), "resource");

            // 既に存在するリソースへの永続化は拒否。
            if (resource.Exists) throw new ArgumentException(
                string.Format("Resource '{0}' already exists.", resource), "resource");

            logger.Info("SaveAs: {0}", resource);

            // シリアライズ。
            var serializer = GetSerializer(asset.GetType());
            serializer.Serialize(resource, asset);

            // キャッシュ。
            assets.Add(asset);
        }

        IAssetSerializer GetSerializer(Type type)
        {
            IAssetSerializer result;
            if (TryGetSerializer(type, out result))
                return result;

            throw new InvalidOperationException(string.Format("Serializer for '{0}' can not be found: ", type));
        }

        bool TryGetSerializer(Type type, out IAssetSerializer result)
        {
            if (serializerMap.TryGetValue(type, out result))
                return true;

            // By the interface.
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (TryGetSerializer(interfaceType, out result))
                    return true;
            }

            // By the base type.
            if (type.BaseType != null && TryGetSerializer(type.BaseType, out result))
                return true;

            return false;
        }

        void DisposeIfNeeded(IAsset asset)
        {
            var disposable = asset as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~AssetManager()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            Unload();

            disposed = true;
        }

        #endregion
    }
}
