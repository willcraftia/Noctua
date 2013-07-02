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

        /// <summary>
        /// シャドウ マップ最大分割数。
        /// </summary>
        public const int MaxShadowMapSplitCount = 3;

        const int InitialSceneObjectCapacity = 300;

        const int InitialShadowCasterCapacity = 300;

        /// <summary>
        /// デフォルトのライト カメラ ビルダ。
        /// </summary>
        static readonly BasicLightCameraBuilder DefaultLightCameraBuilder = new BasicLightCameraBuilder();

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
        /// 投影オブジェクトのリスト。
        /// </summary>
        List<ShadowCaster> shadowCasters;

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
        int shadowMapSize = 1024;

        /// <summary>
        /// ライト カメラ視錐台。
        /// </summary>
        BoundingFrustum lightFrustum = new BoundingFrustum(Matrix.Identity);

        /// <summary>
        /// ライト カメラ視錐台頂点配列。
        /// </summary>
        Vector3[] lightFrustumCorners = new Vector3[BoundingFrustum.CornerCount];

        /// <summary>
        /// 境界ボックスによる八分木クエリ。
        /// </summary>
        BoundindBoxOctreeQuery boundindBoxOctreeQuery = new BoundindBoxOctreeQuery();

        /// <summary>
        /// 投影オブジェクト検索のための八分木クエリ関数。
        /// </summary>
        Predicate<Octree> queryShadowCasterMethod;

        /// <summary>
        /// 投影オブジェクト収集メソッド。
        /// </summary>
        Action<Octree> collectShadowCasterMethod;

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
        /// 光閉塞マップ用レンダ ターゲット。
        /// </summary>
        RenderTarget lightOcclusionMapRenderTarget;

        /// <summary>
        /// 線形深度マップ エフェクト。
        /// </summary>
        LinearDepthMapEffect depthMapEffect;

        /// <summary>
        /// 法線マップ エフェクト。
        /// </summary>
        NormalMapEffect normalMapEffect;

        /// <summary>
        /// 光閉塞マップ用ポストプロセス。
        /// </summary>
        Postprocess lightOcclusionMapPostprocess;

        /// <summary>
        /// 光閉塞マップ用ガウシアン フィルタ。
        /// </summary>
        GaussianFilter lightOcclusionMapGaussianFilter;

        /// <summary>
        /// 光閉塞マップ用ガウシアン フィルタ水平パス。
        /// </summary>
        GaussianFilterPass lightOcclusionMapGaussianFilterPassH;

        /// <summary>
        /// 光閉塞マップ用ガウシアン フィルタ垂直パス。
        /// </summary>
        GaussianFilterPass lightOcclusionMapGaussianFilterPassV;

        /// <summary>
        /// 光閉塞マップ用アップ サンプリング フィルタ。
        /// </summary>
        UpFilter lightOcclusionMapUpFilter;

        /// <summary>
        /// 光閉塞マップ用ダウン サンプリング フィルタ。
        /// </summary>
        DownFilter lightOcclusionMapDownFilter;

        /// <summary>
        /// 光閉塞マップ結合フィルタ。
        /// </summary>
        OcclusionMergeFilter occlusionMergeFilter;

        /// <summary>
        /// シャドウ閉塞マップ機能。
        /// </summary>
        ShadowOcclusionMap shadowOcclusionMap;

        /// <summary>
        /// 環境光閉塞マップ。
        /// </summary>
        SSAOMap ssaoMap;

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
            get { return pssm.Count; }
            set
            {
                if (value < 1 || MaxShadowMapSplitCount < value) throw new ArgumentOutOfRangeException("value");

                pssm.Count = value;
            }
        }

        /// <summary>
        /// シャドウ マップ分割ラムダ値を取得または設定します。
        /// </summary>
        public float ShadowMapSplitLambda
        {
            get { return pssm.Lambda; }
            set { pssm.Lambda = value; }
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
            get { return lightOcclusionMapRenderTarget; }
        }

        // ブラー適用前の環境光閉塞マップ。
        public ShaderResourceView BaseAmbientOcclusionMap
        {
            get { return ssaoMap.BaseTexture; }
        }

        // ブラー適用後の環境光閉塞マップ。
        public ShaderResourceView FinalAmbientOcclusionMap
        {
            get { return ssaoMap.FinalTexture; }
        }

        // ライト閉塞マップと環境光閉塞マップの合成後のライト閉塞マップ。
        public ShaderResourceView FinalOcclusionMap { get; private set; }

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

            // 光閉塞マップ。
            lightOcclusionMapRenderTarget = DeviceContext.Device.CreateRenderTarget();
            lightOcclusionMapRenderTarget.Width = backBuffer.Width;
            lightOcclusionMapRenderTarget.Height = backBuffer.Height;
            lightOcclusionMapRenderTarget.Format = SurfaceFormat.Single;
            lightOcclusionMapRenderTarget.Initialize();

            // エフェクト。
            depthMapEffect = new LinearDepthMapEffect(DeviceContext);
            normalMapEffect = new NormalMapEffect(DeviceContext);

            // TODO
            //
            // オンデマンド生成にする。

            // 閉塞マップ ポストプロセス。
            lightOcclusionMapPostprocess = new Postprocess(DeviceContext);
            lightOcclusionMapPostprocess.Width = lightOcclusionMapRenderTarget.Width;
            lightOcclusionMapPostprocess.Height = lightOcclusionMapRenderTarget.Height;
            lightOcclusionMapPostprocess.Format = lightOcclusionMapRenderTarget.Format;
            lightOcclusionMapPostprocess.PreferredMultisampleCount = lightOcclusionMapRenderTarget.MultisampleCount;

            // 範囲と標準偏差に適した値は、アップ/ダウン サンプリングを伴うか否かで大きく異なる。
            lightOcclusionMapGaussianFilter = new GaussianFilter(DeviceContext);
            lightOcclusionMapGaussianFilter.Radius = 3;
            lightOcclusionMapGaussianFilter.Sigma = 1;
            lightOcclusionMapGaussianFilterPassH = new GaussianFilterPass(lightOcclusionMapGaussianFilter, GaussianFilterDirection.Horizon);
            lightOcclusionMapGaussianFilterPassV = new GaussianFilterPass(lightOcclusionMapGaussianFilter, GaussianFilterDirection.Vertical);

            lightOcclusionMapUpFilter = new UpFilter(DeviceContext);
            lightOcclusionMapDownFilter = new DownFilter(DeviceContext);

            occlusionMergeFilter = new OcclusionMergeFilter(DeviceContext);

            lightOcclusionMapPostprocess.Filters.Add(lightOcclusionMapDownFilter);
            lightOcclusionMapPostprocess.Filters.Add(lightOcclusionMapGaussianFilterPassH);
            lightOcclusionMapPostprocess.Filters.Add(lightOcclusionMapGaussianFilterPassV);
            lightOcclusionMapPostprocess.Filters.Add(lightOcclusionMapUpFilter);
            lightOcclusionMapPostprocess.Filters.Add(occlusionMergeFilter);
            lightOcclusionMapPostprocess.Enabled = true;

            // 深度バイアスは、主に PCF の際に重要となる。
            // VSM の場合、あまり意味は無い。
            shadowOcclusionMap = new ShadowOcclusionMap(DeviceContext);
            shadowOcclusionMap.SplitCount = pssm.Count;
            shadowOcclusionMap.PcfEnabled = false;

            ssaoMap = new SSAOMap(DeviceContext);
            ssaoMap.RenderTargetWidth = backBuffer.Width;
            ssaoMap.RenderTargetHeight = backBuffer.Height;
            ssaoMap.BlurScale = 0.25f;

            // シーン用ポストプロセス。
            scenePostprocess = new Postprocess(DeviceContext);
            scenePostprocess.Width = sceneMapRenderTarget.Width;
            scenePostprocess.Height = sceneMapRenderTarget.Height;
            scenePostprocess.Format = sceneMapRenderTarget.Format;
            scenePostprocess.PreferredMultisampleCount = sceneMapRenderTarget.MultisampleCount;

            occlusionCombineFilter = new OcclusionCombineFilter(DeviceContext);
            occlusionCombineFilter.ShadowColor = new Vector3(0.5f, 0.5f, 0.5f);

            // TODO
            octreeManager = new OctreeManager(new Vector3(256), 3);

            // TODO
            RootNode = new SceneNode(this, "Root");

            SkySphereNode = new SceneNode(this, "SkySphere");
            LensFlareNode = new SceneNode(this, "LensFlare");

            queryShadowCasterMethod = new Predicate<Octree>(boundindBoxOctreeQuery.Contains);
            collectShadowCasterMethod = new Action<Octree>(CollectShadowCasters);
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

        void CollectShadowCasters(Matrix lightView, Matrix lightProjection, Vector3 lightDirection)
        {
            Matrix frustumMatrix;
            Matrix.Multiply(ref lightView, ref lightProjection, out frustumMatrix);
            lightFrustum.Matrix = frustumMatrix;
            lightFrustum.GetCorners(lightFrustumCorners);

            BoundingBox box;
            BoundingBox.CreateFromPoints(lightFrustumCorners, out box);

            boundindBoxOctreeQuery.Box = box;

            // ライト カメラに含まれる投影オブジェクトを収集。
            octreeManager.Execute(queryShadowCasterMethod, collectShadowCasterMethod);
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
                    if (shadowCaster != null && shadowCaster.CastShadow)
                    {
                        shadowCasters.Add(shadowCaster);
                    }
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
            pssm.View = activeCamera.View;
            pssm.Fov = activeCamera.Fov;
            pssm.AspectRatio = activeCamera.AspectRatio;
            pssm.NearClipDistance = activeCamera.NearClipDistance;
            // TODO
            //
            // 割合を設定ファイルで管理。
            pssm.FarClipDistance = activeCamera.FarClipDistance * 0.8f;
            pssm.SceneBox = sceneBox;
            pssm.Split(splitDistances, splitProjections);

            // ライト カメラ ビルダの状態を更新。
            lightCameraBuilder.EyeView = activeCamera.View;
            lightCameraBuilder.LightDirection = activeDirectionalLight.Direction;
            lightCameraBuilder.SceneBox = sceneBox;

            DeviceContext.RasterizerState = RasterizerState.CullBack;

            for (int i = 0; i < pssm.Count; i++)
            {
                // 必要となった場合にシャドウ マップ オブジェクトを生成。
                if (shadowMaps[i] == null)
                {
                    shadowMaps[i] = new ShadowMap(DeviceContext);
                    shadowMaps[i].DrawShadowCastersMethod = DrawShadowCasters;
                }

                // 射影行列は分割毎に異なる。
                lightCameraBuilder.EyeProjection = splitProjections[i];

                // ライトのビューおよび射影行列の算出。
                Matrix lightView;
                Matrix lightProjection;
                lightCameraBuilder.Build(out lightView, out lightProjection);

                // 後のモデル描画用にライト空間行列を算出。
                Matrix.Multiply(ref lightView, ref lightProjection, out lightViewProjections[i]);

                // ライト領域に含まれる投影オブジェクトを収集。
                CollectShadowCasters(lightView, lightProjection, activeDirectionalLight.Direction);

                // シャドウ マップを描画。
                shadowMaps[i].Form = shadowMapForm;
                shadowMaps[i].Size = shadowMapSize;
                shadowMaps[i].View = lightView;
                shadowMaps[i].Projection = lightProjection;
                shadowMaps[i].Draw();

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

            DeviceContext.RasterizerState = null;
        }

        void DrawShadowCasters(IEffect effect)
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
            DeviceContext.SetRenderTarget(lightOcclusionMapRenderTarget);
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

                    if (shadowMaps[i] != null)
                    {
                        shadowOcclusionMap.SetShadowMap(i, shadowMaps[i].RenderTarget);
                    }
                    else
                    {
                        shadowOcclusionMap.SetShadowMap(i, null);
                    }
                }
                shadowOcclusionMap.SetSplitDistance(MaxShadowMapSplitCount, splitDistances[MaxShadowMapSplitCount]);
                shadowOcclusionMap.LinearDepthMap = depthMapRenderTarget;

                shadowOcclusionMap.Draw();
            }

            DeviceContext.SetRenderTarget(null);

            occlusionMergeFilter.OtherOcclusionMap = ssaoMap.FinalTexture;
            FinalOcclusionMap = lightOcclusionMapPostprocess.Draw(lightOcclusionMapRenderTarget);
        }

        void DrawAmbientOcclusion()
        {
            // TODO
            // 設定ファイル管理。
            ssaoMap.BlurIteration = 3;
            ssaoMap.Radius = 2;
            ssaoMap.LinearDepthMap = depthMapRenderTarget;
            ssaoMap.NormalMap = normalMapRenderTarget;
            ssaoMap.Projection = activeCamera.Projection;
            ssaoMap.Draw();
        }

        void DrawSceneShadow()
        {
            //occlusionCombineFilter.Enabled = false;

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

                occlusionCombineFilter.Apply();
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
