#region Using

using System;
using System.Collections.Generic;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class ShadowMapPass : SceneLightPass, IDisposable
    {
        /// <summary>
        /// シャドウ マップ最大分割数。
        /// </summary>
        public const int MaxShadowMapSplitCount = 3;

        /// <summary>
        /// デフォルトのライト カメラ ビルダ。
        /// </summary>
        static readonly BasicLightCameraBuilder DefaultLightCameraBuilder = new BasicLightCameraBuilder();

        float sceneScale = 1.0f;

        /// <summary>
        /// カスケード シャドウ マップ。
        /// </summary>
        CascadeShadowMap cascadeShadowMap;

        /// <summary>
        /// シャドウ シーン マップ。
        /// </summary>
        ShadowSceneMap shadowSceneMap;

        /// <summary>
        /// ライト カメラ ビルダ。
        /// </summary>
        LightCameraBuilder lightCameraBuilder;

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
        /// 投影オブジェクトのリスト。
        /// </summary>
        List<ShadowCaster> shadowCasters = new List<ShadowCaster>(200);

        /// <summary>
        /// シャドウ マップ分割数を取得または設定します。
        /// </summary>
        public int SplitCount
        {
            get { return cascadeShadowMap.SplitCount; }
            set { cascadeShadowMap.SplitCount = value; }
        }

        /// <summary>
        /// シャドウ マップ分割ラムダ値を取得または設定します。
        /// </summary>
        public float SplitLambda
        {
            get { return cascadeShadowMap.SplitLambda; }
            set { cascadeShadowMap.SplitLambda = value; }
        }

        /// <summary>
        /// シャドウ マップ サイズを取得または設定します。
        /// </summary>
        public int ShadowMapSize
        {
            get { return cascadeShadowMap.ShadowMapSize; }
            set { cascadeShadowMap.ShadowMapSize = value; }
        }

        /// <summary>
        /// シャドウ マップ形式を取得または設定します。
        /// </summary>
        public ShadowMapForm ShadowMapForm
        {
            get { return cascadeShadowMap.ShadowMapForm; }
            set { cascadeShadowMap.ShadowMapForm = value; }
        }

        public float SceneScale
        {
            get { return sceneScale; }
            set
            {
                if (value < 0.0f || 1.0f < value) throw new ArgumentOutOfRangeException("value");

                sceneScale = value;
            }
        }

        public int PreferredSceneMultisampleCount
        {
            get { return shadowSceneMap.PreferredRenderTargetMultisampleCount; }
            set { shadowSceneMap.PreferredRenderTargetMultisampleCount = value; }
        }

        public float DepthBias
        {
            get { return shadowSceneMap.DepthBias; }
            set { shadowSceneMap.DepthBias = value; }
        }

        public bool PcfEnabled
        {
            get { return shadowSceneMap.PcfEnabled; }
            set { shadowSceneMap.PcfEnabled = value; }
        }

        public int PcfRadius
        {
            get { return shadowSceneMap.PcfRadius; }
            set { shadowSceneMap.PcfRadius = value; }
        }

        public bool SceneBlurEnabled
        {
            get { return shadowSceneMap.BlurEnabled; }
            set { shadowSceneMap.BlurEnabled = value; }
        }

        public float SceneBlurScale
        {
            get { return shadowSceneMap.BlurScale; }
            set { shadowSceneMap.BlurScale = value; }
        }

        public int SceneBlurRadius
        {
            get { return shadowSceneMap.BlurRadius; }
            set { shadowSceneMap.BlurRadius = value; }
        }

        public float SceneBlurSigma
        {
            get { return shadowSceneMap.BlurSigma; }
            set { shadowSceneMap.BlurSigma = value; }
        }

        public LightCameraBuilder LightCameraBuilder
        {
            get { return lightCameraBuilder; }
            set { lightCameraBuilder = value; }
        }

        public SamplerState LinearDepthMapSampler
        {
            get { return shadowSceneMap.LinearDepthMapSampler; }
            set { shadowSceneMap.LinearDepthMapSampler = value; }
        }

        public SamplerState ShadowMapSampler
        {
            get { return shadowSceneMap.ShadowMapSampler; }
            set { shadowSceneMap.ShadowMapSampler = value; }
        }

        public ShaderResourceView BaseTexture
        {
            get { return shadowSceneMap.BaseTexture; }
        }

        public ShaderResourceView FinalTexture
        {
            get { return shadowSceneMap.FinalTexture; }
        }

        public ShadowMapPass(DeviceContext deviceContext)
            : base(deviceContext)
        {
            cascadeShadowMap = new CascadeShadowMap(DeviceContext)
            {
                DrawShadowCastersCallback = DrawShadowCasters
            };

            shadowSceneMap = new ShadowSceneMap(DeviceContext);

            queryShadowCasterMethod = new Predicate<Octree>(boundindBoxOctreeQuery.Contains);
            collectShadowCasterMethod = new Action<Octree>(CollectShadowCasters);
        }

        public override void Draw()
        {
            var light = Manager.ActiveDirectionalLight;
            if (light == null || !light.Enabled)
                return;
            
            var camera = Manager.ActiveCamera;

            cascadeShadowMap.View = camera.View;
            cascadeShadowMap.Fov = camera.Fov;
            cascadeShadowMap.AspectRatio = camera.AspectRatio;
            cascadeShadowMap.NearClipDistance = camera.NearClipDistance;
            // TODO
            //
            // 割合を設定ファイルで管理。
            cascadeShadowMap.FarClipDistance = camera.FarClipDistance * 0.8f;
            cascadeShadowMap.LightDirection = light.Direction;
            cascadeShadowMap.SceneBox = Manager.SceneBox;
            cascadeShadowMap.LightCameraBuilder = lightCameraBuilder ?? DefaultLightCameraBuilder;

            cascadeShadowMap.Draw();

            shadowSceneMap.RenderTargetWidth = (int) (DeviceContext.Device.BackBufferWidth * sceneScale);
            shadowSceneMap.RenderTargetHeight = (int) (DeviceContext.Device.BackBufferHeight * sceneScale);

            shadowSceneMap.View = camera.View;
            shadowSceneMap.Projection = camera.Projection;
            shadowSceneMap.LinearDepthMap = Manager.DepthMap;
            shadowSceneMap.UpdateShadowMapSettings(cascadeShadowMap);

            shadowSceneMap.Draw();

            Manager.LightSceneMaps.Add(shadowSceneMap.FinalTexture);
        }

        /// <summary>
        /// シャドウ マップを取得します。
        /// </summary>
        /// <param name="index">分割に対応するライト カメラのインデックス。</param>
        /// <returns>シャドウ マップ。</returns>
        public ShaderResourceView GetShadowMap(int index)
        {
            return cascadeShadowMap.GetTexture(index);
        }

        void DrawShadowCasters(IEffect effect)
        {
            var shadowMapEffect = effect as ShadowMapEffect;

            CollectShadowCasters(shadowMapEffect.View, shadowMapEffect.Projection);

            for (int i = 0; i < shadowCasters.Count; i++)
            {
                var shadowCaster = shadowCasters[i];
                shadowCaster.Draw(effect);
            }

            shadowCasters.Clear();
        }

        void CollectShadowCasters(Matrix lightView, Matrix lightProjection)
        {
            Matrix frustumMatrix;
            Matrix.Multiply(ref lightView, ref lightProjection, out frustumMatrix);
            lightFrustum.Matrix = frustumMatrix;
            lightFrustum.GetCorners(lightFrustumCorners);

            BoundingBox box;
            BoundingBox.CreateFromPoints(lightFrustumCorners, out box);

            boundindBoxOctreeQuery.Box = box;

            // ライト カメラに含まれる投影オブジェクトを収集。
            Manager.QueryOctree(queryShadowCasterMethod, collectShadowCasterMethod);
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

        #region IDisposable

        bool disposed;

        ~ShadowMapPass()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                cascadeShadowMap.Dispose();
                shadowSceneMap.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
