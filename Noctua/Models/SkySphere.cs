#region Using

using System;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;
using Noctua.Scene;

#endregion

namespace Noctua.Models
{
    public sealed class SkySphere : SceneObject
    {
        SkySphereEffect skySphereEffect;

        SphereMesh sphereMesh;

        public DeviceContext DeviceContext { get; private set; }

        public Vector3 SkyColor
        {
            get { return skySphereEffect.SkyColor; }
            set { skySphereEffect.SkyColor = value; }
        }

        public Vector3 SunDirection
        {
            get { return skySphereEffect.SunDirection; }
            set { skySphereEffect.SunDirection = value; }
        }

        public Vector3 SunColor
        {
            get { return skySphereEffect.SunColor; }
            set { skySphereEffect.SunColor = value; }
        }

        public float SunThreshold
        {
            get { return skySphereEffect.SunThreshold; }
            set { skySphereEffect.SunThreshold = value; }
        }

        public bool SunVisible
        {
            get { return skySphereEffect.SunVisible; }
            set { skySphereEffect.SunVisible = value; }
        }

        public SkySphere(string name, DeviceContext deviceContext)
            : base(name)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            DeviceContext = deviceContext;

            skySphereEffect = new SkySphereEffect(deviceContext);
            sphereMesh = new SphereMesh(deviceContext, 1, 32);
        }

        public override void Draw()
        {
            var camera = Parent.Manager.ActiveCamera;

            var localView = camera.View;
            localView.Translation = Vector3.Zero;

            skySphereEffect.View = localView;

            // z = w を強制して遠クリップ面に描画。
            var localProjection = camera.Projection;
            localProjection.M13 = localProjection.M14;
            localProjection.M23 = localProjection.M24;
            localProjection.M33 = localProjection.M34;
            localProjection.M43 = localProjection.M44;

            skySphereEffect.Projection = localProjection;

            // 読み取り専用深度かつ深度比較 LessEqual。
            // 内側 (背面) を描画。
            DeviceContext.DepthStencilState = DepthStencilState.DepthReadLessEqual;
            DeviceContext.RasterizerState = RasterizerState.CullFront;

            skySphereEffect.Apply();
            sphereMesh.Draw();

            // デフォルトへ戻す。
            DeviceContext.DepthStencilState = null;
            DeviceContext.RasterizerState = null;
        }
    }
}
