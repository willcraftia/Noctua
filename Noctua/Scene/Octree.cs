#region Using

using System;
using System.Collections.ObjectModel;
using Libra;

#endregion

namespace Noctua.Scene
{
    public sealed class Octree
    {
        #region NodeCollection

        public sealed class NodeCollection : Collection<SceneNode>
        {
            Octree octree;

            internal NodeCollection(Octree octree)
            {
                this.octree = octree;
            }

            protected override void InsertItem(int index, SceneNode item)
            {
                item.Octree = octree;

                octree.Ref();

                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, SceneNode item)
            {
                var removed = this[index];
                removed.Octree = null;

                item.Octree = octree;

                base.SetItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                var removed = this[index];
                removed.Octree = null;

                octree.Unref();

                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                foreach (var item in this)
                {
                    item.Octree = null;

                    octree.Unref();
                }

                base.ClearItems();
            }
        }

        #endregion

        public BoundingBox Box;

        public BoundingSphere Sphere;

        Octree[, ,] children = new Octree[2, 2, 2];

        public Octree Parent { get; private set; }

        public Octree this[int x, int y, int z]
        {
            get { return children[x, y, z]; }
            set
            {
                if (children[x, y, z] != null)
                    children[x, y, z].Parent = null;

                children[x, y, z] = value;

                if (children[x, y, z] != null)
                    children[x, y, z].Parent = this;
            }
        }

        public Octree Root
        {
            get { return (Parent == null) ? this : Parent.Root; }
        }

        public NodeCollection Nodes { get; private set; }

        public int NodeCount { get; private set; }

        public Octree()
        {
            Nodes = new NodeCollection(this);
        }

        public bool IsTwiceSize(ref BoundingBox box)
        {
            Vector3 halfSize;
            Box.GetHalfSize(out halfSize);

            Vector3 targetSize;
            box.GetSize(out targetSize);

            return (targetSize.X <= halfSize.X) && (targetSize.Y <= halfSize.Y) && (targetSize.Z <= halfSize.Z);
        }

        public void GetChildIndex(ref BoundingBox box, out int x, out int y, out int z)
        {
            Vector3 center;
            Box.GetCenter(out center);

            Vector3 targetCenter;
            box.GetCenter(out targetCenter);

            x = (targetCenter.X <= center.X) ? 0 : 1;
            y = (targetCenter.Y <= center.Y) ? 0 : 1;
            z = (targetCenter.Z <= center.Z) ? 0 : 1;
        }

        void Ref()
        {
            NodeCount++;

            if (Parent != null) Parent.Ref();
        }

        void Unref()
        {
            NodeCount--;

            if (Parent != null) Parent.Unref();
        }
    }
}
