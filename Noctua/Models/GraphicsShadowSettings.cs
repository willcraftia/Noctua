#region Using

using System;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Models
{
    public sealed class GraphicsShadowSettings
    {
        static readonly int[] ShadowMapSizes =
        {
            512, 1024, 2048
        };

        static readonly float[] OcclusionMapScales =
        {
            0.25f, 0.5f, 1.0f
        };

        int splitCount = 3;

        float splitLambda = 0.5f;

        int shadowMapSizeIndex = 1;

        int occlusionMapScaleIndex = 2;

        int occlusionMapBlurRadius = 3;

        float occlusionMapBlurSigma = 1.0f;

        float depthBias = 0.0001f;

        public int SplitCount
        {
            get { return splitCount; }
            set
            {
                if (value < 1 || CascadeShadowMap.MaxSplitCount < value)
                    throw new ArgumentOutOfRangeException("value");

                splitCount = value;
            }
        }

        public float SplitLambda
        {
            get { return splitLambda; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                splitLambda = value;
            }
        }

        public int ShadowMapSizeIndex
        {
            get { return shadowMapSizeIndex; }
            set
            {
                if ((uint) ShadowMapSizes.Length <= (uint) value)
                    throw new ArgumentOutOfRangeException("value");

                shadowMapSizeIndex = value;
            }
        }

        public int OcclusionMapScaleIndex
        {
            get { return occlusionMapScaleIndex; }
            set
            {
                if ((uint) OcclusionMapScales.Length <= (uint) value)
                    throw new ArgumentOutOfRangeException("value");

                occlusionMapScaleIndex = value;
            }
        }

        public int OcclusionMapBlurRadius
        {
            get { return occlusionMapBlurRadius; }
            set
            {
                if (value < 1 || GaussianFilter.MaxRadius < value)
                    throw new ArgumentOutOfRangeException("value");

                occlusionMapBlurRadius = value;
            }
        }

        public float OcclusionMapBlurSigma
        {
            get { return occlusionMapBlurSigma; }
            set
            {
                if (value < float.Epsilon) throw new ArgumentOutOfRangeException("value");

                occlusionMapBlurSigma = value;
            }
        }

        public float DepthBias
        {
            get { return depthBias; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                depthBias = value;
            }
        }

        public int ShadowMapSize
        {
            get { return ShadowMapSizes[shadowMapSizeIndex]; }
        }

        public float OcclusionMapScale
        {
            get { return OcclusionMapScales[occlusionMapScaleIndex]; }
        }
    }
}
