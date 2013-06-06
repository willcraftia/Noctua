#region Using

using System;
using System.IO;
using Libra;
using Libra.Graphics;
using Libra.IO;
using Noctua;
using Noctua.Asset;

#endregion

namespace Samples.AssetSerialization
{
    class Program
    {
        static ResourceManager resourceManager;

        static AssetContainer assetContainer;

        static string directoryPath;

        static void Main(string[] args)
        {
            #region 開始処理

            resourceManager = new ResourceManager();
            resourceManager.Register<FileResourceLoader>();
            
            assetContainer = new AssetContainer(resourceManager);
            assetContainer.RegisterAssetSerializer<MeshSerializer>();

            directoryPath = Directory.GetCurrentDirectory() + "/Assets";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            Console.WriteLine("出力先: " + directoryPath);
            Console.WriteLine();

            Console.WriteLine("開始するにはエンター キーを押して下さい...");
            Console.ReadLine();

            var meshesPath = directoryPath + "/Meshes";
            if (!Directory.Exists(meshesPath)) Directory.CreateDirectory(meshesPath);

            #endregion

            #region 立方体メッシュ定義

            Console.WriteLine("立方体メッシュ定義");
            {
                var mesh = CreateCubeMesh();
                SerializeAndDeserialize<Mesh>("Meshes/Cube", mesh);
            }
            Console.WriteLine();

            #endregion

            #region 終了処理

            Console.WriteLine("終了するにはエンター キーを押して下さい...");
            Console.ReadLine();

            #endregion
        }

        #region シリアライゼーション/デシリアライゼーション補助メソッド

        static void SerializeAndDeserialize<T>(string baseFileName, T asset)
        {
            var fileName = baseFileName + ".json";
            var uri = "file:///" + directoryPath + "/" + fileName;

            // 新規生成。
            assetContainer.Create(uri, asset);

            // デタッチ。
            assetContainer.Detach(asset);

            // ロード。
            assetContainer.Load<T>(uri);
        }

        #endregion

        #region 立方体メッシュ生成補助メソッド

        static Mesh CreateCubeMesh()
        {
            Vector3[] normals =
            {
                new Vector3( 0,  1,  0),
                new Vector3( 0, -1,  0),
                new Vector3( 0,  0,  1),
                new Vector3( 0,  0, -1),
                new Vector3(-1,  0,  0),
                new Vector3( 1,  0,  0)
            };

            ushort[] indices = { 0, 1, 2, 0, 2, 3 };

            var mesh = new Mesh();
            mesh.Name = "Default Cube";
            mesh.MeshParts[Side.Top]    = CreateMeshPart(ref normals[0]);
            mesh.MeshParts[Side.Bottom] = CreateMeshPart(ref normals[1]);
            mesh.MeshParts[Side.Front]  = CreateMeshPart(ref normals[2]);
            mesh.MeshParts[Side.Back]   = CreateMeshPart(ref normals[3]);
            mesh.MeshParts[Side.Left]   = CreateMeshPart(ref normals[4]);
            mesh.MeshParts[Side.Right]  = CreateMeshPart(ref normals[5]);

            // Front と Back のみ UV 座標を調整。

            var meshPartFront = mesh.MeshParts[Side.Front];
            meshPartFront.Vertices[0].TexCoord = new Vector2(0, 1);
            meshPartFront.Vertices[1].TexCoord = new Vector2(1, 1);
            meshPartFront.Vertices[2].TexCoord = new Vector2(1, 0);
            meshPartFront.Vertices[3].TexCoord = new Vector2(0, 0);

            var meshPartBack = mesh.MeshParts[Side.Back];
            meshPartBack.Vertices[0].TexCoord = new Vector2(1, 0);
            meshPartBack.Vertices[1].TexCoord = new Vector2(0, 0);
            meshPartBack.Vertices[2].TexCoord = new Vector2(0, 1);
            meshPartBack.Vertices[3].TexCoord = new Vector2(1, 1);

            return mesh;
        }

        static MeshPart CreateMeshPart(ref Vector3 normal)
        {
            var vertices = CreateVertexPositionNormalTexture(ref normal);
            var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            return new MeshPart(vertices, indices);
        }

        static VertexPositionNormalTexture[] CreateVertexPositionNormalTexture(ref Vector3 normal)
        {
            var vertices = new VertexPositionNormalTexture[4];

            var side1 = new Vector3(normal.Y, normal.Z, normal.X);
            var side2 = Vector3.Cross(normal, side1);

            vertices[0].Position = (normal - side1 - side2) * 0.5f;
            vertices[1].Position = (normal - side1 + side2) * 0.5f;
            vertices[2].Position = (normal + side1 + side2) * 0.5f;
            vertices[3].Position = (normal + side1 - side2) * 0.5f;

            vertices[0].Normal = normal;
            vertices[1].Normal = normal;
            vertices[2].Normal = normal;
            vertices[3].Normal = normal;

            vertices[0].TexCoord = new Vector2(0, 0);
            vertices[1].TexCoord = new Vector2(0, 1);
            vertices[2].TexCoord = new Vector2(1, 1);
            vertices[3].TexCoord = new Vector2(1, 0);

            return vertices;
        }

        #endregion
    }
}
