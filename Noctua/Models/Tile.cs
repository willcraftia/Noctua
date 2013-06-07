#region Using

using System;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    public sealed class Tile
    {
        // ライティングに関するメモ
        //
        // フォン シェーディングは、洞窟内ブロックへ不自然なライティングを行う問題があるため、
        // 利用しない。
        // よって、過去に定義していた拡散反射光色、鏡面反射光色を破棄する。
        // また、放射光色は光源方向に依存しないため利用しても問題ないが、
        // 放射光のみを扱う利点は無いため、これも破棄する。

        public const int Size = 16;

        public const byte EmptyIndex = 0;

        public byte Index { get; set; }

        public TileCatalog Catalog { get; internal set; }

        public string Name { get; set; }

        public Texture2D Texture { get; set; }

        public bool Translucent { get; set; }

        public void GetTexCoordOffset(out Vector2 offset)
        {
            if (Catalog == null)
            {
                offset = Vector2.Zero;
            }
            else
            {
                Catalog.GetTexCoordOffset(Index, out offset);
            }
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + ", Index:" + Index + "}";
        }

        #endregion
    }
}
