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

        public const int InitialSceneObjectCapacity = 500;

        public const int InitialShadowCasterCapacity = 500;

        /// <summary>
        /// デフォルトのシャドウ マップ サイズ。
        /// </summary>
        public const int DefaultShadowMapSize = 1024;

        /// <summary>
        /// シャドウ マップ最大分割数。
        /// </summary>
        public const int MaxShadowMapSplitCount = 3;

        /// <summary>
        /// デフォルトのライト カメラ ビルダ。
        /// </summary>
        static readonly BasicLightCameraBuilder DefaultLightCameraBuilder = new BasicLightCameraBuilder();

        OctreeManager octreeManager;

        string activeCameraName;

        SceneCamera activeCamera;

        Vector3 ambientLightColor;

        string activeDirectionalLightName;

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

        List<ShadowCaster> shadowCasters;

        Action<Octree> collectObjectsMethod;

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
        /// シャドウ マップ分割数。
        /// </summary>
        int shadowMapSplitCount = MaxShadowMapSplitCount;

        /// <summary>
        /// PSSM 分割機能。
        /// </summary>
        PSSM pssm = new PSSM();

        /// <summary>
        /// 分割された距離の配列。
        /// </summary>
        float[] splitDistances = new float[MaxShadowMapSplitCount + 1];

        /// <summary>
        /// 分割された射影行列の配列。
        /// </summary>
        Matrix[] splitProjections = new Matrix[MaxShadowMapSplitCount];

        /// <summary>
        /// 分割されたシャドウ マップの配列。
        /// </summary>
        ShadowMap[] shadowMaps = new ShadowMap[MaxShadowMapSplitCount];

        /// <summary>
        /// 分割されたシャドウ マップのテクスチャ配列。
        /// </summary>
        ShaderResourceView[] shadowMapResults = new ShaderResourceView[MaxShadowMapSplitCount];

        /// <summary>
        /// 分割されたライト カメラ空間行列の配列。
        /// </summary>
        Matrix[] lightViewProjections = new Matrix[MaxShadowMapSplitCount];

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
        /// 深度マップ用レンダ ターゲット。
        /// </summary>
        RenderTarget depthMapRenderTarget;

        /// <summary>
        /// 法線マップ用レンダ ターゲット。
        /// </summary>
        RenderTarget normalMapRenderTarget;

        /// <summary>
        /// シーン用レンダ ターゲット。
        /// </summary>
        RenderTarget sceneMapRenderTarget;

        /// <summary>
        /// ライティング シーン用レンダ ターゲット。
        /// </summary>
        RenderTarget lightingSceneMapRenderTarget;

        /// <summary>
        /// 閉塞マップ用レンダ ターゲット。
        /// </summary>
        RenderTarget occlusionMapRenderTarget;

        /// <summary>
        /// 環境光閉塞マップの描画先レンダ ターゲット。
        /// </summary>
        RenderTarget ssaoMapRenderTarget;

        /// <summary>
        /// 線形深度マップ エフェクト。
        /// </summary>
        LinearDepthMapEffect depthMapEffect;

        /// <summary>
        /// 法線マップ エフェクト。
        /// </summary>
        NormalMapEffect normalMapEffect;

        /// <summary>
        /// 閉塞マップ用ポストプロセス。
        /// </summary>
        Postprocess occlusionMapPostprocess;

        /// <summary>
        /// 閉塞マップ用ガウシアン フィルタ。
        /// </summary>
        GaussianFilter occlusionMapGaussianFilter;

        /// <summary>
        /// 閉塞マップ用ガウシアン フィルタ水平パス。
        /// </summary>
        GaussianFilterPass occlusionMapGaussianFilterPassH;

        /// <summary>
        /// 閉塞マップ用ガウシアン フィルタ垂直パス。
        /// </summary>
        GaussianFilterPass occlusionMapGaussianFilterPassV;

        /// <summary>
        /// 閉塞マップ用アップ サンプリング フィルタ。
        /// </summary>
        UpFilter occlusionMapUpFilter;

        /// <summary>
        /// 閉塞マップ用ダウン サンプリング フィルタ。
        /// </summary>
        DownFilter occlusionMapDownFilter;

        OcclusionMergeFilter occlusionMergeFilter;

        /// <summary>
        /// シャドウ閉塞マップ機能。
        /// </summary>
        ShadowOcclusionMap shadowOcclusionMap;

        /// <summary>
        /// 環境光閉塞マップ シェーダ。
        /// </summary>
        SSAOMap ssaoMap;

        /// <summary>
        /// 環境光閉塞マップ用ポストプロセス。
        /// </summary>
        Postprocess ssaoMapPostprocess;

        /// <summary>
        /// ダウン フィルタ。
        /// </summary>
        DownFilter ssaoMapDownFilter;

        /// <summary>
        /// アップ フィルタ。
        /// </summary>
        UpFilter ssaoMapUpFilter;

        /// <summary>
        /// 法線深度バイラテラル フィルタ。
        /// </summary>
        NormalDepthBilateralFilter ssaoMapNormalDepthBilateralFilter;

        /// <summary>
        /// 環境光閉塞ブラー フィルタ 水平パス。
        /// </summary>
        GaussianFilterPass ssaoBlurH;

        /// <summary>
        /// 環境光閉塞ブラー フィルタ 垂直パス。
        /// </summary>
        GaussianFilterPass ssaoBlurV;

        int ssaoBlurIteration = 1;

        /// <summary>
        /// 閉塞マップ合成フィルタ。
        /// </summary>
        OcclusionCombineFilter occlusionCombineFilter;

        /// <summary>
        /// シーン用ポストプロセス。
        /// </summary>
        Postprocess scenePostprocess;

        public DeviceContext DeviceContext { get; private set; }

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

        /// <summary>
        /// シャドウ マップ分割数を取得または設定します。
        /// </summary>
        public int ShadowMapSplitCount
        {
            get { return shadowMapSplitCount; }
            set
            {
                if (value < 1 || MaxShadowMapSplitCount < value)
                    throw new ArgumentOutOfRangeException("value");

                shadowMapSplitCount = value;
            }
        }

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

        public SceneNode SkySphereNode { get; set; }

        public SceneNode LensFlareNode { get; set; }

        public SceneCameraCollection Cameras { get; private set; }

        public DirectionalLightCollection DirectionalLights { get; private set; }

        public ReadOnlyCollection<SceneObject> CurrentOpaqueObjects { get; private set; }

        public ReadOnlyCollection<SceneObject> CurrentTranslucentObjects { get; private set; }

        public ReadOnlyCollection<ShadowCaster> CurrentShadowCasters { get; private set; }

        public ParticleSystemCollection ParticleSystems { get; private set; }

        public PostprocessSetupCollection PostprocessSetups { get; private set; }

        /// <summary>
        /// 深度マップを取得します。
        /// </summary>
        public ShaderResourceView DepthMap
        {
            get { return depthMapRenderTarget; }
        }

        /// <summary>
        /// 法線マップを取得します。
        /// </summary>
        public ShaderResourceView NormalMap
        {
            get { return normalMapRenderTarget; }
        }

        // シャドウ マップによるライト閉塞マップ。
        public ShaderResourceView ShadowOcclusionMap
        {
            get { return occlusionMapRenderTarget; }
        }

        // ブラー適用前の環境光閉塞マップ。
        public ShaderResourceView AmbientOcclusionMap
        {
            get { return ssaoMapRenderTarget; }
        }

        // ブラー適用後の環境光閉塞マップ。
        public ShaderResourceView FinalAmbientOcclusionMap { get; private set; }

        // ライト閉塞マップと環境光閉塞マップの合成後のライト閉塞マップ。
        public ShaderResourceView FinalOcclusionMap { get; private set; }

        // ポストプロセス適用前、ライティング適用後のシーン。
        public ShaderResourceView BaseSceneMap { get; private set; }

        // ポストプロセス適用後のシーン。
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
            shadowCasters = new List<ShadowCaster>(InitialShadowCasterCapacity);
            CurrentOpaqueObjects = new ReadOnlyCollection<SceneObject>(opaqueObjects);
            CurrentTranslucentObjects = new ReadOnlyCollection<SceneObject>(translucentObjects);
            CurrentShadowCasters = new ReadOnlyCollection<ShadowCaster>(shadowCasters);
            
            collectObjectsMethod = new Action<Octree>(CollectObjects);
            frustumOctreeQueryMethod = new Predicate<Octree>(frustumOctreeQuery.Contains);

            var backBuffer = DeviceContext.Device.BackBuffer;

            // TODO
            //
            // MRT でやるべき。あるいは、深度ステンシルを共有すべき。

            // レンダ ターゲット

            // 深度。
            depthMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            depthMapRenderTarget.Width = backBuffer.Width;
            depthMapRenderTarget.Height = backBuffer.Height;
            depthMapRenderTarget.Format = SurfaceFormat.Single;
            depthMapRenderTarget.DepthFormat = DepthFormat.Depth24Stencil8;
            depthMapRenderTarget.Initialize();

            // 法線。
            normalMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            normalMapRenderTarget.Width = backBuffer.Width;
            normalMapRenderTarget.Height = backBuffer.Height;
            normalMapRenderTarget.Format = SurfaceFormat.NormalizedByte4;
            normalMapRenderTarget.DepthFormat = DepthFormat.Depth24Stencil8;
            normalMapRenderTarget.Initialize();

            // シーン。
            sceneMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            sceneMapRenderTarget.Width = backBuffer.Width;
            sceneMapRenderTarget.Height = backBuffer.Height;
            sceneMapRenderTarget.Format = backBuffer.Format;
            sceneMapRenderTarget.MipLevels = backBuffer.MipLevels;
            sceneMapRenderTarget.PreferredMultisampleCount = backBuffer.MultisampleCount;
            sceneMapRenderTarget.DepthFormat = backBuffer.DepthFormat;
            sceneMapRenderTarget.Initialize();

            // ライティング シーン。
            lightingSceneMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            lightingSceneMapRenderTarget.Width = sceneMapRenderTarget.Width;
            lightingSceneMapRenderTarget.Height = sceneMapRenderTarget.Height;
            lightingSceneMapRenderTarget.Format = sceneMapRenderTarget.Format;
            lightingSceneMapRenderTarget.MipLevels = sceneMapRenderTarget.MipLevels;
            lightingSceneMapRenderTarget.PreferredMultisampleCount = sceneMapRenderTarget.MultisampleCount;
            lightingSceneMapRenderTarget.DepthFormat = sceneMapRenderTarget.DepthFormat;
            lightingSceneMapRenderTarget.Initialize();

            // 閉塞マップ。
            occlusionMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            occlusionMapRenderTarget.Width = backBuffer.Width;
            occlusionMapRenderTarget.Height = backBuffer.Height;
            occlusionMapRenderTarget.Format = SurfaceFormat.Single;
            occlusionMapRenderTarget.Initialize();

            // 環境光閉塞マップ。
            ssaoMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            ssaoMapRenderTarget.Width = backBuffer.Width;
            ssaoMapRenderTarget.Height = backBuffer.Height;
            ssaoMapRenderTarget.Format = SurfaceFormat.Single;
            ssaoMapRenderTarget.DepthFormat = DepthFormat.Depth24Stencil8;
            ssaoMapRenderTarget.Initialize();

            // エフェクト。
            depthMapEffect = new LinearDepthMapEffect(DeviceContext.Device);
            normalMapEffect = new NormalMapEffect(DeviceContext.Device);

            // TODO
            //
            // オンデマンド生成にする。

            // 閉塞マップ ポストプロセス。
            occlusionMapPostprocess = new Postprocess(DeviceContext);
            occlusionMapPostprocess.Width = occlusionMapRenderTarget.Width;
            occlusionMapPostprocess.Height = occlusionMapRenderTarget.Height;
            occlusionMapPostprocess.Format = occlusionMapRenderTarget.Format;
            occlusionMapPostprocess.PreferredMultisampleCount = occlusionMapRenderTarget.MultisampleCount;

            // 範囲と標準偏差に適した値は、アップ/ダウン サンプリングを伴うか否かで大きく異なる。
            occlusionMapGaussianFilter = new GaussianFilter(DeviceContext.Device);
            occlusionMapGaussianFilter.Radius = 3;
            occlusionMapGaussianFilter.Sigma = 1;
            occlusionMapGaussianFilterPassH = new GaussianFilterPass(occlusionMapGaussianFilter, GaussianFilterDirection.Horizon);
            occlusionMapGaussianFilterPassV = new GaussianFilterPass(occlusionMapGaussianFilter, GaussianFilterDirection.Vertical);

            occlusionMapUpFilter = new UpFilter(DeviceContext.Device);
            occlusionMapDownFilter = new DownFilter(DeviceContext.Device);

            occlusionMergeFilter = new OcclusionMergeFilter(DeviceContext.Device);

            occlusionMapPostprocess.Filters.Add(occlusionMapDownFilter);
            occlusionMapPostprocess.Filters.Add(occlusionMapGaussianFilterPassH);
            occlusionMapPostprocess.Filters.Add(occlusionMapGaussianFilterPassV);
            occlusionMapPostprocess.Filters.Add(occlusionMapUpFilter);
            //occlusionMapPostprocess.Filters.Add(occlusionMergeFilter);
            occlusionMapPostprocess.Enabled = true;

            // 深度バイアスは、主に PCF の際に重要となる。
            // VSM の場合、あまり意味は無い。
            shadowOcclusionMap = new ShadowOcclusionMap(DeviceContext);
            shadowOcclusionMap.SplitCount = shadowMapSplitCount;
            shadowOcclusionMap.PcfEnabled = false;

            ssaoMap = new SSAOMap(DeviceContext);

            // 環境光閉塞マップ用ポストプロセス。
            ssaoMapPostprocess = new Postprocess(DeviceContext);
            ssaoMapPostprocess.Width = ssaoMapRenderTarget.Width;
            ssaoMapPostprocess.Height = ssaoMapRenderTarget.Height;
            ssaoMapPostprocess.Format = SurfaceFormat.Single;

            ssaoMapDownFilter = new DownFilter(DeviceContext.Device);
            ssaoMapUpFilter = new UpFilter(DeviceContext.Device);

            ssaoMapNormalDepthBilateralFilter = new NormalDepthBilateralFilter(DeviceContext.Device);
            ssaoBlurH = new GaussianFilterPass(ssaoMapNormalDepthBilateralFilter, GaussianFilterDirection.Horizon);
            ssaoBlurV = new GaussianFilterPass(ssaoMapNormalDepthBilateralFilter, GaussianFilterDirection.Vertical);

            // シーン用ポストプロセス。
            scenePostprocess = new Postprocess(DeviceContext);
            scenePostprocess.Width = sceneMapRenderTarget.Width;
            scenePostprocess.Height = sceneMapRenderTarget.Height;
            scenePostprocess.Format = sceneMapRenderTarget.Format;
            scenePostprocess.PreferredMultisampleCount = sceneMapRenderTarget.MultisampleCount;

            occlusionCombineFilter = new OcclusionCombineFilter(DeviceContext.Device);
            occlusionCombineFilter.ShadowColor = new Vector3(0.5f, 0.5f, 0.5f);

            // 投影オブジェクト描画コールバック。
            drawShadowCastersCallback = new ShadowMap.DrawShadowCastersCallback(DrawShadowCasters);

            // TODO
            octreeManager = new OctreeManager(new Vector3(256), 3);

            // TODO
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
        /// シャドウ マップを取得します。
        /// シャドウ マップを描画しない場合、あるいは、
        /// 指定のインデックスで描画していない場合は null を返します。
        /// </summary>
        /// <param name="index">シャドウ マップのインデックス。</param>
        /// <returns>シャドウ マップ。</returns>
        public ShaderResourceView GetShadowMap(int index)
        {
            if ((uint) MaxShadowMapSplitCount <= (uint) index)
                throw new ArgumentOutOfRangeException("index");

            if (shadowMaps[index] == null)
                return null;

            return shadowMaps[index].RenderTarget;
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

            // クエリ更新。
            Matrix frustumMatrix;
            Matrix.Multiply(ref activeCamera.View, ref activeCamera.Projection, out frustumMatrix);
            frustumOctreeQuery.Matrix = frustumMatrix;

            // 視錐台に含まれるオブジェクトを収集。
            // その際、シーン領域を同時に算出。
            sceneBox = BoundingBox.Empty;
            octreeManager.Execute(frustumOctreeQueryMethod, collectObjectsMethod);

            SceneObjectCount = opaqueObjects.Count + translucentObjects.Count;

            // TODO
            // 不透明オブジェクトのソートは除外して良いかも。

            // 視点からの距離でソート。
            Matrix.GetViewPosition(ref activeCamera.View, out DistanceComparer.Instance.EyePosition);
            opaqueObjects.Sort(DistanceComparer.Instance);
            translucentObjects.Sort(DistanceComparer.Instance);

            // シャドウ マップ
            DrawShadowMap();

            // 深度
            DrawDepth();

            // 法線
            DrawNormal();

            // シーン
            DrawScene();

            // シャドウ パス
            DrawAmbientOcclusion();
            DrawShadowOcclusion();
            DrawSceneShadow();

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
            shadowCasters.Clear();
        }

        BoundingFrustum lightFrustum = new BoundingFrustum(Matrix.Identity);

        Vector3[] lightFrustumCorners = new Vector3[BoundingFrustum.CornerCount];

        float shadowSceneExtrudeDistance = 500.0f;

        BoundindBoxOctreeQuery boundindBoxOctreeQuery = new BoundindBoxOctreeQuery();

        void CollectShadowCasters(Matrix lightView, Matrix lightProjection, Vector3 lightDirection)
        {
            Matrix frustumMatrix;
            Matrix.Multiply(ref lightView, ref lightProjection, out frustumMatrix);
            lightFrustum.Matrix = frustumMatrix;
            lightFrustum.GetCorners(lightFrustumCorners);

            var extrusion = lightDirection * (-shadowSceneExtrudeDistance);

            BoundingBox box = BoundingBox.Empty;
            for (int i = 0; i < lightFrustumCorners.Length; i++)
            {
                box.Merge(lightFrustumCorners[i]);
                box.Merge(lightFrustumCorners[i] + extrusion);
            }

            boundindBoxOctreeQuery.Box = box;

            octreeManager.Execute(boundindBoxOctreeQuery.Contains, CollectShadowCasters);
        }

        void CollectShadowCasters(Octree octree)
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

                    // 投影可か否か。
                    var shadowCaster = obj as ShadowCaster;
                    if (shadowCaster != null)
                    {
                        shadowCasters.Add(shadowCaster);
                    }

                    // シーン領域へ描画オブジェクト領域を追加。
                    //sceneBox.Merge(ref obj.Box);
                }
            }
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
                    //var shadowCaster = obj as ShadowCaster;
                    //if (shadowCaster != null)
                    //{
                    //    shadowCasters.Add(shadowCaster);
                    //}

                    // シーン領域へ描画オブジェクト領域を追加。
                    sceneBox.Merge(ref obj.Box);
                }
            }
        }

        void DrawShadowMap()
        {
            // シャドウ マップ描画結果のクリア。
            Array.Clear(shadowMapResults, 0, shadowMapResults.Length);

            if (activeDirectionalLight == null || !activeDirectionalLight.Enabled)
                return;

            // PSSM による距離と射影行列の分割。
            pssm.Count = shadowMapSplitCount;
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

            for (int i = 0; i < shadowMapSplitCount; i++)
            {
                // 必要となった場合にシャドウ マップ オブジェクトを生成。
                if (shadowMaps[i] == null)
                {
                    shadowMaps[i] = new ShadowMap(DeviceContext);
                }

                // 射影行列は分割毎に異なる。
                lightCameraBuilder.EyeProjection = splitProjections[i];

                // ライトのビューおよび射影行列の算出。
                Matrix lightView;
                Matrix lightProjection;
                lightCameraBuilder.Build(out lightView, out lightProjection);

                // 後のモデル描画用にライト空間行列を算出。
                Matrix.Multiply(ref lightView, ref lightProjection, out lightViewProjections[i]);

                // TODO
                //
                // ライト空間行列からライトの視錐台を構築し、
                // 視錐台から構築した AABB に含まれる投影オブジェクトを検索し、
                // それらの深度をシャドウ マップへ描画する。

                // ライト領域に含まれる投影オブジェクトを収集。
                CollectShadowCasters(lightView, lightProjection, activeDirectionalLight.Direction);

                DeviceContext.RasterizerState = RasterizerState.CullNone;

                // シャドウ マップを描画。
                shadowMaps[i].Form = shadowMapForm;
                shadowMaps[i].Size = shadowMapSize;
                shadowMaps[i].Draw(activeCamera.View, splitProjections[i], lightView, lightProjection, drawShadowCastersCallback);

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

                // 描画結果を配列へ格納。
                shadowMapResults[i] = shadowMaps[i].RenderTarget;

                shadowCasters.Clear();
            }
        }

        void DrawShadowCasters(Matrix eyeView, Matrix eyeProjection, ShadowMapEffect effect)
        {
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

            for (int i = 0; i < opaqueObjects.Count; i++)
                opaqueObjects[i].UpdateOcclusion();

            for (int i = 0; i < translucentObjects.Count; i++)
                translucentObjects[i].UpdateOcclusion();

            //
            // 不透明オブジェクトの描画
            //

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

            //
            // スカイ スフィア
            //

            foreach (var obj in SkySphereNode.Objects)
            {
                if (obj.Visible) obj.Draw();
            }

            //
            // レンズ フレア
            //

            foreach (var obj in LensFlareNode.Objects)
            {
                if (obj.Visible) obj.Draw();
            }

            DeviceContext.SetRenderTarget(null);
        }

        void DrawShadowOcclusion()
        {
            DeviceContext.SetRenderTarget(occlusionMapRenderTarget);
            DeviceContext.Clear(Vector4.One);

            if (activeDirectionalLight != null && activeDirectionalLight.Enabled)
            {
                shadowOcclusionMap.ShadowMapForm = shadowMapForm;
                shadowOcclusionMap.View = activeCamera.View;
                shadowOcclusionMap.Projection = activeCamera.Projection;

                for (int i = 0; i < MaxShadowMapSplitCount; i++)
                {
                    shadowOcclusionMap.SetSplitDistance(i, splitDistances[i]);
                    shadowOcclusionMap.SetLightViewProjection(i, lightViewProjections[i]);
                    shadowOcclusionMap.SetShadowMap(i, shadowMaps[i].RenderTarget);
                }
                shadowOcclusionMap.SetSplitDistance(MaxShadowMapSplitCount, splitDistances[MaxShadowMapSplitCount]);
                shadowOcclusionMap.LinearDepthMap = depthMapRenderTarget;

                shadowOcclusionMap.Draw();
            }

            DeviceContext.SetRenderTarget(null);

            occlusionMergeFilter.OtherOcclusionMap = FinalAmbientOcclusionMap;
            FinalOcclusionMap = occlusionMapPostprocess.Draw(occlusionMapRenderTarget);
        }

        void DrawAmbientOcclusion()
        {
            DeviceContext.SetRenderTarget(ssaoMapRenderTarget);
            DeviceContext.Clear(Vector4.One);

            ssaoMap.LinearDepthMap = depthMapRenderTarget;
            ssaoMap.NormalMap = normalMapRenderTarget;
            ssaoMap.Projection = activeCamera.Projection;
            ssaoMap.Draw();

            DeviceContext.SetRenderTarget(null);

            ssaoMapPostprocess.Filters.Clear();
            ssaoMapNormalDepthBilateralFilter.LinearDepthMap = depthMapRenderTarget;
            ssaoMapNormalDepthBilateralFilter.NormalMap = normalMapRenderTarget;

            ssaoMapPostprocess.Filters.Add(ssaoMapDownFilter);
            for (int i = 0; i < ssaoBlurIteration; i++)
            {
                ssaoMapPostprocess.Filters.Add(ssaoBlurH);
                ssaoMapPostprocess.Filters.Add(ssaoBlurV);
            }
            ssaoMapPostprocess.Filters.Add(ssaoMapUpFilter);

            FinalAmbientOcclusionMap = ssaoMapPostprocess.Draw(ssaoMapRenderTarget);
        }

        void DrawSceneShadow()
        {
            occlusionCombineFilter.Enabled = false;

            if (!occlusionCombineFilter.Enabled)
            {
                BaseSceneMap = sceneMapRenderTarget;
            }
            else
            {
                DeviceContext.SetRenderTarget(lightingSceneMapRenderTarget);
                DeviceContext.BlendState = null;
                DeviceContext.RasterizerState = null;
                DeviceContext.DepthStencilState = DepthStencilState.None;

                occlusionCombineFilter.Texture = sceneMapRenderTarget;
                occlusionCombineFilter.OcclusionMap = FinalOcclusionMap;
                //occlusionCombineFilter.OcclusionMap = finalAmbientOcclusionMap;

                occlusionCombineFilter.Apply(DeviceContext);
                fullScreenQuad.Draw();

                DeviceContext.SetRenderTarget(null);

                BaseSceneMap = lightingSceneMapRenderTarget;
            }
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
            foreach (var setup in PostprocessSetups)
            {
                if (!setup.Initialized)
                    setup.Initialize(this);

                setup.Setup(scenePostprocess);
            }

            FinalSceneMap = scenePostprocess.Draw(BaseSceneMap);

            scenePostprocess.Filters.Clear();
        }
    }
}
