#region Using

using System;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class DofSetup : FilterChainSetup
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
        /// ブラー用レンダ ターゲットのスケール。
        /// </summary>
        float blurScale = 0.25f;

        /// <summary>
        /// 焦点範囲を取得または設定します。
        /// </summary>
        public float FocusRange
        {
            get { return dofCombineFilter.FocusRange; }
            set { dofCombineFilter.FocusRange = value; }
        }

        /// <summary>
        /// 焦点距離を取得または設定します。
        /// </summary>
        public float FocusDistance
        {
            get { return dofCombineFilter.FocusDistance; }
            set { dofCombineFilter.FocusDistance = value; }
        }

        /// <summary>
        /// ブラー用レンダ ターゲットのスケールを取得または設定します。
        /// </summary>
        public float BlurScale
        {
            get { return blurScale; }
            set
            {
                if (value < 0.0f || 1.0f < value) throw new ArgumentOutOfRangeException("value");

                blurScale = value;
            }
        }

        /// <summary>
        /// ブラーの半径を取得または設定します。
        /// </summary>
        public int BlurRadius
        {
            get { return gaussianFilter.Radius; }
            set { gaussianFilter.Radius = value; }
        }

        /// <summary>
        /// ブラーの標準偏差を取得または設定します。
        /// </summary>
        public float BlurSigma
        {
            get { return gaussianFilter.Sigma; }
            set { gaussianFilter.Sigma = value; }
        }

        public DofSetup(DeviceContext deviceContext)
            : base(deviceContext)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            downFilter = new DownFilter(deviceContext);
            upFilter = new UpFilter(deviceContext);
            gaussianFilter = new GaussianFilter(deviceContext);
            gaussianFilterH = new GaussianFilterPass(gaussianFilter, GaussianFilterDirection.Horizon);
            gaussianFilterV = new GaussianFilterPass(gaussianFilter, GaussianFilterDirection.Vertical);
            dofCombineFilter = new DofCombineFilter(deviceContext);
        }

        public override void Setup(FilterChain filterChain)
        {
            dofCombineFilter.BaseTexture = Manager.BaseSceneMap;
            dofCombineFilter.LinearDepthMap = Manager.DepthMap;

            var upScale = 1.0f / BlurScale;
            downFilter.WidthScale = BlurScale;
            downFilter.HeightScale = BlurScale;
            upFilter.WidthScale = upScale;
            upFilter.HeightScale = upScale;

            filterChain.Filters.Add(downFilter);
            filterChain.Filters.Add(gaussianFilterH);
            filterChain.Filters.Add(gaussianFilterV);
            filterChain.Filters.Add(upFilter);
            filterChain.Filters.Add(dofCombineFilter);
        }
    }
}
