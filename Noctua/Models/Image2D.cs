#region Using

using System;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    public sealed class Image2D : IDisposable
    {
        public string Name { get; set; }

        public Texture2D Texture { get; set; }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;

        ~Image2D()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (Texture != null) Texture.Dispose();

            disposed = true;
        }

        #endregion
    }
}
