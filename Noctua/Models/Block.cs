#region Using

using System;

#endregion

namespace Noctua.Models
{
    public sealed class Block
    {
        public const byte EmptyIndex = 0;

        // MEMO
        //
        // 編集と複製に関して。
        // エディタでは、読み取り専用スキームの Block を編集可、および、複製可とする。
        // 読み取り専用である場合、書き込み可能な URI を指定するように促す。
        // これに拒否する場合、保存は取り消される。

        public string Name { get; set; }

        public byte Index { get; set; }

        public Mesh MeshPrototype { get; set; }

        public SideCollection<Tile> Tiles { get; private set; }

        // 1 つでも半透明タイルを参照したら、ブロック自体を半透明とする。
        // エディタでは、これをユーザへ強制する。
        public bool Translucent { get; set; }

        public bool Fluid { get; set; }

        public bool ShadowCasting { get; set; }

        public BlockShape Shape { get; set; }

        public float Mass { get; set; }

        // Always immovable.
        //public bool Immovable { get; set; }

        public float StaticFriction { get; set; }

        public float DynamicFriction { get; set; }

        public float Restitution { get; set; }

        public BlockMesh Mesh { get; private set; }

        public Block()
        {
            Tiles = new SideCollection<Tile>();
        }

        public void BuildMesh()
        {
            Mesh = BlockMesh.Create(this);
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + ", Index:" + Index + "}";
        }

        #endregion
    }
}
