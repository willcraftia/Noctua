#region Using

using System;

#endregion

namespace Noctua.Models
{
    public sealed class GraphicsSettings
    {
        public GraphicsShadowSettings Shadow { get; set; }

        public GraphicsDofSettings Dof { get; set; }
    }
}
