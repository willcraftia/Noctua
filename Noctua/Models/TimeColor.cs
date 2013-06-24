#region Using

using System;
using Libra;

#endregion

namespace Noctua.Models
{
    /// <summary>
    /// 時間と色の組を管理する構造体です。
    /// </summary>
    public struct TimeColor
    {
        /// <summary>
        /// 時間。
        /// </summary>
        public float Time;

        /// <summary>
        /// 色。
        /// </summary>
        public Vector3 Color;

        /// <summary>
        /// 指定の時間と色でインスタンスを生成します。
        /// </summary>
        /// <param name="time">時間。</param>
        /// <param name="color">色。</param>
        public TimeColor(float time, Vector3 color)
        {
            Time = time;
            Color = color;
        }

        #region ToString

        public override string ToString()
        {
            return "{Time: " + Time + ", Color: " + Color + "}";
        }

        #endregion
    }
}
