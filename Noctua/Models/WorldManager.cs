﻿#region Using

using System;
using Libra;
using Libra.Graphics;
using Libra.IO;
using Noctua.Asset;
using Noctua.Scene;
using Noctua.Serialization;

#endregion

namespace Noctua.Models
{
    public sealed class WorldManager
    {
        SpriteBatch spriteBatch;

        ResourceManager resourceManager = new ResourceManager();

        AssetContainer assetContainer;

        SceneCamera defaultCamera = new SceneCamera("Default");

        public DeviceContext DeviceContext { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public SceneManager.Settings SceneManagerSettings { get; private set; }

        public ChunkManager ChunkManager { get; private set; }

        public RegionManager RegionManager { get; private set; }

        public GraphicsSettings GraphicsSettings { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

        public WorldManager(DeviceContext deviceContext)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            DeviceContext = deviceContext;

            spriteBatch = new SpriteBatch(deviceContext);

            resourceManager.Register<TitleResourceLoader>();

            assetContainer = new AssetContainer(deviceContext, resourceManager, JsonObjectSerializer.Instance);
            assetContainer.RegisterAssetSerializer<GraphicsSettingsSerializer>();
            assetContainer.RegisterAssetSerializer<SceneSettingsSerializer>();
            assetContainer.RegisterAssetSerializer<ChunkSettingsSerializer>();

            GraphicsSettings = assetContainer.Load<GraphicsSettings>("title:Resources/GraphicsSettings.json");
            SceneSettings = assetContainer.Load<SceneSettings>("title:Resources/SceneSettings.json");
            // TODO: リソースから取得する。
            SceneManagerSettings = new SceneManager.Settings();

            //----------------------------------------------------------------
            // シーン マネージャ

            SceneManager = new SceneManager(SceneManagerSettings, DeviceContext);

            // 太陽と月をディレクショナル ライトとして登録。
            SceneManager.DirectionalLights.Add(SceneSettings.Sunlight);
            SceneManager.DirectionalLights.Add(SceneSettings.Moonlight);

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager = new RegionManager(this);

            //----------------------------------------------------------------
            // チャンク マネージャ

            var chunkSettings = assetContainer.Load<ChunkSettings>("title:Resources/ChunkSettings.json");
            ChunkManager = new ChunkManager(chunkSettings, RegionManager, SceneManager);

            //----------------------------------------------------------------
            // デフォルト カメラ

            defaultCamera.Position = new Vector3(0, 16 * 16, 0);
            defaultCamera.AspectRatio = DeviceContext.Viewport.AspectRatio;

            // 最大アクティブ範囲を超えない位置へ FarPlaneDistance を設定。
            // パーティション (チャンク) のサイズを掛けておく。

            var minChunkSize = Math.Min(chunkSettings.ChunkSize.X, chunkSettings.ChunkSize.Y);
            minChunkSize = Math.Min(minChunkSize, chunkSettings.ChunkSize.Z);
            defaultCamera.FarClipDistance = (chunkSettings.MaxActiveRange - 1) * minChunkSize;

            // 念のためここで一度更新。
            defaultCamera.Update();

            // シーン マネージャへ登録してアクティブ化。
            SceneManager.Cameras.Add(defaultCamera);
            SceneManager.ActiveCameraName = defaultCamera.Name;
        }

        // TODO: 戻り値を Region にしない。
        public Region Load(string worldUri)
        {
            // TODO
            return RegionManager.LoadRegion("title:Resources/Regions/Default.json");
        }

        public void Unload()
        {
            // TODO
        }

        public void Update(GameTime gameTime)
        {
            //----------------------------------------------------------------
            // カメラ更新

            SceneManager.ActiveCamera.Update();

            //----------------------------------------------------------------
            // シーン設定

            SceneSettings.Update(gameTime);

            SceneManager.AmbientLightColor = SceneSettings.CurrentAmbientLightColor;

            if (SceneSettings.Sunlight.Enabled && SceneSettings.SunAboveHorizon)
            {
                SceneManager.ActiveDirectionalLightName = SceneSettings.Sunlight.Name;
            }
            else if (SceneSettings.Moonlight.Enabled && SceneSettings.MoonAboveHorizon)
            {
                SceneManager.ActiveDirectionalLightName = SceneSettings.Moonlight.Name;
            }
            else
            {
                SceneManager.ActiveDirectionalLightName = null;
            }

            if (SceneSettings.FogEnabled)
            {
                var far = SceneManager.ActiveCamera.FarClipDistance;
                SceneManager.FogStart = far * SceneSettings.FogStart;
                SceneManager.FogEnd = far * SceneSettings.FogEnd;
                SceneManager.FogColor = SceneSettings.CurrentSkyColor;
            }
            SceneManager.FogEnabled = SceneSettings.FogEnabled;


            // TODO
            // 太陽が見える場合にのみレンズ フレアを描画。

            //----------------------------------------------------------------
            // チャンク マネージャ

            ChunkManager.Update(SceneManager.ActiveCamera.View, SceneManager.ActiveCamera.Projection);

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            SceneManager.BackgroundColor = SceneSettings.CurrentSkyColor;
            SceneManager.Draw(gameTime);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            spriteBatch.Draw(SceneManager.FinalSceneMap, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
