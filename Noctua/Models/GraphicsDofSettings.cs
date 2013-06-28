#region Using

using System;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Models
{
    public sealed class GraphicsDofSettings
    {
        public static readonly float[] BlurResolutions =
        {
            0.25f, 0.5f, 1.0f
        };

        int blurResolution = 2;

        int blurRadius = GaussianFilter.DefaultRadius;

        float blurSigma = GaussianFilter.DefaultSigma;

        public int BlurResolution
        {
            get { return blurResolution; }
            set
            {
                if ((uint) BlurResolutions.Length < (uint) value) throw new ArgumentOutOfRangeException("value");

                blurResolution = value;
            }
        }

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
