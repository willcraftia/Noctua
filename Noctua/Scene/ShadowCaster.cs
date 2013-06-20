#region Using

using System;

#endregion

namespace Noctua.Scene
{
    public abstract class ShadowCaster : SceneObject
    {
        public bool CastShadow { get; set; }

        protected ShadowCaster(string name)
            : base(name)
        {
            CastShadow = true;
        }
    }
}
