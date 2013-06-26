#region Using

using System;
using System.Runtime.InteropServices;
using Libra;
using Libra.Graphics;
using Noctua.Properties;

#endregion

namespace Noctua.Models
{
    public sealed class SkySphereEffect : IEffect, IEffectMatrices, IDisposable
    {
        #region SharedDeviceResource

        sealed class SharedDeviceResource
        {
            public VertexShader VertexShader { get; private set; }

            public PixelShader PixelShader { get; private set; }

            public SharedDeviceResource(Device device)
            {
                VertexShader = device.CreateVertexShader();
                VertexShader.Name = "SkySphereVS";
                VertexShader.Initialize(Resources.SkySphereVS);

                PixelShader = device.CreatePixelShader();
                PixelShader.Name = "SkySpherePS";
                PixelShader.Initialize(Resources.SkySpherePS);
            }
        }

        #endregion

        #region ParametersPerCameraVS

        public struct ParametersPerCameraVS
        {
            public Matrix ViewProjection;
        }

        #endregion

        #region ParametersPerObjectPS

        [StructLayout(LayoutKind.Explicit, Size = 64)]
        public struct ParametersPerObjectPS
        {
            [FieldOffset(0)]
            public Vector3 SkyColor;

            [FieldOffset(16)]
            public Vector3 SunDirection;

            [FieldOffset(32)]
            public Vector3 SunColor;

            [FieldOffset(48)]
            public float SunThreshold;

            [FieldOffset(52)]
            public float SunVisible;
        }

        #endregion

        #region DirtyFlags

        [Flags]
        enum DirtyFlags
        {
            ConstantBufferPerCameraVS   = (1 << 0),
            ConstantBufferPerObjectPS   = (1 << 1),
            ViewProjection              = (1 << 2)
        }

        #endregion

        Device device;

        SharedDeviceResource sharedDeviceResource;

        ConstantBuffer constantBufferPerCameraVS;

        ConstantBuffer constantBufferPerObjectPS;

        ParametersPerCameraVS parametersPerCameraVS;

        ParametersPerObjectPS parametersPerObjectPS;

        Matrix view;

        Matrix projection;

        DirtyFlags dirtyFlags;

        public Matrix World
        {
            get { return Matrix.Identity; }
            set { }
        }

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;

                dirtyFlags |= DirtyFlags.ViewProjection;
            }
        }

        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;

                dirtyFlags |= DirtyFlags.ViewProjection;
            }
        }

        public Vector3 SkyColor
        {
            get { return parametersPerObjectPS.SkyColor; }
            set
            {
                parametersPerObjectPS.SkyColor = value;

                dirtyFlags |= DirtyFlags.ConstantBufferPerObjectPS;
            }
        }

        public Vector3 SunDirection
        {
            get { return parametersPerObjectPS.SunDirection; }
            set
            {
                parametersPerObjectPS.SunDirection = value;

                dirtyFlags |= DirtyFlags.ConstantBufferPerObjectPS;
            }
        }

        public Vector3 SunColor
        {
            get { return parametersPerObjectPS.SunColor; }
            set
            {
                parametersPerObjectPS.SunColor = value;

                dirtyFlags |= DirtyFlags.ConstantBufferPerObjectPS;
            }
        }

        public float SunThreshold
        {
            get { return parametersPerObjectPS.SunThreshold; }
            set
            {
                parametersPerObjectPS.SunThreshold = value;

                dirtyFlags |= DirtyFlags.ConstantBufferPerObjectPS;
            }
        }

        public bool SunVisible
        {
            get { return parametersPerObjectPS.SunVisible != 0.0f; }
            set
            {
                parametersPerObjectPS.SunVisible = (value) ? 1.0f : 0.0f;

                dirtyFlags |= DirtyFlags.ConstantBufferPerObjectPS;
            }
        }

        public SkySphereEffect(Device device)
        {
            if (device == null) throw new ArgumentNullException("device");

            this.device = device;

            sharedDeviceResource = device.GetSharedResource<SkySphereEffect, SharedDeviceResource>();

            constantBufferPerCameraVS = device.CreateConstantBuffer();
            constantBufferPerCameraVS.Initialize<ParametersPerCameraVS>();

            constantBufferPerObjectPS = device.CreateConstantBuffer();
            constantBufferPerObjectPS.Initialize<ParametersPerObjectPS>();

            view = Matrix.Identity;
            projection = Matrix.Identity;

            parametersPerCameraVS.ViewProjection = Matrix.Identity;
            parametersPerObjectPS.SkyColor = Vector3.Zero;
            parametersPerObjectPS.SunDirection = Vector3.Up;
            parametersPerObjectPS.SunColor = Vector3.One;
            parametersPerObjectPS.SunThreshold = 0.999f;
            parametersPerObjectPS.SunVisible = 1.0f;

            dirtyFlags = DirtyFlags.ConstantBufferPerCameraVS |
                DirtyFlags.ConstantBufferPerObjectPS;
        }

        public void Apply(DeviceContext context)
        {
            if ((dirtyFlags & DirtyFlags.ViewProjection) != 0)
            {
                Matrix viewProjection;
                Matrix.Multiply(ref view, ref projection, out viewProjection);
                Matrix.Transpose(ref viewProjection, out parametersPerCameraVS.ViewProjection);

                dirtyFlags &= ~DirtyFlags.ViewProjection;
                dirtyFlags |= DirtyFlags.ConstantBufferPerCameraVS;
            }

            if ((dirtyFlags & DirtyFlags.ConstantBufferPerCameraVS) != 0)
            {
                constantBufferPerCameraVS.SetData(context, parametersPerCameraVS);

                dirtyFlags &= ~DirtyFlags.ConstantBufferPerCameraVS;
            }

            if ((dirtyFlags & DirtyFlags.ConstantBufferPerObjectPS) != 0)
            {
                constantBufferPerObjectPS.SetData(context, parametersPerObjectPS);

                dirtyFlags &= ~DirtyFlags.ConstantBufferPerObjectPS;
            }

            context.VertexShaderConstantBuffers[0] = constantBufferPerCameraVS;
            context.VertexShader = sharedDeviceResource.VertexShader;
            context.PixelShaderConstantBuffers[0] = constantBufferPerObjectPS;
            context.PixelShader = sharedDeviceResource.PixelShader;
        }

        #region IDisposable

        bool disposed;

        ~SkySphereEffect()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                sharedDeviceResource = null;
                constantBufferPerCameraVS.Dispose();
                constantBufferPerObjectPS.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
