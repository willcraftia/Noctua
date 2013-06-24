#region Using

using System;
using Libra;

#endregion

namespace Noctua.Landscape
{
    /// <summary>
    /// アクティブ パーティション領域のデフォルト実装です。
    /// この実装では、円形のような形状で領域を管理します。
    /// </summary>
    public sealed class DefaultActiveVolume : IActiveVolume
    {
        /// <summary>
        /// 領域の半径。
        /// </summary>
        int radius;

        /// <summary>
        /// radius * radius。
        /// </summary>
        int radiusSquared;

        /// <summary>
        /// 領域の半径を取得または設定します。
        /// </summary>
        public int Radius
        {
            get { return radius; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                radius = value;
                radiusSquared = radius * radius;
            }
        }

        public bool Contains(IntVector3 eyePosition, IntVector3 point)
        {
            int distanceSquared;
            IntVector3.DistanceSquared(ref eyePosition, ref point, out distanceSquared);

            return distanceSquared <= radiusSquared;
        }

        public void ForEach(Action<IntVector3> action)
        {
            for (int z = -radius; z < radius; z++)
            {
                for (int y = -radius; y < radius; y++)
                {
                    for (int x = -radius; x < radius; x++)
                    {
                        var point = new IntVector3(x, y, z);
                        var lengthSquared = point.LengthSquared();
                        if (lengthSquared <= radiusSquared)
                            action(point);
                    }
                }
            }
        }
    }
}
