#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public sealed class AssetContainer : IDisposable
    {
        Dictionary<Type, AssetSerializer> assetSerializers = new Dictionary<Type, AssetSerializer>();

        Dictionary<IResource, object> cache = new Dictionary<IResource, object>();

        Dictionary<object, IResource> reverseCache = new Dictionary<object, IResource>();

        public ResourceManager ResourceManager { get; private set; }

        public IObjectSerializer ObjectSerializer { get; private set; }

        // もし、異なるシリアライザを用いたい場合には、
        // 異なるコンテナを利用する。
        // 複雑化させたくないので、シリアライザが異なるアセット間での参照は許可しないことにする。

        public AssetContainer(ResourceManager resourceManager)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");

            // デフォルトは JSON。
            ObjectSerializer = JsonObjectSerializer.Instance;
            ResourceManager = resourceManager;
        }

        public AssetContainer(ResourceManager resourceManager, IObjectSerializer objectSerializer)
        {
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (objectSerializer == null) throw new ArgumentNullException("objectSerializer");

            ObjectSerializer = objectSerializer;
            ResourceManager = resourceManager;
        }

        public void RegisterAssetSerializer<T>() where T : AssetSerializer, new()
        {
            var assetSerializer = new T();
            assetSerializer.Initialize(this);
            assetSerializers[assetSerializer.AssetType] = assetSerializer;
        }

        public void DeregisterAssetSerializer<T>() where T : AssetSerializer, new()
        {
            var assetSerializer = new T();
            assetSerializers.Remove(assetSerializer.AssetType);
        }

        public IResource GetResource(object asset)
        {
            IResource resource;
            reverseCache.TryGetValue(asset, out resource);
            return resource;
        }

        public void Create(string uri, object asset)
        {
            var resource = ResourceManager.Load(uri);
            Create(resource, asset);
        }

        public void Create(IResource resource, object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (resource == null) throw new ArgumentNullException("resource");

            // コンテナ管理内のアセットを拒否。
            // これを行いたい場合、デタッチ、あるいは、削除してから新規生成を試みる。
            AssetUnmanagedAsset(asset);

            // アセット シリアライザ。
            var assetSerializer = GetRequiredAssetSerializer(asset.GetType());

            // シリアライズ。
            using (var stream = resource.CreateNew())
            {
                assetSerializer.WriteAsset(stream, resource, asset);
            }

            // キャッシュ登録。
            cache[resource] = asset;
            reverseCache[asset] = resource;
        }

        public T Load<T>(string uri)
        {
            var resource = ResourceManager.Load(uri);
            return Load<T>(resource);
        }

        public T Load<T>(IResource resource)
        {
            if (resource == null) throw new ArgumentNullException("resource");

            // キャッシュ検索。
            object asset;
            if (!cache.TryGetValue(resource, out asset))
            {
                // アセット シリアライザ。
                var assetSerializer = GetRequiredAssetSerializer(typeof(T));

                // デシリアライズ。
                using (var stream = resource.OpenRead())
                {
                    asset = assetSerializer.ReadAsset(stream, resource);
                }

                // キャッシュ登録。
                cache[resource] = asset;
                reverseCache[asset] = resource;
            }

            return (T) asset;
        }

        public void Update(object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // リソース。
            var resource = reverseCache[asset];

            // アセット シリアライザ。
            var assetSerializer = GetRequiredAssetSerializer(asset.GetType());

            // シリアライズ。
            using (var stream = resource.OpenWrite())
            {
                assetSerializer.WriteAsset(stream, resource, asset);
            }
        }

        public void Delete(object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // リソース。
            var resource = reverseCache[asset];

            // 削除。
            resource.Delete();

            // キャッシュ削除。
            cache.Remove(resource);
        }

        public void Detach(object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // リソース。
            var resource = reverseCache[asset];

            // キャッシュ削除。
            cache.Remove(resource);
        }

        public void DetachAll()
        {
            // キャッシュ削除。
            cache.Clear();
            reverseCache.Clear();
        }

        public void DetachAndDispose(object asset)
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
                // 破棄。
                DisposeIfNeeded(asset);
            }

            // キャッシュ削除。
            cache.Clear();
        }

        void DisposeIfNeeded(object asset)
        {
            var disposable = asset as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        AssetSerializer GetRequiredAssetSerializer(Type type)
        {
            AssetSerializer assetSerializer;
            if (!assetSerializers.TryGetValue(type, out assetSerializer))
            {
                throw new InvalidOperationException(
                    string.Format("AssetSerializer for '{0} does not exist.", type));
            }
            return assetSerializer;
        }

        void AssetUnmanagedAsset(object asset)
        {
            if (reverseCache.ContainsKey(asset))
                throw new ArgumentException(
                    string.Format("The asset is associated with the resource '{0}'.", reverseCache[asset]), "asset");
        }

        void AssetManagedAsset(object asset)
        {
            if (!reverseCache.ContainsKey(asset))
                throw new ArgumentException("The asset is associated without any resource.", "asset");
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~AssetContainer()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            DetachAndDisposeAll();

            disposed = true;
        }

        #endregion
    }
}
