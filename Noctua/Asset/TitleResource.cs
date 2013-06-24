#region Using

using System;
using System.IO;
using Libra.IO;

#endregion

namespace Noctua.Asset
{
    public sealed class TitleResource: IResource, IEquatable<TitleResource>
    {
        public const string TitleScheme = "title";

        string extension;

        object extensionLock = new object();

        string baseUri;

        object baseUriLock = new object();

        public string AbsoluteUri { get; internal set; }

        public string Scheme
        {
            get { return TitleScheme; }
        }

        public string AbsolutePath { get; internal set; }

        public string Extension
        {
            get
            {
                lock (extensionLock)
                {
                    if (extension == null)
                        extension = Path.GetExtension(AbsolutePath);
                    return extension;
                }
            }
        }

        public bool ReadOnly
        {
            get { return true; }
        }

        public bool Exists
        {
            get
            {
                return File.Exists(AbsolutePath);
            }
        }

        public string BaseUri
        {
            get
            {
                lock (baseUriLock)
                {
                    if (baseUri == null)
                    {
                        var lastSlash = AbsolutePath.LastIndexOf('/');
                        if (lastSlash < 0) lastSlash = 0;
                        var basePath = AbsolutePath.Substring(0, lastSlash + 1);
                        baseUri = TitleScheme + ":" + basePath;
                    }
                    return baseUri;
                }
            }
        }

        internal TitleResource() { }

        public Stream OpenRead()
        {
            return TitleContainer.OpenStream(AbsolutePath);
        }

        public Stream OpenWrite() { throw new NotSupportedException(); }

        public Stream CreateNew() { throw new NotSupportedException(); }

        public void Delete() { throw new NotSupportedException(); }

        #region Equatable

        public bool Equals(TitleResource other)
        {
            return AbsoluteUri == other.AbsoluteUri;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return Equals((TitleResource) obj);
        }

        public override int GetHashCode()
        {
            return AbsoluteUri.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return AbsoluteUri;
        }

        #endregion
    }

}
