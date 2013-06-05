#region Using

using System;
using System.IO;

#endregion

namespace Noctua
{
    public sealed class TitleContainer
    {
        static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static Stream OpenStream(string name)
        {
            var path = Path.Combine(BaseDirectory, name);
            return File.OpenRead(path);
        }
    }
}
