#region Using

using System;
using Libra;

#endregion

namespace Noctua.Scene
{
    public sealed class BoundindBoxOctreeQuery
    {
        BoundingBox box = BoundingBox.Empty;

        Vector3[] corners = new Vector3[BoundingBox.CornerCount];

        BoundingSphere boxSphere;

        public BoundingBox Box
        {
            get { return box; }
            set
            {
                box = value;

                box.GetCorners(corners);
                boxSphere = BoundingSphere.CreateFromPoints(corners);
            }
        }

        public bool Contains(Octree octree)
        {
            bool intersected;

            // 球 vs 球
            boxSphere.Intersects(ref octree.Sphere, out intersected);
            if (!intersected)
                return false;

            // ボックス vs 球
            box.Intersects(ref octree.Sphere, out intersected);
            if (!intersected)
                return false;

            // ボックス vs ボックス
            box.Intersects(ref octree.Box, out intersected);
            if (!intersected)
                return false;

            return true;
        }
    }
}
