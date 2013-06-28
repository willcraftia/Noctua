#region Using

using System;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class LinearFogSetup : PostprocessSetup
    {
        LinearFogFilter linearFogFilter;

        public float FogStart { get; set; }

        public float FogEnd { get; set; }

        public Vector3 FogColor { get; set; }

        public float FarClipDistance { get; set; }

        public LinearFogSetup(Device device)
        {
            if (device == null) throw new ArgumentNullException("device");

            linearFogFilter = new LinearFogFilter(device);
            
            // 初期値はフィルタの初期値に合わせる。
            FogStart = linearFogFilter.FogStart;
            FogEnd = linearFogFilter.FogEnd;
            FogColor = linearFogFilter.FogColor;
            FarClipDistance = linearFogFilter.FarClipDistance;

            Enabled = true;
        }

        public override void Setup(Postprocess postprocess)
        {
            linearFogFilter.FogStart = FogStart;
            linearFogFilter.FogEnd = FogEnd;
            linearFogFilter.FogColor = FogColor;
            linearFogFilter.FarClipDistance = FarClipDistance;

            linearFogFilter.LinearDepthMap = Manager.DepthMap;

            postprocess.Filters.Add(linearFogFilter);
        }
    }
}
