#region Using

using System;
using System.IO;

#endregion

namespace Noctua.Asset
{
    public static class TitleContainer
    {
        static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static Stream OpenStream(string path)
        {
            var absolutePath = Path.Combine(BaseDirectory, path);
            return File.OpenRead(absolutePath);
        }
    }
}
