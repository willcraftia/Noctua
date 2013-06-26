#region Using

using System;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    public sealed class LensFlareMesh : SceneObject
    {
        DeviceContext context;

        LensFlare lensFlare;

        public float QuerySize
        {
            get { return lensFlare.QuerySize; }
            set { lensFlare.QuerySize = value; }
        }

        public LensFlare.FlareCollection Flares
        {
            get { return lensFlare.Flares; }
        }

        public ShaderResourceView GlowTexture
        {
            get { return lensFlare.GlowTexture; }
            set { lensFlare.GlowTexture = value; }
        }

        public float GlowSize
        {
            get { return lensFlare.GlowSize; }
            set { lensFlare.GlowSize = value; }
        }

        public string LightName { get; set; }

        public LensFlareMesh(string name, DeviceContext context)
            : base(name)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            lensFlare = new LensFlare(context);
        }

        public override void Draw()
        {
            var manager = Parent.Manager;

            // ライト設定。

            // ライトが存在しないならば描画をスキップする。
            if (!manager.DirectionalLights.Contains(LightName))
                return;

            var light = manager.DirectionalLights[LightName];
            lensFlare.LightDirection = light.Direction;

            // カメラ設定。
            
            var camera = manager.ActiveCamera;
            lensFlare.View = camera.View;
            lensFlare.Projection = camera.Projection;

            lensFlare.Draw();
        }
    }
}
