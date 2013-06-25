#region Using

using System;
using Libra;
using Libra.Graphics;
using Libra.Graphics.Toolkit;

#endregion

namespace Noctua.Scene
{
    // TODO
    //
    // シーン オブジェクトとして実装すべきか？

    public class SceneCamera : SceneObject
    {
        /// <summary>
        /// 姿勢。
        /// </summary>
        public Quaternion Orientation = Quaternion.Identity;

        /// <summary>
        /// ビュー行列。
        /// </summary>
        public Matrix View = Matrix.Identity;

        /// <summary>
        /// 射影行列。
        /// </summary>
        public Matrix Projection = Matrix.Identity;

        float fov = MathHelper.PiOver4;

        float aspectRatio = 1.0f;

        float nearClipDistance = 1.0f;

        float farClipDistance = 1000.0f;

        float focusRange = 3.0f;
        
        float focusDistance = 200.0f;

        Vector3[] frustumCorners = new Vector3[BoundingFrustum.CornerCount];

        public float Fov
        {
            get { return fov; }
            set
            {
                if (value <= 0.0f || Math.PI <= value) throw new ArgumentOutOfRangeException("value");

                fov = value;
            }
        }

        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                if (value <= 0.0f) throw new ArgumentOutOfRangeException("value");

                aspectRatio = value;
            }
        }

        public float NearClipDistance
        {
            get { return nearClipDistance; }
            set
            {
                if (value < 0.0f) throw new ArgumentOutOfRangeException("value");

                nearClipDistance = value;
            }
        }

        public float FarClipDistance
        {
            get { return farClipDistance; }
            set
            {
                if (value < 0.0f) throw new ArgumentOutOfRangeException("value");

                farClipDistance = value;
            }
        }

        public Vector3 Direction
        {
            get
            {
                var baseDirection = Vector3.Forward;

                Vector3 result;
                Vector3.Transform(ref baseDirection, ref Orientation, out result);

                return result;
            }
            set
            {
                if (value.IsZero()) throw new ArgumentException("Direction must be not zero.", "value");

                var direction = value;
                direction.Normalize();

                var start = Vector3.Forward;
                Quaternion.CreateRotationBetween(ref start, ref direction, out Orientation);
            }
        }

        public Vector3 Right
        {
            get
            {
                var baseRight = Vector3.Right;

                Vector3 result;
                Vector3.Transform(ref baseRight, ref Orientation, out result);

                return result;
            }
        }

        public Vector3 Up
        {
            get
            {
                var baseUp = Vector3.Up;

                Vector3 result;
                Vector3.Transform(ref baseUp, ref Orientation, out result);

                return result;
            }
        }

        /// <summary>
        /// Yaw 回転において回転軸を (0, 1, 0) に固定するか否かを示す値を取得または設定します。
        /// デフォルトは true です。
        /// </summary>
        /// <remarks>
        /// 通常、人が期待するカメラの Yaw は、カメラ座標系の y 軸周りの回転ではなく、
        /// (0, 1, 0) 軸周りの回転です。
        /// </remarks>
        /// <value>
        /// true (回転軸を固定する場合)、false (それ以外の場合)。
        /// </value>
        public bool YawAxisFixed { get; set; }

        public BoundingFrustum Frustum { get; private set; }

        public float FocusRange
        {
            get { return focusRange; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                focusRange = value;
            }
        }

        public float FocusDistance
        {
            get { return focusDistance; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                focusDistance = value;
            }
        }

        public SceneCamera(string name)
            : base(name)
        {
            Frustum = new BoundingFrustum(Matrix.Identity);
            YawAxisFixed = true;

            // 基本的には不可視。
            // デバッグ用ワイヤフレームを表示する場合には可視へ設定する。
            Visible = false;
        }

        public void Move(Vector3 movement)
        {
            Move(ref movement);
        }

        public void Move(ref Vector3 movement)
        {
            Vector3.Add(ref Position, ref movement, out Position);
        }

        public void MoveRelative(Vector3 movement)
        {
            MoveRelative(ref movement);
        }

        public void MoveRelative(ref Vector3 movement)
        {
            Vector3 translation;
            Vector3.Transform(ref movement, ref Orientation, out translation);

            Move(ref translation);
        }

        public void LookAt(Vector3 target)
        {
            LookAt(ref target);
        }

        public void LookAt(ref Vector3 target)
        {
            Vector3 direction;
            Vector3.Subtract(ref target, ref Position, out direction);

            Direction = direction;
        }

        public void Rotate(Vector3 axis, float angle)
        {
            Rotate(ref axis, angle);
        }

        public void Rotate(ref Vector3 axis, float angle)
        {
            Quaternion rotation;
            Quaternion.CreateFromAxisAngle(ref axis, angle, out rotation);

            rotation.Normalize();

            Quaternion newOrientation;
            Quaternion.Concatenate(ref Orientation, ref rotation, out newOrientation);

            Orientation = newOrientation;
        }

        public void Yaw(float angle)
        {
            var baseAxis = Vector3.UnitY;

            Vector3 axis;
            if (YawAxisFixed)
            {
                axis = baseAxis;
            }
            else
            {
                Vector3.Transform(ref baseAxis, ref Orientation, out axis);
            }

            Rotate(ref axis, angle);
        }

        public void Pitch(float angle)
        {
            var baseAxis = Vector3.UnitX;

            Vector3 axis;
            Vector3.Transform(ref baseAxis, ref Orientation, out axis);

            Rotate(ref axis, angle);
        }

        public void Roll(float angle)
        {
            var baseAxis = Vector3.UnitZ;

            Vector3 axis;
            Vector3.Transform(ref baseAxis, ref Orientation, out axis);

            Rotate(ref axis, angle);
        }

        public void Update()
        {
            UpdateView();
            UpdateProjection();
            UpdateFrustum();
        }

        void UpdateView()
        {
            Matrix rotation;
            Matrix.CreateFromQuaternion(ref Orientation, out rotation);

            Matrix transposeRotation;
            Matrix.Transpose(ref rotation, out transposeRotation);

            Vector3 translation;
            Vector3.Transform(ref Position, ref transposeRotation, out translation);

            View = transposeRotation;
            View.M41 = -translation.X;
            View.M42 = -translation.Y;
            View.M43 = -translation.Z;
        }

        void UpdateProjection()
        {
            Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, nearClipDistance, farClipDistance, out Projection);
        }

        void UpdateFrustum()
        {
            Matrix viewProjection;
            Matrix.Multiply(ref View, ref Projection, out viewProjection);

            Frustum.Matrix = viewProjection;

            Frustum.GetCorners(frustumCorners);
            Box = BoundingBox.CreateFromPoints(frustumCorners);
            Sphere = BoundingSphere.CreateFromPoints(frustumCorners);
        }

        public override void Draw()
        {
            // TODO
        }

        public override void Draw(IEffect effect)
        {
            // カメラは特殊効果に対応しない。
        }
    }
}
