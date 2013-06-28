#region Using

using System;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class DofSetup : PostprocessSetup
    {
        /// <summary>
        /// ダウン フィルタ。
        /// </summary>
        DownFilter downFilter;

        /// <summary>
        /// アップ フィルタ。
        /// </summary>
        UpFilter upFilter;

        /// <summary>
        /// ガウシアン フィルタ。
        /// </summary>
        GaussianFilter gaussianFilter;

        /// <summary>
        /// ガウシアン フィルタ 水平パス。
        /// </summary>
        GaussianFilterPass gaussianFilterH;

        /// <summary>
        /// ガウシアン フィルタ 垂直パス。
        /// </summary>
        GaussianFilterPass gaussianFilterV;

        /// <summary>
        /// 被写界深度合成フィルタ。
        /// </summary>
        DofCombineFilter dofCombineFilter;

        /// <summary>
        /// 焦点範囲を取得または設定します。
        /// </summary>
        public float FocusRange { get; set; }

        /// <summary>
        /// 焦点距離を取得または設定します。
        /// </summary>
        public float FocusDistance { get; set; }

        public float BlurResolution { get; set; }

        public int BlurRadius { get; set; }

        public float BlurSigma { get; set; }

        // TODO
        // ブラー設定用プロパティ。

        public DofSetup(Device device)
        {
            downFilter = new DownFilter(device);
            upFilter = new UpFilter(device);
            gaussianFilter = new GaussianFilter(device);
            gaussianFilterH = new GaussianFilterPass(gaussianFilter, GaussianFilterDirection.Horizon);
            gaussianFilterV = new GaussianFilterPass(gaussianFilter, GaussianFilterDirection.Vertical);
            dofCombineFilter = new DofCombineFilter(device);

            FocusRange = dofCombineFilter.FocusRange;
            FocusDistance = dofCombineFilter.FocusDistance;
            BlurResolution = 0.25f;
            BlurRadius = gaussianFilter.Radius;
            BlurSigma = gaussianFilter.Sigma;
        }

        public override void Setup(Postprocess postprocess)
        {
            dofCombineFilter.FocusRange = FocusRange;
            dofCombineFilter.FocusDistance = FocusDistance;

            dofCombineFilter.BaseTexture = Manager.BaseSceneMap;
            dofCombineFilter.LinearDepthMap = Manager.DepthMap;

            var upResolution = 1.0f / BlurResolution;
            downFilter.WidthScale = BlurResolution;
            downFilter.HeightScale = BlurResolution;
            upFilter.WidthScale = upResolution;
            upFilter.HeightScale = upResolution;

            gaussianFilter.Radius = BlurRadius;
            gaussianFilter.Sigma = BlurSigma;

            postprocess.Filters.Add(downFilter);
            postprocess.Filters.Add(gaussianFilterH);
            postprocess.Filters.Add(gaussianFilterV);
            postprocess.Filters.Add(upFilter);
            postprocess.Filters.Add(dofCombineFilter);
        }
    }
}
