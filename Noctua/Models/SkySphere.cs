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
        /// <summary>
        /// DepthFunction に LessEqual を用いる読み取り専用深度ステンシル ステート。
        /// Libra の DepthRead は Less (D3D11 デフォルト) を深度比較に用いるため、
        /// DepthRead では SkySphere の射影を遠クリップ面に強制する際に
        /// SkySphere の頂点深度が深度ステンシルのデフォルト深度 1 と等価となり、描画から除外されてしまう。
        /// そこで、深度比較を LessEqual とし、深度 1 に等しいなら描画対象にする。
        /// </summary>
        static readonly DepthStencilState DepthReadWithLessEqual = new DepthStencilState
        {
            DepthEnable = true,
            DepthWriteEnable = false,
            DepthFunction = ComparisonFunction.LessEqual,
            Name = "SkySphere.DepthReadWithLessEqual"
        };

        DeviceContext context;

        SkySphereEffect skySphereEffect;

        SphereMesh sphereMesh;

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

        public SkySphere(string name, DeviceContext context)
            : base(name)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;

            skySphereEffect = new SkySphereEffect(context.Device);
            sphereMesh = new SphereMesh(context, 1, 32);
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
            context.DepthStencilState = DepthReadWithLessEqual;
            // 内側 (背面) を描画。
            context.RasterizerState = RasterizerState.CullFront;

            skySphereEffect.Apply(context);
            sphereMesh.Draw();

            // デフォルトへ戻す。
            context.DepthStencilState = DepthStencilState.Default;
            context.RasterizerState = RasterizerState.CullBack;
        }

        public override void Draw(IEffect effect)
        {
            // スカイ スフィアを特殊なエフェクトで描画することはない。
        }
    }
}
