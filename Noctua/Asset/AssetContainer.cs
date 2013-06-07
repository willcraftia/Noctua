#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Libra.Graphics;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public sealed class AssetContainer : IDisposable
    {
        Dictionary<Type, AssetSerializer> assetSerializers = new Dictionary<Type, AssetSerializer>();

        Dictionary<IResource, object> cache = new Dictionary<IResource, object>();

        Dictionary<object, IResource> reverseCache = new Dictionary<object, IResource>();

        public DeviceContext DeviceContext { get; private set; }

        public ResourceManager ResourceManager { get; private set; }

        public IObjectSerializer ObjectSerializer { get; private set; }

        // もし、異なるシリアライザを用いたい場合には、
        // 異なるコンテナを利用する。
        // 複雑化させたくないので、シリアライザが異なるアセット間での参照は許可しないことにする。

        public AssetContainer(DeviceContext deviceContext, ResourceManager resourceManager, IObjectSerializer objectSerializer)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");
            if (resourceManager == null) throw new ArgumentNullException("resourceManager");
            if (objectSerializer == null) throw new ArgumentNullException("objectSerializer");

            DeviceContext = deviceContext;
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

            // シリアライズ。
            using (var stream = resource.CreateNew())
            {
                // アセット シリアライザ。
                var assetSerializer = GetAssetSerializer(asset.GetType());

                if (assetSerializer != null)
                {
                    assetSerializer.WriteAsset(stream, resource, asset);
                }
                else
                {
                    ObjectSerializer.WriteObject(stream, asset);
                }
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
            return (T) Load(resource, typeof(T));
        }

        // 引数による Type 指定は、動的に型が定まる場合に必要。
        // 静的に型が定まる場合は、型引数による指定の方が便利。

        public object Load(IResource resource, Type type)
        {
            if (resource == null) throw new ArgumentNullException("resource");
            if (type == null) throw new ArgumentNullException("type");

            // キャッシュ検索。
            object asset;
            if (!cache.TryGetValue(resource, out asset))
            {
                // デシリアライズ。
                using (var stream = resource.OpenRead())
                {
                    // アセット シリアライザ。
                    var assetSerializer = GetAssetSerializer(type);

                    if (assetSerializer != null)
                    {
                        asset = assetSerializer.ReadAsset(stream, resource);
                    }
                    else
                    {
                        asset = ObjectSerializer.ReadObject(stream, type);
                    }
                }

                // キャッシュ登録。
                cache[resource] = asset;
                reverseCache[asset] = resource;
            }

            return asset;
        }

        public void Update(object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            // コンテナ管理外のアセットを拒否。
            AssetManagedAsset(asset);

            // リソース。
            var resource = reverseCache[asset];

            // シリアライズ。
            using (var stream = resource.OpenWrite())
            {
                // アセット シリアライザ。
                var assetSerializer = GetAssetSerializer(asset.GetType());

                if (assetSerializer != null)
                {
                    assetSerializer.WriteAsset(stream, resource, asset);
                }
                else
                {
                    ObjectSerializer.WriteObject(stream, asset);
                }
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

        AssetSerializer GetAssetSerializer(Type type)
        {
            AssetSerializer assetSerializer;
            assetSerializers.TryGetValue(type, out assetSerializer);
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
