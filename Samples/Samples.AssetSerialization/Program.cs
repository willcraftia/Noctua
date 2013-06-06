#region Using

using System;
using System.IO;
using Libra;
using Libra.Graphics;
using Libra.IO;
using Noctua;
using Noctua.Asset;
using Noctua.Models;
using Noctua.Serialization;

#endregion

namespace Samples.AssetSerialization
{
    class Program
    {
        const bool generateXml = true;

        static readonly string jsonExtension = ".json";

        static readonly string xmlExtension = ".xml";

        static string directoryPath;

        static void Main(string[] args)
        {
            #region 開始処理

            directoryPath = Directory.GetCurrentDirectory() + "/Assets";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            Console.WriteLine("出力先: " + directoryPath);
            Console.WriteLine();

            Console.WriteLine("開始するにはエンター キーを押して下さい...");
            Console.ReadLine();

            var tilesPath = directoryPath + "/Tiles";
            if (!Directory.Exists(tilesPath)) Directory.CreateDirectory(tilesPath);

            var tileCatalogsPath = directoryPath + "/TileCatalogs";
            if (!Directory.Exists(tileCatalogsPath)) Directory.CreateDirectory(tileCatalogsPath);

            var meshesPath = directoryPath + "/Meshes";
            if (!Directory.Exists(meshesPath)) Directory.CreateDirectory(meshesPath);

            #endregion

            #region タイル定義

            Console.WriteLine("タイル定義");
            {
                {
                    var definition = new TileDefinition
                    {
                        Name = "Dirt",
                        Texture = "Dirt.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/Dirt", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Grass Bottom",
                        Texture = "GrassBottom.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/GrassBottom", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Grass Side",
                        Texture = "GrassSide.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/GrassSide", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Grass Top",
                        Texture = "GrassTop.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/GrassTop", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Mantle",
                        Texture = "Mantle.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/Mantle", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Sand",
                        Texture = "Sand.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/Sand", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Snow",
                        Texture = "Snow.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/Snow", definition);
                }

                {
                    var definition = new TileDefinition
                    {
                        Name = "Stone",
                        Texture = "Stone.png",
                        Translucent = false,
                    };
                    SerializeAndDeserialize<TileDefinition>("Tiles/Stone", definition);
                }
            }
            Console.WriteLine();

            #endregion

            #region タイル カタログ定義

            Console.WriteLine("タイル カタログ定義");
            {
                var definition = new TileCatalogDefinition
                {
                    Name = "Default",
                    Entries = new IndexedUriDefinition[]
                {
                    new IndexedUriDefinition { Index = 1, Uri = "../Tiles/Dirt.json" },
                    new IndexedUriDefinition { Index = 2, Uri = "../Tiles/GrassBottom.json" },
                    new IndexedUriDefinition { Index = 3, Uri = "../Tiles/GrassSide.json" },
                    new IndexedUriDefinition { Index = 4, Uri = "../Tiles/GrassTop.json" },
                    new IndexedUriDefinition { Index = 5, Uri = "../Tiles/Mantle.json" },
                    new IndexedUriDefinition { Index = 6, Uri = "../Tiles/Sand.json" },
                    new IndexedUriDefinition { Index = 7, Uri = "../Tiles/Snow.json" },
                    new IndexedUriDefinition { Index = 8, Uri = "../Tiles/Stone.json" }
                }
                };
                SerializeAndDeserialize<TileCatalogDefinition>("TileCatalogs/Default", definition);
            }
            Console.WriteLine();

            #endregion

            #region 立方体メッシュ定義

            Console.WriteLine("立方体メッシュ定義");
            {
                var mesh = CreateCubeMesh();
                SerializeAndDeserialize<MeshDefinition>("Meshes/Cube", mesh);
            }
            Console.WriteLine();

            #endregion

            #region 終了処理

            Console.WriteLine("終了するにはエンター キーを押して下さい...");
            Console.ReadLine();

            #endregion
        }

        #region シリアライゼーション/デシリアライゼーション補助メソッド

        static void SerializeAndDeserialize<T>(string baseFileName, object graph)
        {
            JsonObjectSerializer.Instance.JsonSerializer.Formatting = Newtonsoft.Json.Formatting.Indented;
         
            var jsonPath = Serialize(JsonObjectSerializer.Instance, baseFileName, jsonExtension, graph);

            Console.WriteLine("JSON シリアライズ成功: " + jsonPath);

            var jsonResult = Deserialize(JsonObjectSerializer.Instance, jsonPath, typeof(T));

            Console.WriteLine("JSON デシリアライズ成功");

            if (generateXml)
            {
                XmlObjectSerializer.Instance.WriterSettings.Indent = true;
                XmlObjectSerializer.Instance.WriterSettings.IndentChars = "\t";
                XmlObjectSerializer.Instance.WriterSettings.OmitXmlDeclaration = true;

                var xmlPath = Serialize(XmlObjectSerializer.Instance, baseFileName, xmlExtension, graph);

                Console.WriteLine("XML シリアライズ成功: " + xmlPath);

                var xmlResult = Deserialize(XmlObjectSerializer.Instance, xmlPath, typeof(T));

                Console.WriteLine("XML デシリアライズ成功");
            }
        }

        static string Serialize(IObjectSerializer serializer, string baseFileName, string extension, object graph)
        {
            var fileName = baseFileName + extension;
            var path = directoryPath + "/" + fileName;

            using (var stream = File.Create(path))
            {
                serializer.WriteObject(stream, graph);
            }
            
            return path;
        }

        static object Deserialize(IObjectSerializer serializer, string path, Type type)
        {
            object result;

            using (var stream = File.OpenRead(path))
            {
                result = serializer.ReadObject(stream, type);
            }
            
            return result;
        }

        #endregion

        #region 立方体メッシュ生成補助メソッド

        static MeshDefinition CreateCubeMesh()
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

            var mesh = new MeshDefinition();
            mesh.Name   = "Default Cube";
            mesh.Top    = CreateMeshPart(ref normals[0]);
            mesh.Bottom = CreateMeshPart(ref normals[1]);
            mesh.Front  = CreateMeshPart(ref normals[2]);
            mesh.Back   = CreateMeshPart(ref normals[3]);
            mesh.Left   = CreateMeshPart(ref normals[4]);
            mesh.Right  = CreateMeshPart(ref normals[5]);

            // Front と Back のみ UV 座標を調整。

            var meshPartFront = mesh.Front;
            meshPartFront.Vertices[0].TexCoord = new Vector2(0, 1);
            meshPartFront.Vertices[1].TexCoord = new Vector2(1, 1);
            meshPartFront.Vertices[2].TexCoord = new Vector2(1, 0);
            meshPartFront.Vertices[3].TexCoord = new Vector2(0, 0);

            var meshPartBack = mesh.Back;
            meshPartBack.Vertices[0].TexCoord = new Vector2(1, 0);
            meshPartBack.Vertices[1].TexCoord = new Vector2(0, 0);
            meshPartBack.Vertices[2].TexCoord = new Vector2(0, 1);
            meshPartBack.Vertices[3].TexCoord = new Vector2(1, 1);

            return mesh;
        }

        static MeshPartDefinition CreateMeshPart(ref Vector3 normal)
        {
            var vertices = CreateVertexPositionNormalTexture(ref normal);
            var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            return new MeshPartDefinition
            {
                Vertices = vertices,
                Indices = indices
            };
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
