#region Using

using System;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class LinearFogSetup : FilterChainSetup
    {
        LinearFogFilter linearFogFilter;

        public float FogStart { get; set; }

        public float FogEnd { get; set; }

        public Vector3 FogColor { get; set; }

        public float FarClipDistance { get; set; }

        public LinearFogSetup(DeviceContext deviceContext)
            : base(deviceContext)
        {
            linearFogFilter = new LinearFogFilter(deviceContext);
            
            // 初期値はフィルタの初期値に合わせる。
            FogStart = linearFogFilter.FogStart;
            FogEnd = linearFogFilter.FogEnd;
            FogColor = linearFogFilter.FogColor;
            FarClipDistance = linearFogFilter.FarClipDistance;

            Enabled = true;
        }

        public override void Setup(FilterChain filterChain)
        {
            linearFogFilter.FogStart = FogStart;
            linearFogFilter.FogEnd = FogEnd;
            linearFogFilter.FogColor = FogColor;
            linearFogFilter.FarClipDistance = FarClipDistance;

            linearFogFilter.LinearDepthMap = Manager.DepthMap;

            filterChain.Filters.Add(linearFogFilter);
        }
    }
}
