#region Using

using System;
using Libra.IO;

#endregion

namespace Noctua.Content
{
    public interface IAsset
    {
        // Resource は常にアセットの永続先を示す。
        // 換言すると、Resource が null ではないアセットは永続化されている。
        // ただし、ファイル システム上で直接削除した場合は、この限りではない。
        //
        // Resource の設定は、IAsset のインスタンス化を担う IAssetSerializer で行う。
        // Resource を変更できるクラスは IAssetSerializer の実装クラスのみであり、
        // その他のクラスからは変更できない。
        //
        // 読み込み:
        //      デシリアライズ直後に設定。
        //
        // 書き込み:
        //      シリアライズ直後に設定。
        //      永続先の変更がある場合、シリアライズ直後に Resource の参照を変更。

        IResource Resource { get; }
    }
}
