#region Using

using System;
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
    public sealed class WorldManager
    {
        SpriteBatch spriteBatch;

        ResourceManager resourceManager = new ResourceManager();

        AssetContainer assetContainer;

        SceneCamera defaultCamera = new SceneCamera("Default");

        /// <summary>
        /// 線形フォグ ポストプロセス設定。
        /// </summary>
        LinearFogSetup linearFogSetup;

        /// <summary>
        /// 被写界深度合成ポストプロセス設定。
        /// </summary>
        DofSetup dofSetup;

        public DeviceContext DeviceContext { get; private set; }

        public SceneManager SceneManager { get; private set; }

        public SceneManager.Settings SceneManagerSettings { get; private set; }

        public ChunkManager ChunkManager { get; private set; }

        public RegionManager RegionManager { get; private set; }

        public GraphicsSettings GraphicsSettings { get; private set; }

        public SceneSettings SceneSettings { get; private set; }

        internal IObjectSerializer ObjectSerializer { get; private set; }

        public WorldManager(DeviceContext deviceContext)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            DeviceContext = deviceContext;

            spriteBatch = new SpriteBatch(deviceContext);

            ObjectSerializer = XmlObjectSerializer.Instance;

            resourceManager.Register<TitleResourceLoader>();

            assetContainer = new AssetContainer(deviceContext, resourceManager, ObjectSerializer);
            assetContainer.RegisterAssetSerializer<GraphicsSettingsSerializer>();
            assetContainer.RegisterAssetSerializer<SceneSettingsSerializer>();
            assetContainer.RegisterAssetSerializer<ChunkSettingsSerializer>();

            GraphicsSettings = assetContainer.Load<GraphicsSettings>("title:Assets/GraphicsSettings.xml");
            SceneSettings = assetContainer.Load<SceneSettings>("title:Assets/SceneSettings.xml");
            // TODO: リソースから取得する。
            SceneManagerSettings = new SceneManager.Settings();

            //----------------------------------------------------------------
            // シーン マネージャ

            SceneManager = new SceneManager(SceneManagerSettings, DeviceContext);

            // 太陽と月をディレクショナル ライトとして登録。
            SceneManager.DirectionalLights.Add(SceneSettings.Sunlight);
            SceneManager.DirectionalLights.Add(SceneSettings.Moonlight);

            if (SceneSettings.FogEnabled)
            {
                linearFogSetup = new LinearFogSetup(DeviceContext.Device);
                SceneManager.PostprocessSetups.Add(linearFogSetup);
            }

            // TODO
            // DoF の ON/OFF はどこでやる？
            dofSetup = new DofSetup(DeviceContext.Device);
            dofSetup.BlurResolution = GraphicsDofSettings.BlurResolutions[GraphicsSettings.Dof.BlurResolution];
            dofSetup.BlurRadius = GraphicsSettings.Dof.BlurRadius;
            dofSetup.BlurSigma = GraphicsSettings.Dof.BlurSigma;
            SceneManager.PostprocessSetups.Add(dofSetup);

            //----------------------------------------------------------------
            // リージョン マネージャ

            RegionManager = new RegionManager(this);

            //----------------------------------------------------------------
            // チャンク マネージャ

            var chunkSettings = assetContainer.Load<ChunkSettings>("title:Assets/ChunkSettings.xml");
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

        public void Load(string worldUri)
        {
            RegionManager.LoadRegion(worldUri);
        }

        public void Unload()
        {
            // TODO
        }

        public void Update(GameTime gameTime)
        {
            if (ChunkManager.Closed)
            {
                Closing = false;
                Closed = true;
                return;
            }

            var camera = SceneManager.ActiveCamera;

            //----------------------------------------------------------------
            // カメラ更新

            camera.Update();

            //----------------------------------------------------------------
            // シーン設定

            SceneSettings.Update(gameTime);

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
                var far = camera.FarClipDistance;

                linearFogSetup.FogStart = far * SceneSettings.FogStart;
                linearFogSetup.FogEnd = far * SceneSettings.FogEnd;
                linearFogSetup.FogColor = SceneSettings.CurrentSkyColor;
                linearFogSetup.FarClipDistance = far;
            }

            // TODO
            //
            // ON/OFF
            {
                dofSetup.FocusDistance = camera.FocusDistance;
                dofSetup.FocusRange = camera.FocusRange;
            }

            //----------------------------------------------------------------
            // チャンク マネージャ

            ChunkManager.Update(camera.View, camera.Projection);

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

        public bool Closing { get; private set; }

        public bool Closed { get; private set; }

        public void Close()
        {
            if (!ChunkManager.Closing && !ChunkManager.Closed)
            {
                ChunkManager.Close();

                Closing = true;
            }
        }
    }
}
