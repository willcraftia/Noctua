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

        public const int InitialSceneObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 500;

        /// <summary>
        /// デフォルトのシャドウ マップ サイズ。
        /// </summary>
        public const int DefaultShadowMapSize = 1024;

        /// <summary>
        /// 最大分割数。
        /// </summary>
        const int MaxSplitCount = 3;

        static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        /// <summary>
        /// デフォルトのライト カメラ ビルダ。
        /// </summary>
        static readonly BasicLightCameraBuilder DefaultLightCameraBuilder = new BasicLightCameraBuilder();

        OctreeManager octreeManager;

        SceneNode skySphereNode;

        string activeCameraName;

        SceneCamera activeCamera;

        Vector3 ambientLightColor;

        string activeDirectionalLightName;

        DirectionalLight activeDirectionalLight;

        Vector3 fogColor;

        bool shadowMapAvailable;

        SpriteBatch spriteBatch;

        Postprocess postprocess;

        Settings settings;

        List<SceneObject> opaqueObjects;

        List<SceneObject> translucentObjects;

        List<ShadowCaster> shadowCasters;

        Action<Octree> collectObjectsMethod;

        BoundingBox sceneBox;

        /// <summary>
        /// 分割数。
        /// </summary>
        int splitCount = MaxSplitCount;

        /// <summary>
        /// PSSM 分割機能。
        /// </summary>
        PSSM pssm = new PSSM();

        /// <summary>
        /// 分割された距離の配列。
        /// </summary>
        float[] splitDistances = new float[MaxSplitCount + 1];

        /// <summary>
        /// 分割された射影行列の配列。
        /// </summary>
        Matrix[] splitProjections = new Matrix[MaxSplitCount];

        /// <summary>
        /// 分割されたシャドウ マップの配列。
        /// </summary>
        ShadowMap[] shadowMaps = new ShadowMap[MaxSplitCount];

        /// <summary>
        /// 分割されたライト カメラ空間行列の配列。
        /// </summary>
        Matrix[] lightViewProjections = new Matrix[MaxSplitCount];

        /// <summary>
        /// シャドウ マップ形式。
        /// </summary>
        ShadowMapForm shadowMapForm = ShadowMapForm.Basic;

        /// <summary>
        /// ライト カメラ ビルダ。
        /// </summary>
        LightCameraBuilder lightCameraBuilder = DefaultLightCameraBuilder;

        /// <summary>
        /// シャドウ マップ サイズ。
        /// </summary>
        int shadowMapSize = DefaultShadowMapSize;

        /// <summary>
        /// 投影オブジェクト描画コールバック。
        /// </summary>
        ShadowMap.DrawShadowCastersCallback drawShadowCastersCallback;

        /// <summary>
        /// VSM 用ガウシアン フィルタ。
        /// </summary>
        GaussianFilterSuite vsmGaussianFilterSuite;

        /// <summary>
        /// 深度マップの描画先レンダ ターゲット。
        /// </summary>
        RenderTarget depthMapRenderTarget;

        /// <summary>
        /// 法線マップの描画先レンダ ターゲット。
        /// </summary>
        RenderTarget normalMapRenderTarget;

        /// <summary>
        /// シーンの描画先レンダ ターゲット。
        /// </summary>
        RenderTarget sceneMapRenderTarget;

        /// <summary>
        /// 線形深度マップ エフェクト。
        /// </summary>
        LinearDepthMapEffect depthMapEffect;

        /// <summary>
        /// 法線マップ エフェクト。
        /// </summary>
        NormalMapEffect normalMapEffect;

        public LightCameraBuilder LightCameraBuilder
        {
            get { return lightCameraBuilder; }
            set { lightCameraBuilder = value ?? DefaultLightCameraBuilder; }
        }

        public int ShadowMapSize
        {
            get { return shadowMapSize; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");
                shadowMapSize = value;
            }
        }

        public DeviceContext DeviceContext { get; private set; }

        public SceneCamera ActiveCamera
        {
            get
            {
                if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

                return activeCamera;
            }
        }

        public DirectionalLight ActiveDirectionalLight
        {
            get { return activeDirectionalLight; }
        }

        public SceneNode RootNode { get; private set; }

        public SceneCameraCollection Cameras { get; private set; }

        public DirectionalLightCollection DirectionalLights { get; private set; }

        public ReadOnlyCollection<SceneObject> CurrentOpaqueObjects { get; private set; }

        public ReadOnlyCollection<SceneObject> CurrentTranslucentObjects { get; private set; }

        public ReadOnlyCollection<ShadowCaster> CurrentShadowCasters { get; private set; }

        public ShadowMap ShadowMap { get; set; }

        public ParticleSystemCollection ParticleSystems { get; private set; }

        public Postprocess.FilterCollection PostprocessFilters
        {
            get { return postprocess.Filters; }
        }

        public ShaderResourceView DepthMap
        {
            get { return depthMapRenderTarget.GetShaderResourceView(); }
        }

        public ShaderResourceView NormalMap
        {
            get { return normalMapRenderTarget.GetShaderResourceView(); }
        }

        public ShaderResourceView FinalSceneMap { get; private set; }

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

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; }
        }

        public bool FogEnabled { get; set; }

        public float FogStart { get; set; }

        public float FogEnd { get; set; }

        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }

        public Vector3 BackgroundColor { get; set; }

        public SceneNode SkySphere
        {
            get { return skySphereNode; }
            set { skySphereNode = value; }
        }

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

            Cameras = new SceneCameraCollection();
            DirectionalLights = new DirectionalLightCollection();
            ParticleSystems = new ParticleSystemCollection();

            opaqueObjects = new List<SceneObject>(InitialSceneObjectCapacity);
            translucentObjects = new List<SceneObject>(InitialSceneObjectCapacity);
            shadowCasters = new List<ShadowCaster>(InitialShadowCasterCapacity);
            CurrentOpaqueObjects = new ReadOnlyCollection<SceneObject>(opaqueObjects);
            CurrentTranslucentObjects = new ReadOnlyCollection<SceneObject>(translucentObjects);
            CurrentShadowCasters = new ReadOnlyCollection<ShadowCaster>(shadowCasters);
            
            collectObjectsMethod = new Action<Octree>(CollectObjects);

            var backBuffer = DeviceContext.Device.BackBuffer;

            // TODO
            //
            // MRT でやるべき。あるいは、深度ステンシルを共有すべき。

            // レンダ ターゲット

            // 深度
            depthMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            depthMapRenderTarget.Width = backBuffer.Width;
            depthMapRenderTarget.Height = backBuffer.Height;
            depthMapRenderTarget.Format = SurfaceFormat.Single;
            depthMapRenderTarget.DepthFormat = DepthFormat.Depth24Stencil8;
            depthMapRenderTarget.Initialize();

            // 法線
            normalMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            normalMapRenderTarget.Width = backBuffer.Width;
            normalMapRenderTarget.Height = backBuffer.Height;
            normalMapRenderTarget.Format = SurfaceFormat.NormalizedByte4;
            normalMapRenderTarget.DepthFormat = DepthFormat.Depth24Stencil8;
            normalMapRenderTarget.Initialize();

            // シーン
            sceneMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            sceneMapRenderTarget.Width = backBuffer.Width;
            sceneMapRenderTarget.Height = backBuffer.Height;
            sceneMapRenderTarget.Format = backBuffer.Format;
            sceneMapRenderTarget.MipLevels = backBuffer.MipLevels;
            sceneMapRenderTarget.MultisampleCount = backBuffer.MultisampleCount;
            sceneMapRenderTarget.DepthFormat = backBuffer.DepthFormat;
            //sceneMapRenderTarget.RenderTargetUsage = RenderTargetUsage.Preserve;
            sceneMapRenderTarget.Initialize();

            // エフェクト
            
            depthMapEffect = new LinearDepthMapEffect(DeviceContext.Device);
            normalMapEffect = new NormalMapEffect(DeviceContext.Device);

            // シーンへのポストプロセス

            postprocess = new Postprocess(DeviceContext);
            postprocess.Width = sceneMapRenderTarget.Width;
            postprocess.Height = sceneMapRenderTarget.Height;
            postprocess.Format = sceneMapRenderTarget.Format;
            postprocess.MultisampleCount = sceneMapRenderTarget.MultisampleCount;

            // 投影オブジェクト描画コールバック。
            drawShadowCastersCallback = new ShadowMap.DrawShadowCastersCallback(DrawShadowCasters);

            // TODO
            octreeManager = new OctreeManager(new Vector3(256), 3);

            // TODO
            RootNode = new SceneNode(this, "Root");
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

        public void Draw(GameTime gameTime)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");
            if (activeCamera == null) throw new InvalidOperationException("ActiveCamera is null.");

            // アクティブ カメラの状態を更新。
            activeCamera.Update();

            // カウンタをリセット。
            SceneObjectCount = 0;
            OccludedSceneObjectCount = 0;

            // 視錐台に含まれるオブジェクトを収集。
            // その際、シーン領域を同時に算出。
            sceneBox = BoundingBox.Empty;
            octreeManager.Execute(activeCamera.View, activeCamera.Projection, collectObjectsMethod);

            SceneObjectCount = opaqueObjects.Count + translucentObjects.Count;

            // TODO
            // 不透明オブジェクトのソートは除外して良いかも。

            // 視点からの距離でソート。
            Matrix.GetViewPosition(ref activeCamera.View, out DistanceComparer.Instance.EyePosition);
            opaqueObjects.Sort(DistanceComparer.Instance);
            translucentObjects.Sort(DistanceComparer.Instance);

            //
            // シャドウ マップ
            //

            shadowMapAvailable = false;
            if (ShadowMap != null && shadowCasters.Count != 0 &&
                activeDirectionalLight != null && activeDirectionalLight.Enabled)
            {
                DrawShadowMap();
                shadowMapAvailable = true;
            }

            // 深度
            DrawDepth();

            // 法線
            DrawNormal();

            // シーン
            DrawScene();

            //
            // シャドウ パス
            //

            // TODO

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
            // レンズ フレアはシーン描画後かつポストプロセス前に実行。
            // ただし、これは外部から機能を追加した場合のみとし、
            // その機能追加はレンズ フレア特化ではない汎用性のあるものとする。

            // TODO
            // デバック ワイヤフレームの描画はクラス外部で行う。

            //
            // 後処理
            //

            // 分類リストを初期化。
            opaqueObjects.Clear();
            translucentObjects.Clear();
            shadowCasters.Clear();
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

                    // 投影可か否か。
                    var shadowCaster = obj as ShadowCaster;
                    if (shadowCaster != null)
                    {
                        shadowCasters.Add(shadowCaster);
                    }

                    // シーン領域へ描画オブジェクト領域を追加。
                    sceneBox.Merge(ref obj.Box);
                }
            }
        }

        void DrawShadowMap()
        {
            // PSSM による距離と射影行列の分割。
            pssm.Count = splitCount;
            pssm.Lambda = 0.4f;
            pssm.View = activeCamera.View;
            pssm.Fov = activeCamera.Fov;
            pssm.AspectRatio = activeCamera.AspectRatio;
            pssm.NearClipDistance = activeCamera.NearClipDistance;
            pssm.FarClipDistance = activeCamera.FarClipDistance;
            pssm.SceneBox = sceneBox;
            pssm.Split(splitDistances, splitProjections);

            // ライト カメラ ビルダの状態を更新。
            lightCameraBuilder.EyeView = activeCamera.View;
            lightCameraBuilder.LightDirection = activeDirectionalLight.Direction;
            lightCameraBuilder.SceneBox = sceneBox;

            for (int i = 0; i < splitCount; i++)
            {
                // 射影行列は分割毎に異なる。
                lightCameraBuilder.EyeProjection = splitProjections[i];

                // ライトのビューおよび射影行列の算出。
                Matrix lightView;
                Matrix lightProjection;
                lightCameraBuilder.Build(out lightView, out lightProjection);

                // 後のモデル描画用にライト空間行列を算出。
                Matrix.Multiply(ref lightView, ref lightProjection, out lightViewProjections[i]);

                // シャドウ マップを描画。
                shadowMaps[i].Form = shadowMapForm;
                shadowMaps[i].Size = shadowMapSize;
                shadowMaps[i].Draw(DeviceContext, activeCamera.View, splitProjections[i], lightView, lightProjection, drawShadowCastersCallback);

                // VSM の場合は生成したシャドウ マップへブラーを適用。
                if (shadowMapForm == ShadowMapForm.Variance)
                {
                    if (vsmGaussianFilterSuite == null)
                    {
                        vsmGaussianFilterSuite = new GaussianFilterSuite(
                            DeviceContext,
                            shadowMapSize,
                            shadowMapSize,
                            SurfaceFormat.Vector2);

                        // TODO
                        //
                        // パラメータを外部から設定可能にする。
                        
                        vsmGaussianFilterSuite.Radius = 3;
                        vsmGaussianFilterSuite.Sigma = 1;
                    }

                    vsmGaussianFilterSuite.Filter(shadowMaps[i].RenderTarget, shadowMaps[i].RenderTarget);
                }
            }
        }

        void DrawShadowCasters(Matrix eyeView, Matrix eyeProjection, ShadowMapEffect effect)
        {
            // TODO
            //
            // 視錐台カリングはどうする？
            // 投影オブジェクトは視錐台の外から投影する状態もあるため、
            // 単純な交差判定では意味が無い。
            //
            // ひとまず、負荷を無視し、全ての分割視錐台で全ての投影オブジェクトを描画する。

            for (int i = 0; i < shadowCasters.Count; i++)
            {
                var shadowCaster = shadowCasters[i];
                shadowCaster.Draw(effect);
            }
        }

        void DrawDepth()
        {
            DeviceContext.SetRenderTarget(depthMapRenderTarget);
            DeviceContext.Clear(new Vector4(float.MaxValue));
            DeviceContext.BlendState = BlendState.Opaque;

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
            DeviceContext.SetRenderTarget(normalMapRenderTarget);
            DeviceContext.Clear(Vector4.One);
            DeviceContext.BlendState = BlendState.Opaque;

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

        void DrawScene()
        {
            DeviceContext.SetRenderTarget(sceneMapRenderTarget);
            DeviceContext.Clear(new Vector4(BackgroundColor, 1));

            //
            // オクルージョン クエリ
            //

            // TODO
            // タイミングはここで良いの？
            // どこでも良い気はするけど。
            // なお、MRT ならば深度と法線も同時に描画するため、ここで良い気もする。

            // オクルージョン クエリでは深度ステンシルへ書き込まない。
            DeviceContext.DepthStencilState = DepthStencilState.DepthRead;
            DeviceContext.BlendState = ColorWriteDisable;
            DeviceContext.RasterizerState = RasterizerState.CullNone;

            for (int i = 0; i < opaqueObjects.Count; i++)
                opaqueObjects[i].UpdateOcclusion();

            // TODO
            // 不透明オブジェクトは除外すべきでは？

            //for (int i = 0; i < translucentObjects.Count; i++)
            //    translucentObjects[i].UpdateOcclusion();

            //
            // 不透明オブジェクトの描画
            //

            DeviceContext.DepthStencilState = DepthStencilState.Default;
            DeviceContext.BlendState = BlendState.Opaque;
            DeviceContext.RasterizerState = RasterizerState.CullBack;

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

            //
            // 半透明オブジェクトの描画
            //

            DeviceContext.BlendState = BlendState.AlphaBlend;

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

            // TODO
            //
            // スカイ スフィアに限らず、全ての通常オブジェクトの描画を終えた後に
            // 描画するノードとして汎用的な定義を行い、これに従うようにする。

            //
            // スカイ スフィア
            //

            //foreach (var obj in skySphereNode.Objects)
            //{
            //    if (obj.Visible) obj.Draw();
            //}

            DeviceContext.SetRenderTarget(null);
        }

        void DrawParticles(GameTime gameTime)
        {
            DeviceContext.SetRenderTarget(sceneMapRenderTarget);

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
            FinalSceneMap = postprocess.Draw(sceneMapRenderTarget);
        }
    }
}
