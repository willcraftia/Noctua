﻿#region Using

using System;
using System.Collections.Generic;
using Libra;
using Libra.Collections;

#endregion

namespace Noctua.Scene
{
    public sealed class OctreeManager
    {
        public readonly Vector3 RegionSize;

        public readonly int MaxDepth;

        Dictionary<IntVector3, Octree> rootsByPositionGrid = new Dictionary<IntVector3, Octree>();

        Pool<Octree> octreePool;

        //BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

        //Vector3[] frustumCorners = new Vector3[BoundingFrustum.CornerCount];

        //BoundingSphere frustumSphere;

        public OctreeManager(Vector3 regionSize, int maxDepth)
        {
            RegionSize = regionSize;
            MaxDepth = maxDepth;

            octreePool = new Pool<Octree>(CreateOctree);
        }

        public void Add(SceneNode node)
        {
            Vector3 center;
            node.Box.GetCenter(out center);

            var rootPositionGrid = new IntVector3
            {
                X = (int) Math.Floor(center.X / RegionSize.X),
                Y = (int) Math.Floor(center.Y / RegionSize.Y),
                Z = (int) Math.Floor(center.Z / RegionSize.Z)
            };

            Octree root;
            if (!rootsByPositionGrid.TryGetValue(rootPositionGrid, out root))
            {
                root = octreePool.Borrow();

                var min = new Vector3
                {
                    X = rootPositionGrid.X * RegionSize.X,
                    Y = rootPositionGrid.Y * RegionSize.Y,
                    Z = rootPositionGrid.Z * RegionSize.Z
                };

                var box = new BoundingBox
                {
                    Min = min,
                    Max = min + RegionSize
                };

                root.Box = box;
                root.Sphere = BoundingSphere.CreateFromBoundingBox(root.Box);

                rootsByPositionGrid[rootPositionGrid] = root;
            }

            Add(node, root);
        }

        public void Update(SceneNode node)
        {
            if (node.Octree == null)
            {
                // 八分木へ未登録ならば追加処理。
                Add(node);
                return;
            }

            ContainmentType containmentType;

            Vector3 center;
            node.Box.GetCenter(out center);

            // ノードの中心がルート八分木に含まれているか否か。
            node.Octree.Root.Box.Contains(ref center, out containmentType);
            if (containmentType != ContainmentType.Contains)
            {
                // 削除してから追加。
                Remove(node);
                Add(node);
                return;
            }

            // ノードの境界ボックスが現在の八分木に含まれているか否か。
            node.Octree.Box.Contains(ref node.Box, out containmentType);
            if (containmentType != ContainmentType.Contains)
            {
                // 削除してから追加。
                // ただし、ルート八分木内での登録先変更。
                var root = node.Octree.Root;
                Remove(node);
                Add(node, root);
                return;
            }
        }

        public void Remove(SceneNode node)
        {
            if (node.Octree == null) return;

            var octree = node.Octree;

            // 八分木からノードを削除。
            octree.Nodes.Remove(node);

            if (octree.NodeCount == 0)
            {
                // ノードを持たない八分木となったならば、
                // その八分木を親から削除する。
                // 子については、ここでノード数が 0 ならば、
                // 子におけるノード削除の結果として子は全て削除済みであり、
                // あらためて探索して削除を試行する必要はない。
                RemoveOctreeFromParent(octree);
            }
        }

        /// <summary>
        /// 判定メソッドを満たす八分木を検索します。
        /// </summary>
        /// <param name="predicate">検索条件。</param>
        /// <param name="action">検索条件に一致した八分木に適用する処理。</param>
        public void Execute(Predicate<Octree> predicate, Action<Octree> action)
        {
            foreach (var root in rootsByPositionGrid.Values)
            {
                Execute(predicate, action, root);
            }
        }

        void Execute(Predicate<Octree> predicate, Action<Octree> action, Octree octree)
        {
            if (!predicate(octree))
                return;

            action(octree);

            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        var child = octree[x, y, z];
                        if (child != null)
                        {
                            Execute(predicate, action, child);
                        }
                    }
                }
            }
        }

        void Add(SceneNode node, Octree root)
        {
            ContainmentType containmentType;
            root.Box.Contains(ref node.Box, out containmentType);
            if (containmentType != ContainmentType.Contains)
            {
                // ルートにすら含まれない場合は、ルートへ強制登録。
                root.Nodes.Add(node);
            }
            else
            {
                // 適切な子を探索して登録。
                Add(node, root, 0);
            }
        }

        void Add(SceneNode node, Octree octree, int depth)
        {
            if (depth < MaxDepth && octree.IsTwiceSize(ref node.Box))
            {
                // 指定された八分木のサイズがノードの二倍以上ならば、
                // 子に対してノードを追加。

                int x;
                int y;
                int z;
                octree.GetChildIndex(ref node.Box, out x, out y, out z);

                if (octree[x, y, z] == null)
                {
                    // 子がまだ存在しないなら生成。
                    AllocateChildOctree(octree, x, y, z);
                }

                Add(node, octree[x, y, z], ++depth);
            }
            else
            {
                octree.Nodes.Add(node);
            }
        }

        void AllocateChildOctree(Octree octree, int x, int y, int z)
        {
            if (octree[x, y, z] != null) return;

            // 子がまだ存在しないなら生成。
            var child = octreePool.Borrow();
            octree[x, y, z] = child;

            var min = octree.Box.Min;
            var max = octree.Box.Max;

            var childMin = new Vector3();
            var childMax = new Vector3();

            if (x == 0)
            {
                childMin.X = min.X;
                childMax.X = (min.X + max.X) / 2;
            }
            else
            {
                childMin.X = (min.X + max.X) / 2;
                childMax.X = max.X;
            }

            if (y == 0)
            {
                childMin.Y = min.Y;
                childMax.Y = (min.Y + max.Y) / 2;
            }
            else
            {
                childMin.Y = (min.Y + max.Y) / 2;
                childMax.Y = max.Y;
            }

            if (z == 0)
            {
                childMin.Z = min.Z;
                childMax.Z = (min.Z + max.Z) / 2;
            }
            else
            {
                childMin.Z = (min.Z + max.Z) / 2;
                childMax.Z = max.Z;
            }

            child.Box.Min = childMin;
            child.Box.Max = childMax;
            BoundingSphere.CreateFromBoundingBox(ref child.Box, out child.Sphere);
        }

        void RemoveOctreeFromParent(Octree octree)
        {
            if (octree.Parent == null) return;

            // 親から自分を削除。
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        if (octree.Parent[x, y, z] != octree) continue;

                        var parent = octree.Parent;

                        // 自分をプールへ戻す。
                        octreePool.Return(octree);
                        octree.Parent[x, y, z] = null;

                        if (parent.NodeCount == 0)
                        {
                            // 再帰的にノードを持たなくなった先祖がいれば全て削除。
                            RemoveOctreeFromParent(parent);
                        }

                        return;
                    }
                }
            }
        }

        Octree CreateOctree()
        {
            return new Octree();
        }
    }
}
