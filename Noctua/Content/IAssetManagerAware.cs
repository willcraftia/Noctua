#region Using

using System;

#endregion

namespace Noctua.Content
{
    public interface IAssetManagerAware
    {
        AssetManager AssetManager { set; }
    }
}
