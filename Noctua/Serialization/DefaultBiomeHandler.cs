#region Using

using System;
using System.Reflection;
using Pyxis;

#endregion

namespace Noctua.Serialization
{
    public sealed class DefaultBiomeHandler : DefaultModuleTypeHandler
    {
        protected override bool IsIgnored(PropertyInfo property)
        {
            if ("Index" == property.Name)
                return false;

            return base.IsIgnored(property);
        }
    }
}
