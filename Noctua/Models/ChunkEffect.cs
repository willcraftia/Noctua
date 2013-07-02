#region Using

using System;
using System.Runtime.InteropServices;
using Libra;
using Libra.Graphics;
using Noctua.Properties;

#endregion

namespace Noctua.Models
{
    // メモ
    //
    // フォン シェーディングは、ブロック地形では洞窟内で不自然な効果となるため用いない。

    public sealed class ChunkEffect : IEffect, IEffectMatrices, IDisposable
    {
        #region SharedDeviceResource

        sealed class SharedDeviceResource
        {
            public VertexShader VertexShader { get; private set; }

            public PixelShader PixelShader { get; private set; }

            public VertexShader OcclusionVertexShader { get; private set; }

            public PixelShader OcclusionPixelShader { get; private set; }

            public VertexShader WireframeVertexShader { get; private set; }

            public PixelShader WireframePixelShader { get; private set; }

            public SharedDeviceResource(Device device)
            {
                // TODO
                // オンデマンドで生成。

                VertexShader = device.CreateVertexShader();
                VertexShader.Initialize(Resources.ChunkVS);

                PixelShader = device.CreatePixelShader();
                PixelShader.Initialize(Resources.ChunkPS);

                OcclusionVertexShader = device.CreateVertexShader();
                OcclusionVertexShader.Initialize(Resources.ChunkOcclusionVS);

                OcclusionPixelShader = device.CreatePixelShader();
                OcclusionPixelShader.Initialize(Resources.ChunkOcclusionPS);

                WireframeVertexShader = device.CreateVertexShader();
                WireframeVertexShader.Initialize(Resources.ChunkWireframeVS);

                WireframePixelShader = device.CreatePixelShader();
                WireframePixelShader.Initialize(Resources.ChunkWireframePS);
            }
        }

        #endregion

        #region ParametersPerObjectVS

        public struct ParametersPerObjectVS
        {
            public Matrix WorldViewProjection;
        }

        #endregion

        #region ParametersPerScenePS

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct ParametersPerScenePS
        {
            public Vector3 SunlightDiffuseColor;
        }

        #endregion

        #region DirtyFlags

        [Flags]
        enum DirtyFlags
        {
            ConstantBufferPerObjectVS   = (1 << 0),
            ConstantBufferPerScenePS    = (1 << 1),
            ViewProjection              = (1 << 2),
            WorldViewProjection         = (1 << 3)
        }

        #endregion

        SharedDeviceResource sharedDeviceResource;

        ConstantBuffer constantBufferPerObjectVS;

        ConstantBuffer constantBufferPerScenePS;

        ParametersPerObjectVS parametersPerObjectVS;

        ParametersPerScenePS parametersPerScenePS;

        Matrix world;

        Matrix view;

        Matrix projection;

        Matrix viewProjection;

        DirtyFlags dirtyFlags;

        public DeviceContext DeviceContext { get; private set; }

        public Matrix World
        {
            get { return world; }
            set
            {
                world = value;

                dirtyFlags |= DirtyFlags.WorldViewProjection;
            }
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

        public Vector3 SunlightDiffuseColor
        {
            get { return parametersPerScenePS.SunlightDiffuseColor; }
            set
            {
                parametersPerScenePS.SunlightDiffuseColor = value;

                dirtyFlags |= DirtyFlags.ConstantBufferPerScenePS;
            }
        }

        public ShaderResourceView Texture { get; set; }

        public SamplerState TextureSampler { get; set; }

        public ChunkEffectMode Mode { get; set; }

        public ChunkEffect(DeviceContext deviceContext)
        {
            if (deviceContext == null) throw new ArgumentNullException("deviceContext");

            DeviceContext = deviceContext;

            sharedDeviceResource = deviceContext.Device.GetSharedResource<ChunkEffect, SharedDeviceResource>();

            constantBufferPerObjectVS = deviceContext.Device.CreateConstantBuffer();
            constantBufferPerObjectVS.Initialize<ParametersPerObjectVS>();

            constantBufferPerScenePS = deviceContext.Device.CreateConstantBuffer();
            constantBufferPerScenePS.Initialize<ParametersPerScenePS>();

            world = Matrix.Identity;
            view = Matrix.Identity;
            projection = Matrix.Identity;
            viewProjection = Matrix.Identity;
            parametersPerObjectVS.WorldViewProjection = Matrix.Identity;

            TextureSampler = SamplerState.PointClamp;

            Mode = ChunkEffectMode.Default;

            dirtyFlags = DirtyFlags.ConstantBufferPerObjectVS |
                DirtyFlags.ConstantBufferPerScenePS;
        }

        public void Apply()
        {
            if ((dirtyFlags & DirtyFlags.ViewProjection) != 0)
            {
                Matrix.Multiply(ref view, ref projection, out viewProjection);

                dirtyFlags &= ~DirtyFlags.ViewProjection;
                dirtyFlags |= DirtyFlags.WorldViewProjection;
            }

            if ((dirtyFlags & DirtyFlags.WorldViewProjection) != 0)
            {
                Matrix worldViewProjection;
                Matrix.Multiply(ref world, ref viewProjection, out worldViewProjection);

                Matrix.Transpose(ref worldViewProjection, out parametersPerObjectVS.WorldViewProjection);

                dirtyFlags &= ~DirtyFlags.WorldViewProjection;
                dirtyFlags |= DirtyFlags.ConstantBufferPerObjectVS;
            }

            if ((dirtyFlags & DirtyFlags.ConstantBufferPerObjectVS) != 0)
            {
                constantBufferPerObjectVS.SetData(DeviceContext, parametersPerObjectVS);

                dirtyFlags &= ~DirtyFlags.ConstantBufferPerObjectVS;
            }

            DeviceContext.VertexShaderConstantBuffers[0] = constantBufferPerObjectVS;

            switch (Mode)
            {
                case ChunkEffectMode.Default:
                    if ((dirtyFlags & DirtyFlags.ConstantBufferPerScenePS) != 0)
                    {
                        constantBufferPerScenePS.SetData(DeviceContext, parametersPerScenePS);

                        dirtyFlags &= ~DirtyFlags.ConstantBufferPerScenePS;
                    }

                    DeviceContext.VertexShader = sharedDeviceResource.VertexShader;

                    DeviceContext.PixelShaderConstantBuffers[0] = constantBufferPerScenePS;
                    DeviceContext.PixelShaderResources[0] = Texture;
                    DeviceContext.PixelShaderSamplers[0] = TextureSampler;
                    DeviceContext.PixelShader = sharedDeviceResource.PixelShader;
                    break;
                case ChunkEffectMode.Occlusion:
                    DeviceContext.VertexShader = sharedDeviceResource.OcclusionVertexShader;
                    DeviceContext.PixelShader = sharedDeviceResource.OcclusionPixelShader;
                    break;
                case ChunkEffectMode.Wireframe:
                    DeviceContext.VertexShader = sharedDeviceResource.WireframeVertexShader;
                    DeviceContext.PixelShader = sharedDeviceResource.WireframePixelShader;
                    break;
            }
        }

        #region IDisposable

        bool disposed;

        ~ChunkEffect()
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
                constantBufferPerObjectVS.Dispose();
            }

            disposed = true;
        }

        #endregion
    }
}
