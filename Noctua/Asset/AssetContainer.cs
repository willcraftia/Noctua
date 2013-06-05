#region Using

using System;
using System.Collections.Generic;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public sealed class AssetContainer : IAssetLoader
    {
        Dictionary<IResource, IAsset> cache = new Dictionary<IResource, IAsset>();

        public ResourceManager ResourceManager { get; private set; }

        public IAssetSerializer Serializer { get; private set; }

        // もし、異なるシリアライザを用いたい場合には、
        // 異なるコンテナを利用する。
        // 複雑化させたくないので、シリアライザが異なるアセット間での参照は許可しないことにする。

        public AssetContainer(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            // デフォルトは JSON。
            Serializer = JsonAssetSerializer.Instance;
            ResourceManager = resourceManager;
        }

        public AssetContainer(ResourceManager resourceManager, IAssetSerializer serializer)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (serializer == null) throw new ArgumentNullException("serializer");

            Serializer = serializer;
            ResourceManager = resourceManager;
        }

        public void Create(IAsset asset, string uri)
        {
            var resource = ResourceManager.Load(uri);
            Create(asset, resource);
        }

        public void Create(IAsset asset, IResource resource)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (resource == null) throw new ArgumentNullException("resource");

            // コンテナ管理内のアセットを拒否。
            // これを行いたい場合、デタッチ、あるいは、削除してから新規生成を試みる。
            AssetUnmanagedAsset(asset);

            // 前処理。
            var preStore = asset as IAssetPreStore;
            if (preStore != null)
            {
                preStore.PreStore(resource, ResourceManager);
            }

            // シリアライズ。
            using (var stream = resource.CreateNew())
            {
                Serializer.WriteAsset(stream, asset);
            }

            // リソース設定。
            asset.Resource = resource;

            // キャッシュ登録。
            cache[resource] = asset;
        }

        public T Load<T>(string uri) where T : IAsset
        {
            var resource = ResourceManager.Load(uri);
            return Load<T>(resource);
        }

        public T Load<T>(IResource resource) where T : IAsset
        {
            if (resource == null) throw new ArgumentNullException("resource");

            // TODO
            // 引数検査。

            // デシリアライズ。
            IAsset asset;
            using (var stream = resource.OpenRead())
            {
                asset = Serializer.ReadAsset<T>(stream);
            }

            // リソース設定。
            asset.Resource = resource;

            // キャッシュ登録。
            cache[resource] = asset;

            // 後処理。
            var postLoad = asset as IAssetPostLoad;
            if (postLoad != null)
            {
                postLoad.PostLoad(this);
            }

            return (T) asset;
        }

        public void Update(IAsset asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // 前処理。
            var preStore = asset as IAssetPreStore;
            if (preStore != null)
            {
                preStore.PreStore(asset.Resource, ResourceManager);
            }

            // シリアライズ。
            using (var stream = asset.Resource.OpenWrite())
            {
                Serializer.WriteAsset(stream, asset);
            }
        }

        public void Delete(IAsset asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // 削除。
            asset.Resource.Delete();

            // デタッチ。
            Detach(asset);
        }

        public void Detach(IAsset asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // リソースをアンバインド。
            asset.Resource = null;

            // キャッシュ削除。
            cache.Remove(asset.Resource);
        }

        public void DetachAll()
        {
            foreach (var asset in cache.Values)
            {
                // リソースをアンバインド。
                asset.Resource = null;
            }

            // キャッシュ削除。
            cache.Clear();
        }

        public void DetachAndDispose(IAsset asset)
        {
            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // デタッチ。
            Detach(asset);

            // 破棄。
            DisposeIfNeeded(asset);
        }

        public void DetachAndDisposeAll()
        {
            foreach (var asset in cache.Values)
            {
                // リソースをアンバインド。
                asset.Resource = null;

                // 破棄。
                DisposeIfNeeded(asset);
            }

            // キャッシュ削除。
            cache.Clear();
        }

        void DisposeIfNeeded(IAsset asset)
        {
            var disposable = asset as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        void AssetUnmanagedAsset(IAsset asset)
        {
            if (asset.Resource != null)
                throw new ArgumentException(
                    string.Format("The asset is associated with the resource '{0}'.", asset.Resource), "asset");
        }

        void AssetManagedAsset(IAsset asset)
        {
            if (asset.Resource == null)
                throw new ArgumentException("The asset is associated without any resource.", "asset");
        }
    }
}
