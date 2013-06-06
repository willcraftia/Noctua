#region Using

using System;
using System.Collections.ObjectModel;
using Libra;

#endregion

namespace Noctua
{
    public sealed class Side
    {
        public const int Count = 6;

        public const int TopIndex = 0;

        public const int BottomIndex = 1;

        public const int FrontIndex = 2;

        public const int BackIndex = 3;

        public const int LeftIndex = 4;

        public const int RightIndex = 5;

        public static readonly Side Top = new Side(TopIndex, "Top", IntVector3.Up);

        public static readonly Side Bottom = new Side(BottomIndex, "Bottom", IntVector3.Down);

        public static readonly Side Front = new Side(FrontIndex, "Front", IntVector3.Forward);

        public static readonly Side Back = new Side(BackIndex, "Back", IntVector3.Backward);

        public static readonly Side Left = new Side(LeftIndex, "Left", IntVector3.Left);

        public static readonly Side Right = new Side(RightIndex, "Right", IntVector3.Right);

        public static ReadOnlyCollection<Side> Items { get; private set; }

        static readonly Side[] sides = { Top, Bottom, Front, Back, Left, Right };

        public int Index { get; private set; }

        public string Name { get; private set; }

        public IntVector3 Direction { get; private set; }

        static Side()
        {
            Items = new ReadOnlyCollection<Side>(sides);
        }

        Side(int index, string name, IntVector3 direction)
        {
            Index = index;
            Name = name;
            Direction = direction;
        }

        public static Side FromIndex(int index)
        {
            if ((uint) Count < (uint) index) throw new ArgumentOutOfRangeException("index");

            return sides[index];
        }

        public Side Reverse()
        {
            switch (Index)
            {
                case TopIndex:      return Side.Bottom;
                case BottomIndex:   return Side.Top;
                case FrontIndex:    return Side.Back;
                case BackIndex:     return Side.Front;
                case LeftIndex:     return Side.Right;
                case RightIndex:    return Side.Left;
            }

            throw new InvalidOperationException();
        }

        public override int GetHashCode()
        {
            return Index;
        }

        #region ToString

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
