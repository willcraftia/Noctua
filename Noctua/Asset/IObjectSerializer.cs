#region Using

using System;
using System.IO;

#endregion

namespace Noctua.Asset
{
    public interface IObjectSerializer
    {
        object ReadObject(Stream stream, Type type);

        void WriteObject(Stream stream, object graph);
    }
}
