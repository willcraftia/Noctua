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
        // 放射色は利用価値があるかもしれないが、当面は必要としないため破棄する。
        // なお、放射光色は、必要ならばタイルで定義し、
        // チャンク メッシュ構築時に頂点色へブレンド済みとしてしまえば良い。

        public const int Size = 16;

        public const byte EmptyIndex = 0;

        public byte Index { get; set; }

        public TileCatalog Catalog { get; internal set; }

        public string Name { get; set; }

        public Texture2D Texture { get; set; }

        public bool Translucent { get; set; }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + ", Index:" + Index + "}";
        }

        #endregion
    }
}
