#region Using

using System;
using Libra;

#endregion

namespace Noctua.Scene
{
    public sealed class BoundingFrustumOctreeQuery
    {
        BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

        Vector3[] corners = new Vector3[BoundingFrustum.CornerCount];

        BoundingSphere frustumSphere;

        public Matrix Matrix
        {
            get { return frustum.Matrix; }
            set
            {
                // 視錐台更新。
                frustum.Matrix = value;

                // 視錐台を包む境界球。
                frustum.GetCorners(corners);
                BoundingSphere.CreateFromPoints(corners, out frustumSphere);
            }
        }

        public bool Contains(Octree octree)
        {
            bool intersected;

            // 球 vs 球
            frustumSphere.Intersects(ref octree.Sphere, out intersected);
            if (!intersected)
                return false;

            // 視錐台 vs 球
            frustum.Intersects(ref octree.Sphere, out intersected);
            if (!intersected)
                return false;

            // 視錐台 vs ボックス
            frustum.Intersects(ref octree.Box, out intersected);
            if (!intersected)
                return false;

            return true;
        }
    }
}
