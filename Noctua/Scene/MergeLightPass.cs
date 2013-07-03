#region Using

using System;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class MergeLightPass : SceneLightPass, IDisposable
    {
        RenderTargetChain renderTargetChain;

        FullScreenQuad fullScreenQuad;

        /// <summary>
        /// 閉塞マップ合成フィルタ。
        /// </summary>
        OcclusionMergeFilter occlusionMergeFilter;

        public MergeLightPass(DeviceContext deviceContext)
            : base(deviceContext)
        {
            var backBuffer = DeviceContext.Device.BackBuffer;

            renderTargetChain = new RenderTargetChain(DeviceContext.Device)
            {
                Width = backBuffer.Width,
                Height = backBuffer.Height,
                Format = SurfaceFormat.Single
            };

            fullScreenQuad = new FullScreenQuad(DeviceContext);

            occlusionMergeFilter = new OcclusionMergeFilter(DeviceContext);
        }

        public override void Draw()
        {
            if (Manager.LightSceneMaps.Count == 0)
                return;

            if (Manager.LightSceneMaps.Count == 1)
            {
                Manager.FinalLightSceneMap = Manager.LightSceneMaps[0];
                return;
            }

            DeviceContext.BlendState = null;
            DeviceContext.RasterizerState = null;
            DeviceContext.DepthStencilState = DepthStencilState.None;

            renderTargetChain.Reset();

            occlusionMergeFilter.Texture = Manager.LightSceneMaps[0];
            for (int i = 1; i < Manager.LightSceneMaps.Count; i++)
            {
                DeviceContext.SetRenderTarget(renderTargetChain.Current);

                occlusionMergeFilter.OtherOcclusionMap = Manager.LightSceneMaps[i];
                occlusionMergeFilter.Apply();
                fullScreenQuad.Draw();

                DeviceContext.SetRenderTarget(null);

                occlusionMergeFilter.Texture = renderTargetChain.Current;
                Manager.FinalLightSceneMap = renderTargetChain.Current;

                renderTargetChain.Next();
            }

            occlusionMergeFilter.Texture = null;
            occlusionMergeFilter.OtherOcclusionMap = null;

            DeviceContext.DepthStencilState = null;
        }

        #region IDisposable

        bool disposed;

        ~MergeLightPass()
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
                renderTargetChain.Dispose();
                fullScreenQuad.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
