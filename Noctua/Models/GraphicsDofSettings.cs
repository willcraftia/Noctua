#region Using

using System;
using System.ComponentModel;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Models
{
    public sealed class GraphicsDofSettings
    {
        static readonly float[] BlurResolutions =
        {
            0.25f, 0.5f, 1.0f
        };

        public const int DefaultBlurResolution = 2;

        public const int DefaultBlurRadius = GaussianFilter.DefaultRadius;

        public const float DefaultBlurSigma = GaussianFilter.DefaultSigma;

        int blurResolution = DefaultBlurResolution;

        int blurRadius = DefaultBlurRadius;

        float blurSigma = DefaultBlurSigma;

        [DefaultValue(DefaultBlurResolution)]
        public int BlurResolution
        {
            get { return blurResolution; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value");

                blurResolution = value;
            }
        }

        [DefaultValue(DefaultBlurRadius)]
        public int BlurRadius
        {
            get { return blurRadius; }
            set
            {
                if (value < 1 || GaussianFilter.MaxRadius < value)
                    throw new ArgumentOutOfRangeException("value");

                blurRadius = value;
            }
        }

        [DefaultValue(DefaultBlurSigma)]
        public float BlurSigma
        {
            get { return blurSigma; }
            set
            {
                if (value <= float.Epsilon) throw new ArgumentOutOfRangeException("value");

                blurSigma = value;
            }
        }
    }
}
