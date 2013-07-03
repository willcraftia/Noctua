#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class SceneManager
    {
        #region Settings

        public sealed class Settings
        {
            // TODO
            // 各種初期設定をここへ。
        }

        #endregion

        #region DistanceComparer

        sealed class DistanceComparer : IComparer<SceneObject>
        {
            public static DistanceComparer Instance = new DistanceComparer();

            public Vector3 EyePosition;

            DistanceComparer() { }

            public int Compare(SceneObject o1, SceneObject o2)
            {
                float d1;
                float d2;
                Vector3.DistanceSquared(ref EyePosition, ref o1.Position, out d1);
                Vector3.DistanceSquared(ref EyePosition, ref o2.Position, out d2);

                if (d1 == d2) return 0;
                return (d1 < d2) ? -1 : 1;
            }
        }

        #endregion

        #region SceneCameraCollection

        public sealed class SceneCameraCollection : KeyedCollection<string, SceneCamera>
        {
            internal SceneCameraCollection() { }

            protected override string GetKeyForItem(SceneCamera item)
            {
                return item.Name;
            }
        }

        #endregion

        #region DirectionalLightCollection

        public sealed class DirectionalLightCollection : KeyedCollection<string, DirectionalLight>
        {
            internal DirectionalLightCollection() { }

            protected override string GetKeyForItem(DirectionalLight item)
            {
                return item.Name;
            }
        }

        #endregion

        #region ParticleSystemCollection

        public sealed class ParticleSystemCollection : KeyedCollection<string, ParticleSystem>
        {
            internal ParticleSystemCollection() { }

            protected override string GetKeyForItem(ParticleSystem item)
            {
                return item.Name;
            }
        }

        #endregion

        #region PostprocessSetupCollection

        public sealed class PostprocessSetupCollection : Collection<PostprocessSetup>
        {
            internal PostprocessSetupCollection() { }
        }

        #endregion

        #region LightSceneMapCollection

        public sealed class LightSceneMapCollection : Collection<ShaderResourceView>
        {
            internal LightSceneMapCollection() { }
        }

        #endregion

        #region LightPassCollection

        public sealed class LightPassCollection : Collection<SceneLightPass>
        {
            internal LightPassCollection() { }
        }

        #endregion

        const int InitialSceneObjectCapacity = 300;

        /// <summary>
        /// 八分木マネージャ。
        /// </summary>
        OctreeManager octreeManager;

        /// <summary>
        /// アクティブ カメラの名前。
        /// </summary>
        string activeCameraName;

        /// <summary>
        /// アクティブ カメラ。
        /// </summary>
        SceneCamera activeCamera;

        /// <summary>
        /// 環境光色。
        /// </summary>
        Vector3 ambientLightColor;

        /// <summary>
        /// アクティブ方向性光源の名前。
        /// </summary>
        string activeDirectionalLightName;

        /// <summary>
        /// アクティブ方向性光源。
        /// </summary>
        DirectionalLight activeDirectionalLight;

        /// <summary>
        /// スプライト バッチ。
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// フルスクリーン クワッド。
        /// </summary>
        FullScreenQuad fullScreenQuad;

        Settings settings;

        /// <summary>
        /// 描画対象の不透明オブジェクトのリスト。
        /// </summary>
        List<SceneObject> opaqueObjects;

        /// <summary>
        /// 描画対象の半透明オブジェクトのリスト。
        /// </summary>
        List<SceneObject> translucentObjects;

        /// <summary>
        /// オブジェクト収集メソッド。
        /// </summary>
        Action<Octree> collectObjectsMethod;

        /// <summary>
        /// シーン領域。
        /// </summary>
        BoundingBox sceneBox;

        /// <summary>
        /// 視錐台カリング八分木用クエリ。
        /// </summary>
        BoundingFrustumOctreeQuery frustumOctreeQuery = new BoundingFrustumOctreeQuery();

        /// <summary>
        /// 視錐台カリング八分木用クエリ述語メソッド。
        /// </summary>
        Predicate<Octree> frustumOctreeQueryMethod;

        /// <summary>
        /// 深度ステンシル。
        /// </summary>
        DepthStencil depthStencil;

        /// <summary>
        /// 深度レンダ ターゲット。
        /// </summary>
        RenderTarget depthRenderTarget;

        /// <summary>
        /// 法線レンダ ターゲット。
        /// </summary>
        RenderTarget normalRenderTarget;

        /// <summary>
        /// テクスチャ カラー レンダ ターゲット。
        /// </summary>
        RenderTarget colorRenderTarget;

        /// <summary>
        /// シーン レンダ ターゲット。
        /// </summary>
        RenderTarget sceneRenderTarget;

        /// <summary>
        /// 線形深度マップ エフェクト。
        /// </summary>
        LinearDepthMapEffect depthMapEffect;

        /// <summary>
        /// 法線マップ エフェクト。
        /// </summary>
        NormalMapEffect normalMapEffect;

        /// <summary>
        /// ポストプロセス。
        /// </summary>
        Postprocess postprocess;

        /// <summary>
        /// 閉塞マップ合成フィルタ。
        /// </summary>
        OcclusionCombineFilter occlusionCombineFilter;

        public DeviceContext DeviceContext { get; private set; }

        public BoundingBox SceneBox
        {
            get { return sceneBox; }
        }

        public string ActiveCameraName
        {
            get { return activeCameraName; }
            set
            {
                if (value != null && !Cameras.Contains(value))
                    throw new ArgumentException("Camera not found: " + value);

                if (activeCameraName == value) return;

                activeCameraName = value;
                activeCamera = (activeCameraName != null) ? Cameras[activeCameraName] : null;
            }
        }

        /// <summary>
        /// アクティブ カメラを取得します。
        /// </summary>
        public SceneCamera ActiveCamera
        {
            get
            {
                if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

                return activeCamera;
            }
        }

        public string ActiveDirectionalLightName
        {
            get { return activeDirectionalLightName; }
            set
            {
                if (value != null && !DirectionalLights.Contains(value))
                    throw new ArgumentException("DirectionalLight not found: " + value);

                if (activeDirectionalLightName == value) return;

                activeDirectionalLightName = value;
                activeDirectionalLight = (activeDirectionalLightName != null) ? DirectionalLights[activeDirectionalLightName] : null;
            }
        }

        /// <summary>
        /// アクティブ方向性光源を取得します。
        /// </summary>
        public DirectionalLight ActiveDirectionalLight
        {
            get { return activeDirectionalLight; }
        }

        public SceneNode RootNode { get; private set; }

        public SceneNode SkySphereNode { get; set; }

        public SceneNode LensFlareNode { get; set; }

        public SceneCameraCollection Cameras { get; private set; }

        public DirectionalLightCollection DirectionalLights { get; private set; }

        public ParticleSystemCollection ParticleSystems { get; private set; }

        public PostprocessSetupCollection PostprocessSetups { get; private set; }

        public LightSceneMapCollection LightSceneMaps { get; private set; }

        public LightPassCollection LightPasses { get; private set; }

        /// <summary>
        /// 深度マップを取得します。
        /// </summary>
        public ShaderResourceView DepthMap
        {
            get { return depthRenderTarget; }
        }

        /// <summary>
        /// 法線マップを取得します。
        /// </summary>
        public ShaderResourceView NormalMap
        {
            get { return normalRenderTarget; }
        }

        /// <summary>
        /// ライト シーン マップを取得または設定します。
        /// </summary>
        public ShaderResourceView FinalLightSceneMap { get; set; }

        // ポストプロセス適用前、ライティング適用後のシーン。
        public ShaderResourceView BaseSceneMap { get; private set; }

        // ポストプロセス適用後のシーン。
        public ShaderResourceView FinalSceneMap { get; private set; }

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; }
        }

        public Vector3 BackgroundColor { get; set; }

        public int SceneObjectCount { get; internal set; }

        public int OccludedSceneObjectCount { get; internal set; }

        public int RenderedSceneObjectCount
        {
            get { return SceneObjectCount - OccludedSceneObjectCount; }
        }

        public SceneManager(Settings settings, DeviceContext deviceContext)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            this.settings = settings;
            DeviceContext = deviceContext;

            spriteBatch = new SpriteBatch(DeviceContext);
            fullScreenQuad = new FullScreenQuad(DeviceContext);

            Cameras = new SceneCameraCollection();
            DirectionalLights = new DirectionalLightCollection();
            ParticleSystems = new ParticleSystemCollection();
            PostprocessSetups = new PostprocessSetupCollection();
            opaqueObjects = new List<SceneObject>(InitialSceneObjectCapacity);
            translucentObjects = new List<SceneObject>(InitialSceneObjectCapacity);
            LightSceneMaps = new LightSceneMapCollection();
            LightPasses = new LightPassCollection();
            
            collectObjectsMethod = new Action<Octree>(CollectObjects);
            frustumOctreeQueryMethod = new Predicate<Octree>(frustumOctreeQuery.Contains);

            // TODO
            //
            // MRT 対応も考慮すべき。
            // 非 MRT の場合、深度、法線、シーンの深度ステンシルを共有すべき。

            depthStencil = DeviceContext.Device.CreateDepthStencil();
            depthStencil.Width = DeviceContext.Device.BackBufferWidth;
            depthStencil.Height = DeviceContext.Device.BackBufferHeight;
            depthStencil.Format = SurfaceFormat.Depth24Stencil8;
            depthStencil.Initialize();

            // レンダ ターゲット

            // 深度。
            depthRenderTarget = DeviceContext.Device.CreateRenderTarget();
            depthRenderTarget.Width = DeviceContext.Device.BackBufferWidth;
            depthRenderTarget.Height = DeviceContext.Device.BackBufferHeight;
            depthRenderTarget.Format = SurfaceFormat.Single;
            depthRenderTarget.DepthStencilEnabled = true;
            depthRenderTarget.Initialize();

            // 法線。
            normalRenderTarget = DeviceContext.Device.CreateRenderTarget();
            normalRenderTarget.Width = DeviceContext.Device.BackBufferWidth;
            normalRenderTarget.Height = DeviceContext.Device.BackBufferHeight;
            normalRenderTarget.Format = SurfaceFormat.NormalizedByte4;
            normalRenderTarget.DepthStencilEnabled = true;
            normalRenderTarget.Initialize();

            // テクスチャ カラー。
            colorRenderTarget = DeviceContext.Device.CreateRenderTarget();
            colorRenderTarget.Width = DeviceContext.Device.BackBufferWidth;
            colorRenderTarget.Height = DeviceContext.Device.BackBufferHeight;
            colorRenderTarget.Format = DeviceContext.Device.BackBufferFormat;
            colorRenderTarget.PreferredMultisampleCount = DeviceContext.Device.BackBufferMultisampleCount;
            colorRenderTarget.DepthStencilEnabled = true;
            colorRenderTarget.DepthStencilFormat = DeviceContext.Device.BackBufferDepthStencilFormat;
            colorRenderTarget.Initialize();

            // シーン。
            sceneRenderTarget = DeviceContext.Device.CreateRenderTarget();
            sceneRenderTarget.Width = colorRenderTarget.Width;
            sceneRenderTarget.Height = colorRenderTarget.Height;
            sceneRenderTarget.Format = colorRenderTarget.Format;
            sceneRenderTarget.Initialize();

            // エフェクト。
            depthMapEffect = new LinearDepthMapEffect(DeviceContext);
            normalMapEffect = new NormalMapEffect(DeviceContext);

            // ポストプロセス。
            postprocess = new Postprocess(DeviceContext);
            postprocess.Width = colorRenderTarget.Width;
            postprocess.Height = colorRenderTarget.Height;
            postprocess.Format = colorRenderTarget.Format;

            // ライト シーンとテクスチャ カラーを合成するフィルタ。
            occlusionCombineFilter = new OcclusionCombineFilter(DeviceContext);
            occlusionCombineFilter.ShadowColor = new Vector3(0.5f, 0.5f, 0.5f);

            // TODO
            //
            // サイズを設定ファイルで管理。
            octreeManager = new OctreeManager(new Vector3(256), 3);

            RootNode = new SceneNode(this, "Root");
            SkySphereNode = new SceneNode(this, "SkySphere");
            LensFlareNode = new SceneNode(this, "LensFlare");
        }

        public SceneNode CreateSceneNode(string name)
        {
            return new SceneNode(this, name);
        }

        public void UpdateOctreeSceneNode(SceneNode node)
        {
            octreeManager.Update(node);
        }

        public void RemoveOctreeSceneNode(SceneNode node)
        {
            octreeManager.Remove(node);
        }

        /// <summary>
        /// シーンを描画します。
        /// </summary>
        /// <param name="gameTime">ゲーム時間。</param>
        public void Draw(GameTime gameTime)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");
            if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

            // アクティブ カメラの状態を更新。
            activeCamera.Update();

            // カウンタをリセット。
            SceneObjectCount = 0;
            OccludedSceneObjectCount = 0;

            // クエリ更新。
            Matrix frustumMatrix;
            Matrix.Multiply(ref activeCamera.View, ref activeCamera.Projection, out frustumMatrix);
            frustumOctreeQuery.Matrix = frustumMatrix;

            // 視錐台に含まれるオブジェクトを収集。
            // その際、シーン領域を同時に算出。
            sceneBox = BoundingBox.Empty;
            octreeManager.Execute(frustumOctreeQueryMethod, collectObjectsMethod);

            // 描画対象オブジェクト数を記録。
            SceneObjectCount = opaqueObjects.Count + translucentObjects.Count;

            // TODO
            // 不透明オブジェクトのソートは除外して良いかも。

            // 視点からの距離でソート。
            Matrix.GetViewPosition(ref activeCamera.View, out DistanceComparer.Instance.EyePosition);
            opaqueObjects.Sort(DistanceComparer.Instance);
            translucentObjects.Sort(DistanceComparer.Instance);

            // ライト シーンをリセット。
            LightSceneMaps.Clear();
            FinalLightSceneMap = null;

            // 深度
            DrawDepth();

            // 法線
            DrawNormal();

            // テクスチャ カラー
            DrawTextureColor();

            // 遅延ライティング
            DrawLightedScene();

            //
            // パーティクル
            //

            // TODO

            //if (0 < ParticleSystems.Count)
            //    DrawParticles(gameTime);

            //
            // ポスト プロセス
            //

            ApplyPostprocess();

            // TODO
            // デバック ワイヤフレームの描画はクラス外部で行う。

            //
            // 後処理
            //

            // 分類リストを初期化。
            opaqueObjects.Clear();
            translucentObjects.Clear();
        }

        public void QueryOctree(Predicate<Octree> predicate, Action<Octree> action)
        {
            octreeManager.Execute(predicate, action);
        }

        void CollectObjects(Octree octree)
        {
            if (octree.Nodes.Count == 0)
                return;

            for (int i = 0; i < octree.Nodes.Count; i++)
            {
                var node = octree.Nodes[i];

                if (node.Objects.Count == 0)
                    continue;

                foreach (var obj in node.Objects)
                {
                    // Visible = false は除外。
                    if (!obj.Visible)
                        continue;

                    // 半透明と不透明で分類。
                    if (obj.Translucent)
                    {
                        translucentObjects.Add(obj);
                    }
                    else
                    {
                        opaqueObjects.Add(obj);
                    }

                    // シーン領域へ描画オブジェクト領域を追加。
                    sceneBox.Merge(ref obj.Box);
                }
            }
        }

        void DrawDepth()
        {
            DeviceContext.RasterizerState = null;
            DeviceContext.BlendState = null;
            DeviceContext.DepthStencilState = null;
            DeviceContext.SetRenderTarget(depthRenderTarget);
            DeviceContext.Clear(new Vector4(float.MaxValue));

            depthMapEffect.View = activeCamera.View;
            depthMapEffect.Projection = activeCamera.Projection;

            // 不透明オブジェクト
            for (int i = 0; i < opaqueObjects.Count; i++)
            {
                var opaque = opaqueObjects[i];

                if (!opaque.Occluded)
                    opaque.Draw(depthMapEffect);
            }

            // 半透明オブジェクト
            for (int i = 0; i < translucentObjects.Count; i++)
            {
                var translucent = translucentObjects[i];

                if (!translucent.Occluded)
                    translucent.Draw(depthMapEffect);
            }

            DeviceContext.SetRenderTarget(null);
        }

        void DrawNormal()
        {
            DeviceContext.RasterizerState = null;
            DeviceContext.BlendState = null;
            DeviceContext.DepthStencilState = null;
            DeviceContext.SetRenderTarget(normalRenderTarget);
            DeviceContext.Clear(Vector4.One);

            normalMapEffect.View = activeCamera.View;
            normalMapEffect.Projection = activeCamera.Projection;

            // 不透明オブジェクト
            for (int i = 0; i < opaqueObjects.Count; i++)
            {
                var opaque = opaqueObjects[i];

                if (!opaque.Occluded)
                    opaque.Draw(normalMapEffect);
            }

            // 半透明オブジェクト
            for (int i = 0; i < translucentObjects.Count; i++)
            {
                var translucent = translucentObjects[i];

                if (!translucent.Occluded)
                    translucent.Draw(normalMapEffect);
            }

            DeviceContext.SetRenderTarget(null);
        }

        void DrawTextureColor()
        {
            DeviceContext.RasterizerState = null;
            DeviceContext.BlendState = null;
            DeviceContext.DepthStencilState = null;
            DeviceContext.SetRenderTarget(colorRenderTarget);
            DeviceContext.Clear(new Vector4(BackgroundColor, 1));

            //
            // オクルージョン クエリ
            //

            // TODO
            // タイミングはここで良いの？
            // どこでも良い気はするけど。
            // なお、MRT ならば深度と法線も同時に描画するため、ここで良い気もする。

            for (int i = 0; i < opaqueObjects.Count; i++)
                opaqueObjects[i].UpdateOcclusion();

            for (int i = 0; i < translucentObjects.Count; i++)
                translucentObjects[i].UpdateOcclusion();

            // 不透明オブジェクトの描画
            for (int i = 0; i < opaqueObjects.Count; i++)
            {
                var opaque = opaqueObjects[i];

                if (opaque.Occluded)
                {
                    OccludedSceneObjectCount++;
                    continue;
                }

                opaque.Draw();
            }

            // 半透明オブジェクトの描画
            for (int i = 0; i < translucentObjects.Count; i++)
            {
                var translucent = translucentObjects[i];

                if (translucent.Occluded)
                {
                    OccludedSceneObjectCount++;
                    continue;
                }

                translucent.Draw();
            }

            // スカイ スフィア
            foreach (var obj in SkySphereNode.Objects)
            {
                if (obj.Visible) obj.Draw();
            }

            // レンズ フレア
            foreach (var obj in LensFlareNode.Objects)
            {
                if (obj.Visible) obj.Draw();
            }

            DeviceContext.SetRenderTarget(null);
        }

        void DrawLightedScene()
        {
            if (LightPasses.Count == 0)
            {
                // ライト パスが無いならば、テクスチャ カラーをシーンとする。
                BaseSceneMap = colorRenderTarget;
            }
            else
            {
                // ライト パスがあるならば、パスを処理する。
                for (int i = 0; i < LightPasses.Count; i++)
                {
                    var pass = LightPasses[i];
                    if (!pass.Initialized)
                        pass.Initialize(this);

                    pass.Draw();
                }

                // パスが最後に設定した FinalLightSceneMap を最終ライト シーンとし、
                // これをテクスチャ カラーへ合成し、ポストプロセス適用前のシーンとする。

                DeviceContext.BlendState = null;
                DeviceContext.RasterizerState = null;
                DeviceContext.DepthStencilState = DepthStencilState.None;
                DeviceContext.SetRenderTarget(sceneRenderTarget);

                occlusionCombineFilter.Texture = colorRenderTarget;
                occlusionCombineFilter.OcclusionMap = FinalLightSceneMap;
                occlusionCombineFilter.Apply();
                fullScreenQuad.Draw();

                DeviceContext.SetRenderTarget(null);

                BaseSceneMap = sceneRenderTarget;
            }
        }

        void DrawParticles(GameTime gameTime)
        {
            DeviceContext.SetRenderTarget(colorRenderTarget);

            for (int i = 0; i < ParticleSystems.Count; i++)
            {
                var particleSystem = ParticleSystems[i];
                if (particleSystem.Enabled)
                {
                    particleSystem.View = activeCamera.View;
                    particleSystem.Projection = activeCamera.Projection;
                    particleSystem.Update(gameTime.ElapsedGameTime);
                    particleSystem.Draw();
                }
            }

            DeviceContext.SetRenderTarget(null);
        }

        void ApplyPostprocess()
        {
            foreach (var setup in PostprocessSetups)
            {
                if (!setup.Initialized)
                    setup.Initialize(this);

                setup.Setup(postprocess);
            }

            FinalSceneMap = postprocess.Draw(BaseSceneMap);

            postprocess.Filters.Clear();
        }
    }
}
