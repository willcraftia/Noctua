#region Using

using System;
using System.ComponentModel;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Models
{
    public sealed class GraphicsShadowSettings
    {
        static readonly int[] ShadowMapResolutions =
        {
            512, 1024, 2048
        };

        static readonly float[] OcclusionMapResolutions =
        {
            0.25f, 0.5f, 1.0f
        };

        public const int DefaultSplitCount = 3;

        public const float DefaultSplitLambda = 0.5f;

        public const int DefaultShadowMapResolution = 1;

        public const int DefaultOcclusionMapResolution = 2;

        public const int DefaultOcclusionMapBlurRadius = 3;

        public const float DefaultOcclusionMapBlurSigma = 1.0f;

        public const float DefaultDepthBias = 0.0001f;

        int splitCount = DefaultSplitCount;

        float splitLambda = DefaultSplitLambda;

        int shadowMapResolution = DefaultShadowMapResolution;

        int occlusionMapResolution = DefaultOcclusionMapResolution;

        int occlusionMapBlurRadius = DefaultOcclusionMapBlurRadius;

        float occlusionMapBlurSigma = DefaultOcclusionMapBlurSigma;

        float depthBias = DefaultDepthBias;

        [DefaultValue(DefaultSplitCount)]
        public int SplitCount
        {
            get { return splitCount; }
            set
            {
                if (value < 1 || ShadowOcclusionMap.MaxSplitCount < value)
                    throw new ArgumentOutOfRangeException("value");

                splitCount = value;
            }
        }

        [DefaultValue(DefaultSplitLambda)]
        public float SplitLambda
        {
            get { return splitLambda; }
            set
            {
                if (value < 0 || 1 < value) throw new ArgumentOutOfRangeException("value");

                splitLambda = value;
            }
        }

        [DefaultValue(DefaultShadowMapResolution)]
        public int ShadowMapResolution
        {
            get { return shadowMapResolution; }
            set
            {
                if ((uint) ShadowMapResolutions.Length <= (uint) value)
                    throw new ArgumentOutOfRangeException("value");

                shadowMapResolution = value;
            }
        }

        [DefaultValue(DefaultOcclusionMapResolution)]
        public int OcclusionMapResolution
        {
            get { return occlusionMapResolution; }
            set
            {
                if ((uint) OcclusionMapResolutions.Length <= (uint) value)
                    throw new ArgumentOutOfRangeException("value");

                occlusionMapResolution = value;
            }
        }

        [DefaultValue(DefaultOcclusionMapBlurRadius)]
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

        [DefaultValue(DefaultOcclusionMapBlurSigma)]
        public float OcclusionMapBlurSigma
        {
            get { return occlusionMapBlurSigma; }
            set
            {
                if (value < float.Epsilon) throw new ArgumentOutOfRangeException("value");

                occlusionMapBlurSigma = value;
            }
        }

        [DefaultValue(DefaultDepthBias)]
        public float DepthBias
        {
            get { return depthBias; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                depthBias = value;
            }
        }
    }
}
