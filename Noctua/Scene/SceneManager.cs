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
        /// シャドウ マップ形式。
        /// </summary>
        ShadowMapForm shadowMapForm = ShadowMapForm.Basic;

        /// <summary>
        /// シャドウ マップ サイズ。
        /// </summary>
        int shadowMapSize = 1024;

        /// <summary>
        /// ライト カメラ ビルダ。
        /// </summary>
        LightCameraBuilder lightCameraBuilder = DefaultLightCameraBuilder;

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
        /// 線形深度マップ エフェクト。
        /// </summary>
        LinearDepthMapEffect depthMapEffect;

        /// <summary>
        /// 法線マップ エフェクト。
        /// </summary>
        NormalMapEffect normalMapEffect;

        /// <summary>
        /// カスケード シャドウ マップ。
        /// </summary>
        CascadeShadowMap cascadeShadowMap;

        /// <summary>
        /// シャドウ シーン マップ。
        /// </summary>
        ShadowSceneMap shadowSceneMap;

        /// <summary>
        /// 環境光閉塞マップ。
        /// </summary>
        SSAOMap ssaoMap;

        /// <summary>
        /// 閉塞マップ ポストプロセス。
        /// </summary>
        Postprocess occlusionPostprocess;

        /// <summary>
        /// 閉塞マップ合成フィルタ。
        /// </summary>
        OcclusionMergeFilter occlusionMergeFilter;

        /// <summary>
        /// シーン用ポストプロセス。
        /// </summary>
        Postprocess scenePostprocess;

        /// <summary>
        /// 閉塞マップ合成フィルタ。
        /// </summary>
        OcclusionCombineFilter occlusionCombineFilter;

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
            get { return cascadeShadowMap.SplitCount; }
            set
            {
                if (value < 1 || MaxShadowMapSplitCount < value) throw new ArgumentOutOfRangeException("value");

                cascadeShadowMap.SplitCount = value;
            }
        }

        /// <summary>
        /// シャドウ マップ分割ラムダ値を取得または設定します。
        /// </summary>
        public float ShadowMapSplitLambda
        {
            get { return cascadeShadowMap.SplitLambda; }
            set { cascadeShadowMap.SplitLambda = value; }
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

        // ブラー適用前のシャドウ シーン マップ。
        public ShaderResourceView BaseShadowSceneMap
        {
            get { return shadowSceneMap.BaseTexture; }
        }

        // ブラー適用後のシャドウ シーン マップ。
        public ShaderResourceView FinalShadowSceneMap
        {
            get { return shadowSceneMap.FinalTexture; }
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

        // シャドウ シーン マップ＋環境光閉塞マップ。
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
            lightingSceneMapRenderTarget.Initialize();

            // エフェクト。
            depthMapEffect = new LinearDepthMapEffect(DeviceContext);
            normalMapEffect = new NormalMapEffect(DeviceContext);

            // TODO
            //
            // オンデマンド生成にする。

            cascadeShadowMap = new CascadeShadowMap(DeviceContext);
            cascadeShadowMap.DrawShadowCastersCallback = DrawShadowCasters;

            shadowSceneMap = new ShadowSceneMap(DeviceContext);
            shadowSceneMap.RenderTargetWidth = backBuffer.Width;
            shadowSceneMap.RenderTargetHeight = backBuffer.Height;
            shadowSceneMap.BlurScale = 0.25f;
            shadowSceneMap.BlurEnabled = true;

            ssaoMap = new SSAOMap(DeviceContext);
            ssaoMap.RenderTargetWidth = backBuffer.Width;
            ssaoMap.RenderTargetHeight = backBuffer.Height;
            ssaoMap.BlurScale = 0.25f;

            occlusionPostprocess = new Postprocess(DeviceContext);
            occlusionPostprocess.Width = backBuffer.Width;
            occlusionPostprocess.Height = backBuffer.Height;
            occlusionPostprocess.Format = SurfaceFormat.Single;

            occlusionMergeFilter = new OcclusionMergeFilter(DeviceContext);
            occlusionPostprocess.Filters.Add(occlusionMergeFilter);

            scenePostprocess = new Postprocess(DeviceContext);
            scenePostprocess.Width = sceneMapRenderTarget.Width;
            scenePostprocess.Height = sceneMapRenderTarget.Height;
            scenePostprocess.Format = sceneMapRenderTarget.Format;

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
            return cascadeShadowMap.GetTexture(index);
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

            // 深度
            DrawDepth();

            // 法線
            DrawNormal();

            // シーン
            DrawScene();

            // シャドウ パス
            DrawAmbientOcclusion();
            DrawShadowScene();
            MergeOcclusionMaps();
            CombineOcclusion();

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

        void DrawShadowScene()
        {
            if (activeDirectionalLight == null || !activeDirectionalLight.Enabled)
                return;

            cascadeShadowMap.SplitCount = ShadowMapSplitCount;
            cascadeShadowMap.SplitLambda = ShadowMapSplitLambda;
            cascadeShadowMap.ShadowMapForm = shadowMapForm;
            cascadeShadowMap.View = activeCamera.View;
            cascadeShadowMap.Fov = activeCamera.Fov;
            cascadeShadowMap.AspectRatio = activeCamera.AspectRatio;
            cascadeShadowMap.NearClipDistance = activeCamera.NearClipDistance;
            // TODO
            //
            // 割合を設定ファイルで管理。
            cascadeShadowMap.FarClipDistance = activeCamera.FarClipDistance * 0.8f;
            cascadeShadowMap.LightDirection = activeDirectionalLight.Direction;
            cascadeShadowMap.ShadowMapSize = shadowMapSize;
            cascadeShadowMap.SceneBox = sceneBox;
            cascadeShadowMap.LightCameraBuilder = lightCameraBuilder ?? DefaultLightCameraBuilder;

            cascadeShadowMap.Draw();

            shadowSceneMap.View = activeCamera.View;
            shadowSceneMap.Projection = activeCamera.Projection;
            shadowSceneMap.LinearDepthMap = depthMapRenderTarget;
            shadowSceneMap.UpdateShadowMapSettings(cascadeShadowMap);

            shadowSceneMap.Draw();
        }

        void DrawShadowCasters(IEffect effect)
        {
            var shadowMapEffect = effect as ShadowMapEffect;
            var lightView = shadowMapEffect.View;
            var lightProjection = shadowMapEffect.Projection;

            // ライト領域に含まれる投影オブジェクトを収集。
            CollectShadowCasters(lightView, lightProjection, activeDirectionalLight.Direction);

            for (int i = 0; i < shadowCasters.Count; i++)
            {
                var shadowCaster = shadowCasters[i];
                shadowCaster.Draw(effect);
            }

            shadowCasters.Clear();
        }

        void DrawDepth()
        {
            DeviceContext.RasterizerState = null;
            DeviceContext.BlendState = null;
            DeviceContext.DepthStencilState = null;
            DeviceContext.SetRenderTarget(depthMapRenderTarget);
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
            DeviceContext.SetRenderTarget(normalMapRenderTarget);
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

        void DrawScene()
        {
            DeviceContext.RasterizerState = null;
            DeviceContext.BlendState = null;
            DeviceContext.DepthStencilState = null;
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

        void MergeOcclusionMaps()
        {
            occlusionMergeFilter.OtherOcclusionMap = ssaoMap.FinalTexture;
            FinalOcclusionMap = occlusionPostprocess.Draw(shadowSceneMap.FinalTexture);
        }

        void CombineOcclusion()
        {
            //occlusionCombineFilter.Enabled = false;

            if (!occlusionCombineFilter.Enabled)
            {
                BaseSceneMap = sceneMapRenderTarget;
            }
            else
            {
                DeviceContext.BlendState = null;
                DeviceContext.RasterizerState = null;
                DeviceContext.DepthStencilState = DepthStencilState.None;
                DeviceContext.SetRenderTarget(lightingSceneMapRenderTarget);

                occlusionCombineFilter.Texture = sceneMapRenderTarget;
                occlusionCombineFilter.OcclusionMap = FinalOcclusionMap;
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
