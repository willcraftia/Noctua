#region Using

using System;

#endregion

namespace Noctua.Asset
{
    public interface IAssetPostLoad
    {
        void PostLoad(IAssetLoader loader);
    }
}
