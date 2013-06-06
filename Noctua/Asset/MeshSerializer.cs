﻿#region Using

using System;
using System.IO;
using Libra.IO;
using Libra.Graphics;

#endregion

namespace Noctua.Asset
{
    public sealed class MeshSerializer : AssetSerializer
    {
        #region MeshPartDefinition

        public struct MeshPartDefinition
        {
            public VertexPositionNormalTexture[] Vertices;

            public ushort[] Indices;
        }

        #endregion

        #region MeshDefinition

        public struct MeshDefinition
        {
            //----------------------------
            // Editor/Debug

            public string Name;

            //----------------------------
            // MeshPart

            public MeshPartDefinition Top;

            public MeshPartDefinition Bottom;

            public MeshPartDefinition Front;

            public MeshPartDefinition Back;

            public MeshPartDefinition Left;

            public MeshPartDefinition Right;
        }

        #endregion

        public override Type AssetType
        {
            get { return typeof(Mesh); }
        }

        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<MeshDefinition>(stream);

            var mesh = new Mesh
            {
                Name = definition.Name
            };
            mesh.MeshParts[Side.Top]    = ToMeshPart(definition.Top);
            mesh.MeshParts[Side.Bottom] = ToMeshPart(definition.Bottom);
            mesh.MeshParts[Side.Front]  = ToMeshPart(definition.Front);
            mesh.MeshParts[Side.Back]   = ToMeshPart(definition.Back);
            mesh.MeshParts[Side.Left]   = ToMeshPart(definition.Left);
            mesh.MeshParts[Side.Right]  = ToMeshPart(definition.Right);

            return mesh;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var mesh = asset as Mesh;

            var definition = new MeshDefinition
            {
                Name    = mesh.Name,
                Top     = ToMeshPartDefinition(mesh.MeshParts[Side.Top]),
                Bottom  = ToMeshPartDefinition(mesh.MeshParts[Side.Bottom]),
                Front   = ToMeshPartDefinition(mesh.MeshParts[Side.Front]),
                Back    = ToMeshPartDefinition(mesh.MeshParts[Side.Back]),
                Left    = ToMeshPartDefinition(mesh.MeshParts[Side.Left]),
                Right   = ToMeshPartDefinition(mesh.MeshParts[Side.Right])
            };

            WriteObject(stream, definition);
        }

        MeshPart ToMeshPart(MeshPartDefinition meshPartDefinition)
        {
            if (meshPartDefinition.Vertices == null || meshPartDefinition.Vertices.Length == 0 ||
                meshPartDefinition.Indices == null || meshPartDefinition.Indices.Length == 0)
                return null;

            return new MeshPart(meshPartDefinition.Vertices, meshPartDefinition.Indices);
        }

        MeshPartDefinition ToMeshPartDefinition(MeshPart meshPart)
        {
            if (meshPart == null) return new MeshPartDefinition();

            return new MeshPartDefinition
            {
                Vertices = meshPart.Vertices,
                Indices = meshPart.Indices
            };
        }
    }
}
