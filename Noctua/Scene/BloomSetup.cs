#region Using

using System;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    public sealed class BloomSetup : FilterChainSetup
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
        /// 輝度抽出フィルタ。
        /// </summary>
        BloomExtractFilter bloomExtractFilter;

        /// <summary>
        /// ブルーム合成フィルタ。
        /// </summary>
        BloomCombineFilter bloomCombineFilter;

        /// <summary>
        /// ブラー用レンダ ターゲットのスケール。
        /// </summary>
        float blurScale = 0.25f;

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

        /// <summary>
        /// 輝度の閾値を取得または設定します。
        /// </summary>
        public float BloomThreshold
        {
            get { return bloomExtractFilter.Threshold; }
            set { bloomExtractFilter.Threshold = value; }
        }

        public float BaseIntensity
        {
            get { return bloomCombineFilter.BaseIntensity; }
            set { bloomCombineFilter.BaseIntensity = value; }
        }

        public float BaseSaturation
        {
            get { return bloomCombineFilter.BaseSaturation; }
            set { bloomCombineFilter.BaseSaturation = value; }
        }

        public float BloomIntensity
        {
            get { return bloomCombineFilter.BloomIntensity; }
            set { bloomCombineFilter.BloomIntensity = value; }
        }

        public float BloomSaturation
        {
            get { return bloomCombineFilter.BloomSaturation; }
            set { bloomCombineFilter.BloomSaturation = value; }
        }

        public BloomSetup(DeviceContext deviceContext)
            : base(deviceContext)
        {
            downFilter = new DownFilter(deviceContext);
            upFilter = new UpFilter(deviceContext);
            gaussianFilter = new GaussianFilter(deviceContext);
            gaussianFilterH = new GaussianFilterPass(gaussianFilter, GaussianFilterDirection.Horizon);
            gaussianFilterV = new GaussianFilterPass(gaussianFilter, GaussianFilterDirection.Vertical);
            bloomExtractFilter = new BloomExtractFilter(deviceContext);
            bloomCombineFilter = new BloomCombineFilter(deviceContext);
        }

        public override void Setup(FilterChain filterChain)
        {
            var upScale = 1.0f / BlurScale;
            downFilter.WidthScale = BlurScale;
            downFilter.HeightScale = BlurScale;
            upFilter.WidthScale = upScale;
            upFilter.HeightScale = upScale;

            bloomCombineFilter.BaseTexture = Manager.BaseSceneMap;

            filterChain.Filters.Add(downFilter);
            filterChain.Filters.Add(bloomExtractFilter);
            filterChain.Filters.Add(gaussianFilterH);
            filterChain.Filters.Add(gaussianFilterV);
            filterChain.Filters.Add(upFilter);
            filterChain.Filters.Add(bloomCombineFilter);
        }
    }
}
