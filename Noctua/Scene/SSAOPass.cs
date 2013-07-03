#region Using

using System;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class SSAOPass : SceneLightPass, IDisposable
    {
        /// <summary>
        /// 環境光閉塞マップ。
        /// </summary>
        SSAOMap ssaoMap;

        float renderTargetScale = 1.0f;

        public float RenderTargetScale
        {
            get { return renderTargetScale; }
            set
            {
                if (value < 0.0f || 1.0f < value) throw new ArgumentOutOfRangeException("value");

                renderTargetScale = value;
            }
        }

        public int PreferredRenderTargetMultisampleCount
        {
            get { return ssaoMap.PreferredRenderTargetMultisampleCount; }
            set { ssaoMap.PreferredRenderTargetMultisampleCount = value; }
        }

        public int Seed
        {
            get { return ssaoMap.Seed; }
            set { ssaoMap.Seed = value; }
        }

        public float Strength
        {
            get { return ssaoMap.Strength; }
            set { ssaoMap.Strength = value; }
        }

        public float Attenuation
        {
            get { return ssaoMap.Attenuation; }
            set { ssaoMap.Attenuation = value; }
        }

        public float Radius
        {
            get { return ssaoMap.Radius; }
            set { ssaoMap.Radius = value; }
        }

        public int SampleCount
        {
            get { return ssaoMap.SampleCount; }
            set { ssaoMap.SampleCount = value; }
        }

        public float BlurScale
        {
            get { return ssaoMap.BlurScale; }
            set { ssaoMap.BlurScale = value; }
        }

        public int BlurIteration
        {
            get { return ssaoMap.BlurIteration; }
            set { ssaoMap.BlurIteration = value; }
        }

        public int BlurRadius
        {
            get { return ssaoMap.BlurRadius; }
            set { ssaoMap.BlurRadius = value; }
        }

        public float BlurSpaceSigma
        {
            get { return ssaoMap.BlurSpaceSigma; }
            set { ssaoMap.BlurSpaceSigma = value; }
        }

        public float BlurDepthSigma
        {
            get { return ssaoMap.BlurDepthSigma; }
            set { ssaoMap.BlurDepthSigma = value; }
        }

        public float BlurNormalSigma
        {
            get { return ssaoMap.BlurNormalSigma; }
            set { ssaoMap.BlurNormalSigma = value; }
        }

        public ShaderResourceView BaseTexture
        {
            get { return ssaoMap.BaseTexture; }
        }

        public ShaderResourceView FinalTexture
        {
            get { return ssaoMap.FinalTexture; }
        }

        public SamplerState LinearDepthMapSampler
        {
            get { return ssaoMap.LinearDepthMapSampler; }
            set { ssaoMap.LinearDepthMapSampler = value; }
        }

        public SamplerState NormalMapSampler
        {
            get { return ssaoMap.NormalMapSampler; }
            set { ssaoMap.NormalMapSampler = value; }
        }

        public SSAOPass(DeviceContext deviceContext)
            : base(deviceContext)
        {
            ssaoMap = new SSAOMap(DeviceContext);
        }

        public override void Draw()
        {
            ssaoMap.RenderTargetWidth = (int) (DeviceContext.Device.BackBufferWidth * renderTargetScale);
            ssaoMap.RenderTargetHeight = (int) (DeviceContext.Device.BackBufferHeight * renderTargetScale);

            ssaoMap.LinearDepthMap = Manager.DepthMap;
            ssaoMap.NormalMap = Manager.NormalMap;
            ssaoMap.Projection = Manager.ActiveCamera.Projection;

            ssaoMap.Draw();

            Manager.LightSceneMaps.Add(ssaoMap.FinalTexture);
        }

        #region IDisposable

        bool disposed;

        ~SSAOPass()
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
                ssaoMap.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
