#region Using

using System;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public sealed class TitleResourceLoader : IResourceLoader
    {
        const string prefix = "title:";

        public IResource Load(string uri)
        {
            if (!uri.StartsWith(prefix)) return null;

            var absolutePath = uri.Substring(prefix.Length);
            return new TitleResource { AbsoluteUri = uri, AbsolutePath = absolutePath };
        }
    }
}
