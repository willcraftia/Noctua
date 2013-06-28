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

        Device device;

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

        public ChunkEffect(Device device)
        {
            if (device == null) throw new ArgumentNullException("device");

            this.device = device;

            sharedDeviceResource = device.GetSharedResource<ChunkEffect, SharedDeviceResource>();

            constantBufferPerObjectVS = device.CreateConstantBuffer();
            constantBufferPerObjectVS.Initialize<ParametersPerObjectVS>();

            constantBufferPerScenePS = device.CreateConstantBuffer();
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

        public void Apply(DeviceContext context)
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
                constantBufferPerObjectVS.SetData(context, parametersPerObjectVS);

                dirtyFlags &= ~DirtyFlags.ConstantBufferPerObjectVS;
            }

            context.VertexShaderConstantBuffers[0] = constantBufferPerObjectVS;

            switch (Mode)
            {
                case ChunkEffectMode.Default:
                    if ((dirtyFlags & DirtyFlags.ConstantBufferPerScenePS) != 0)
                    {
                        constantBufferPerScenePS.SetData(context, parametersPerScenePS);

                        dirtyFlags &= ~DirtyFlags.ConstantBufferPerScenePS;
                    }
                    
                    context.VertexShader = sharedDeviceResource.VertexShader;

                    context.PixelShaderConstantBuffers[0] = constantBufferPerScenePS;
                    context.PixelShaderResources[0] = Texture;
                    context.PixelShaderSamplers[0] = TextureSampler;
                    context.PixelShader = sharedDeviceResource.PixelShader;
                    break;
                case ChunkEffectMode.Occlusion:
                    context.VertexShader = sharedDeviceResource.OcclusionVertexShader;
                    context.PixelShader = sharedDeviceResource.OcclusionPixelShader;
                    break;
                case ChunkEffectMode.Wireframe:
                    context.VertexShader = sharedDeviceResource.WireframeVertexShader;
                    context.PixelShader = sharedDeviceResource.WireframePixelShader;
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
