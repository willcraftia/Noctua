#region Using

using System;
using System.Runtime.Serialization;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public interface IAsset
    {
        [IgnoreDataMember]
        IResource Resource { get; set; }
    }
}
