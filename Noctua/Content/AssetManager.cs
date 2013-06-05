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

        AssetCollection assets = new AssetCollection();

        Dictionary<Type, IAssetSerializer> serializerMap;

        public AssetManager(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            serializerMap = new Dictionary<Type, IAssetSerializer>();
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

        public T Load<T>(IResource resource)
        {
            return (T) Load(resource, typeof(T));
        }

        public IAsset Load(IResource resource, Type type)
        {
            if (resource == null) throw new ArgumentNullException("resource");

            IAsset asset;
            if (assets.Contains(resource))
            {
                asset = assets[resource];
            }
            else
            {
                asset = LoadNew(resource, type);
            }

            return asset;
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

        // ※保存に関する注意
        //
        // アセットはキャッシュされるため、一度ロードしたアセットを編集し、これを異なる URI で保存しようとすると、
        // 二つの URI にアセットが関連付けられることとなる。
        //
        // このような関連付けの問題を起こさないために、AssetManager は、アセットが保存要求とは異なる URI に関連付いている場合、
        // その関連付けを破棄してから保存を行う。
        // これは、キャッシュの破棄であり、アセットに対する Dipose 処理は伴わない。
        // 単なるキャッシュの破棄であれば、過去の URI でアセットを得たい場合、Load 処理により新たなアセットがインスタンス化される。
        //
        // 別の問題として、保存の際に異なるアセットが既に URI に関連づいている場合、
        // URI をキーとして間接的に参照しているコードとの不整合が発生する。
        //
        // この解決のために、URI に関連付くアセットがあり、これが保存しようとするアセットとは異なる場合、
        // AssetManager は保存を拒否する。
        // 通常、アセットは他のクラスでも参照しているため、既存のアセットを AssetManager の判断では処理できない。
        // このため、エディタなどの保存要求を出すクラスでは、
        // 上書きをしたい場合には事前に対象となるアセットの削除処理を行わせる必要がある。
        // この削除処理では、単に AssetManager でアセットを削除するだけでなく、
        // 削除したいアセットを参照しているクラスから、その参照を取り除く必要がある。

        public void Save(IResource resource, IAsset asset)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (asset == null) throw new ArgumentNullException("asset");
            if (resource.ReadOnly) throw new InvalidOperationException("Read-only resource: " + resource);

            logger.Info("Save: {0}", resource);

            // 指定の永続先に関連付く他のアセットがある場合は、保存を拒否。
            IAsset otherAsset = null;
            if (assets.Contains(resource))
            {
                otherAsset = assets[resource];
            }
            if (otherAsset != null && !asset.Equals(otherAsset))
                throw new InvalidOperationException(string.Format("Resource '{0}' is bound to the other asset: ", resource));

            // 永続先の変更の可能性があるため、
            // キャッシュ済みならば、そのキャッシュを一度削除。
            if (asset.Resource != null)
            {
                assets.Remove(asset);

                logger.Info("Cache removed: {0}", asset.Resource);
            }

            // シリアライズ。
            var serializer = GetSerializer(asset.GetType());
            serializer.Serialize(resource, asset);

            // 新ためてキャッシュ。
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
