﻿#region Using

using System;
using System.Collections.Generic;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;
using Libra.IO;
using Noctua.Asset;
using Noctua.Scene;
using Noctua.Serialization;

#endregion

namespace Noctua.Models
{
    public sealed class RegionManager : IDisposable
    {
        ResourceManager resourceManager = new ResourceManager();

        AssetContainer assetContainer;

        List<Region> regions = new List<Region>();

        SkySphere skySphere;

        IEffect chunkEffect;

        ParticleSystem snowParticleSystem;

        ParticleSystem rainParticleSystem;

        public static bool Wireframe { get; set; }

        public SceneSettings SceneSettings { get; private set; }

        public DeviceContext DeviceContext { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public RegionManager(SceneManager sceneManager)
        {
            if (sceneManager == null) throw new ArgumentNullException("sceneManager");

            SceneManager = sceneManager;
            DeviceContext = sceneManager.DeviceContext;

            // TODO
            //
            // タイトル ステージ対応くらいはしたい。
            resourceManager.Register<FileResourceLoader>();

            assetContainer = new AssetContainer(DeviceContext, resourceManager, JsonObjectSerializer.Instance);
            assetContainer.RegisterAssetSerializer<Texture2DSerializer>();
            assetContainer.RegisterAssetSerializer<ParticleSystemSerializer>();
            assetContainer.RegisterAssetSerializer<SkySphereSerializer>();
        }

        public void Initialize(SceneSettings sceneSettings)
        {
            if (sceneSettings == null) throw new ArgumentNullException("sceneSettings");

            SceneSettings = sceneSettings;

            // スカイ スフィア
            skySphere = assetContainer.Load<SkySphere>("title:Resources/Models/SkySphere.json");

            // シーン マネージャへ登録
            var skySphereNode = new SceneNode(SceneManager, "SkySphere");
            skySphereNode.Objects.Add(skySphere);

            SceneManager.SkySphere = skySphereNode;

            //----------------------------------------------------------------
            // チャンク エフェクト

            //chunkEffect = assetContainer.Load<Effect>("content:Effects/Chunk");

            // TODO
            //
            // パーティクルは個別ではなくリスト形式で管理すべき。
            // また、バイオームで定義すべき。


            // 降雪パーティクル
            snowParticleSystem = assetContainer.Load<ParticleSystem>("title:Resources/Particles/Snow.json");
            SceneManager.ParticleSystems.Add(snowParticleSystem);
            snowParticleSystem.Enabled = false;

            // 降雨パーティクル
            rainParticleSystem = assetContainer.Load<ParticleSystem>("title:Resources/Particles/Rain.json");
            SceneManager.ParticleSystems.Add(rainParticleSystem);
            rainParticleSystem.Enabled = false;
        }

        //
        // AssetManager の扱い
        //
        // エディタ:
        //      エディタで一つの AssetManager。
        //      各モデルは、必ずしも Region に関連付けられるとは限らない。
        //
        // ゲーム環境:
        //      Region で一つの AssetManager。
        //      各モデルは、必ず一つの Region に関連付けられる。
        //
        // ※
        // 寿命としては ResourceManager も同様。
        // ただし、ResourceManager は AssetManager から Region をロードするに先駆け、
        // Region の Resource を解決するために用いられる点に注意する。
        //

        // 非同期呼び出しを想定。
        // Region 丸ごと別スレッドでロードするつもり。
        public Region LoadRegion(string uri)
        {
            var localResourceManager = new ResourceManager();

            var resource = localResourceManager.Load(uri);

            var localAssetContainer = new AssetContainer(DeviceContext, localResourceManager, JsonObjectSerializer.Instance);
            localAssetContainer.RegisterAssetSerializer<RegionSerializer>();
            localAssetContainer.RegisterAssetSerializer<Texture2DSerializer>();
            localAssetContainer.RegisterAssetSerializer<MeshSerializer>();
            localAssetContainer.RegisterAssetSerializer<TileSerializer>();
            localAssetContainer.RegisterAssetSerializer<TileCatalogSerializer>();
            localAssetContainer.RegisterAssetSerializer<BlockSerializer>();
            localAssetContainer.RegisterAssetSerializer<BlockCatalogSerializer>();
            localAssetContainer.RegisterAssetSerializer<BiomeSerializer>();
            localAssetContainer.RegisterAssetSerializer<BiomeCatalogSerializer>();
            localAssetContainer.RegisterAssetSerializer<ChunkProcedureSerializer>();

            //assetManager.RegisterLoader(typeof(IBiomeManager), new BiomeManagerLoader(localResourceManager));
            //assetManager.RegisterLoader(typeof(INoiseSource), new NoiseLoader(localResourceManager));

            var region = localAssetContainer.Load<Region>(uri);
            region.Initialize(localAssetContainer);

            lock (regions)
            {
                regions.Add(region);
            }

            return region;
        }

        public Region GetRegionByChunkPosition(IntVector3 chunkPosition)
        {
            lock (regions)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    var region = regions[i];
                    if (region.Box.Contains(chunkPosition))
                    {
                        return region;
                    }
                }
            }
            return null;
        }

        //
        // TODO
        //
        static Random random = new Random();

        public void Update(GameTime gameTime)
        {
            // シーン設定。
            SceneSettings.Update(gameTime);

            // TODO
            //
            // パーティクルの扱いを見直す。

            // 降雪パーティクル。
            if (snowParticleSystem.Enabled)
            {
                int boundsX = 128 * 2;
                int boundsZ = 128 * 2;
                int minY = 32;
                int maxY = 64;

                for (int i = 0; i < 40; i++)
                {
                    var randomX = random.Next(boundsX) - boundsX / 2;
                    var randomY = random.Next(maxY - minY) + minY;
                    var randomZ = random.Next(boundsZ) - boundsZ / 2;
                    var position = new Vector3(randomX, randomY, randomZ) + SceneManager.ActiveCamera.Position;
                    snowParticleSystem.AddParticle(position, Vector3.Zero);
                }
            }

            // 降雨パーティクル。
            if (rainParticleSystem.Enabled)
            {
                int boundsX = 128 * 2;
                int boundsZ = 128 * 2;
                int minY = 32;
                int maxY = 64;

                for (int i = 0; i < 80; i++)
                {
                    var randomX = random.Next(boundsX) - boundsX / 2;
                    var randomY = random.Next(maxY - minY) + minY;
                    var randomZ = random.Next(boundsZ) - boundsZ / 2;
                    var position = new Vector3(randomX, randomY, randomZ) + SceneManager.ActiveCamera.Position;
                    rainParticleSystem.AddParticle(position, Vector3.Zero);
                }
            }

            // スカイスフィア。
            skySphere.SkyColor = SceneSettings.CurrentSkyColor;
            skySphere.SunColor = SceneSettings.Sunlight.DiffuseColor;
            skySphere.SunDirection = SceneSettings.Sunlight.Direction;
        }

        //public void PrepareChunkEffects()
        //{
        //    for (int i = 0; i < regions.Count; i++)
        //    {
        //        var region = regions[i];
        //        SceneManager.UpdateEffect(region.ChunkEffect);

        //        region.ChunkEffect.WireframeEnabled = Wireframe;
        //    }
        //}

        //internal void OnShadowMapUpdated(object sender, EventArgs e)
        //{
        //    for (int i = 0; i < regions.Count; i++)
        //    {
        //        var region = regions[i];
        //        SceneManager.UpdateEffectShadowMap(region.ChunkEffect);
        //    }
        //}

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~RegionManager()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                assetContainer.Dispose();
                snowParticleSystem.Dispose();
                rainParticleSystem.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
