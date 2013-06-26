#region Using

using System;
using System.IO;
using Libra;
using Libra.Graphics;
using Libra.IO;
using Musca;
using Pyxis;
using Noctua;
using Noctua.Asset;
using Noctua.Models;
using Noctua.Serialization;

#endregion

namespace Samples.AssetSerialization
{
    class Program
    {
        #region MockBiome

        class MockBiome : IBiome
        {
            public byte Index { get; set; }

            public INoiseSource DensityNoise
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public INoiseSource TerrainNoise
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public IResource Resource { get; set; }

            public float GetTemperature(int x, int z) { throw new NotSupportedException(); }

            public float GetHumidity(int x, int z) { throw new NotSupportedException(); }

            public BiomeElement GetBiomeElement(int x, int z) { throw new NotSupportedException(); }
        }

        #endregion

        #region MockNoise

        class MockNoise : INoiseSource
        {
            public float Sample(float x, float y, float z)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        static bool generateJson = false;

        static bool generateXml = true;

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

            var blocksPath = directoryPath + "/Blocks";
            if (!Directory.Exists(blocksPath)) Directory.CreateDirectory(blocksPath);

            var blockCatalogsPath = directoryPath + "/BlockCatalogs";
            if (!Directory.Exists(blockCatalogsPath)) Directory.CreateDirectory(blockCatalogsPath);

            var terrainsPath = directoryPath + "/Terrains";
            if (!Directory.Exists(terrainsPath)) Directory.CreateDirectory(terrainsPath);

            var biomesPath = directoryPath + "/Biomes";
            if (!Directory.Exists(biomesPath)) Directory.CreateDirectory(biomesPath);

            var biomeCatalogsPath = directoryPath + "/BiomeCatalogs";
            if (!Directory.Exists(biomeCatalogsPath)) Directory.CreateDirectory(biomeCatalogsPath);

            var biomeManagersPath = directoryPath + "/BiomeManagers";
            if (!Directory.Exists(biomeManagersPath)) Directory.CreateDirectory(biomeManagersPath);

            var chunkProceduresPath = directoryPath + "/ChunkProcedures";
            if (!Directory.Exists(chunkProceduresPath)) Directory.CreateDirectory(chunkProceduresPath);

            var regionsPath = directoryPath + "/Regions";
            if (!Directory.Exists(regionsPath)) Directory.CreateDirectory(regionsPath);

            var modelsPath = directoryPath + "/Models";
            if (!Directory.Exists(modelsPath)) Directory.CreateDirectory(modelsPath);

            var meshesPath = directoryPath + "/Meshes";
            if (!Directory.Exists(meshesPath)) Directory.CreateDirectory(meshesPath);

            var particlesPath = directoryPath + "/Particles";
            if (!Directory.Exists(particlesPath)) Directory.CreateDirectory(particlesPath);

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
                    new IndexedUriDefinition { Index = 1, Uri = "../Tiles/Dirt.xml" },
                    new IndexedUriDefinition { Index = 2, Uri = "../Tiles/GrassBottom.xml" },
                    new IndexedUriDefinition { Index = 3, Uri = "../Tiles/GrassSide.xml" },
                    new IndexedUriDefinition { Index = 4, Uri = "../Tiles/GrassTop.xml" },
                    new IndexedUriDefinition { Index = 5, Uri = "../Tiles/Mantle.xml" },
                    new IndexedUriDefinition { Index = 6, Uri = "../Tiles/Sand.xml" },
                    new IndexedUriDefinition { Index = 7, Uri = "../Tiles/Snow.xml" },
                    new IndexedUriDefinition { Index = 8, Uri = "../Tiles/Stone.xml" }
                }
                };
                SerializeAndDeserialize<TileCatalogDefinition>("TileCatalogs/Default", definition);
            }
            Console.WriteLine();

            #endregion

            #region ブロック定義

            Console.WriteLine("ブロック定義");
            {
                var cube = "../Meshes/Cube.xml";

                {
                    var tile = "../Tiles/Dirt.xml";
                    var definition = new BlockDefinition
                    {
                        Name = "Dirt",
                        Mesh = cube,
                        TopTile = tile,
                        BottomTile = tile,
                        FrontTile = tile,
                        BackTile = tile,
                        LeftTile = tile,
                        RightTile = tile,
                        Fluid = false,
                        ShadowCasting = true,
                        Shape = BlockShape.Cube,
                        Mass = 1,
                        StaticFriction = 0.5f,
                        DynamicFriction = 0.5f,
                        Restitution = 0.5f
                    };
                    SerializeAndDeserialize<BlockDefinition>("Blocks/Dirt", definition);
                }

                {
                    var tileBase = "../Tiles/Grass";
                    var tileSide = tileBase + "Side.xml";
                    var definition = new BlockDefinition
                    {
                        Name = "Grass",
                        Mesh = cube,
                        TopTile = tileBase + "Top.xml",
                        BottomTile = tileBase + "Bottom.xml",
                        FrontTile = tileSide,
                        BackTile = tileSide,
                        LeftTile = tileSide,
                        RightTile = tileSide,
                        Fluid = false,
                        ShadowCasting = true,
                        Shape = BlockShape.Cube,
                        Mass = 1,
                        StaticFriction = 0.5f,
                        DynamicFriction = 0.5f,
                        Restitution = 0.5f
                    };
                    SerializeAndDeserialize<BlockDefinition>("Blocks/Grass", definition);
                }

                {
                    var tile = "../Tiles/Mantle.xml";
                    var definition = new BlockDefinition
                    {
                        Name = "Mantle",
                        Mesh = cube,
                        TopTile = tile,
                        BottomTile = tile,
                        FrontTile = tile,
                        BackTile = tile,
                        LeftTile = tile,
                        RightTile = tile,
                        Fluid = false,
                        ShadowCasting = true,
                        Shape = BlockShape.Cube,
                        Mass = 1,
                        StaticFriction = 0.5f,
                        DynamicFriction = 0.5f,
                        Restitution = 0.5f
                    };
                    SerializeAndDeserialize<BlockDefinition>("Blocks/Mantle", definition);
                }

                {
                    var tile = "../Tiles/Sand.xml";
                    var definition = new BlockDefinition
                    {
                        Name = "Sand",
                        Mesh = cube,
                        TopTile = tile,
                        BottomTile = tile,
                        FrontTile = tile,
                        BackTile = tile,
                        LeftTile = tile,
                        RightTile = tile,
                        Fluid = false,
                        ShadowCasting = true,
                        Shape = BlockShape.Cube,
                        Mass = 1,
                        StaticFriction = 0.5f,
                        DynamicFriction = 0.5f,
                        Restitution = 0.5f
                    };
                    SerializeAndDeserialize<BlockDefinition>("Blocks/Sand", definition);
                }

                {
                    var tile = "../Tiles/Snow.xml";
                    var definition = new BlockDefinition
                    {
                        Name = "Snow",
                        Mesh = cube,
                        TopTile = tile,
                        BottomTile = tile,
                        FrontTile = tile,
                        BackTile = tile,
                        LeftTile = tile,
                        RightTile = tile,
                        Fluid = false,
                        ShadowCasting = true,
                        Shape = BlockShape.Cube,
                        Mass = 1,
                        StaticFriction = 0.5f,
                        DynamicFriction = 0.5f,
                        Restitution = 0.5f
                    };
                    SerializeAndDeserialize<BlockDefinition>("Blocks/Snow", definition);
                }

                {
                    var tile = "../Tiles/Stone.xml";
                    var definition = new BlockDefinition
                    {
                        Name = "Stone",
                        Mesh = cube,
                        TopTile = tile,
                        BottomTile = tile,
                        FrontTile = tile,
                        BackTile = tile,
                        LeftTile = tile,
                        RightTile = tile,
                        Fluid = false,
                        ShadowCasting = true,
                        Shape = BlockShape.Cube,
                        Mass = 1,
                        StaticFriction = 0.5f,
                        DynamicFriction = 0.5f,
                        Restitution = 0.5f
                    };
                    SerializeAndDeserialize<BlockDefinition>("Blocks/Stone", definition);
                }
            }
            Console.WriteLine();

            #endregion

            #region ブロック カタログ定義

            Console.WriteLine("ブロック カタログ定義");
            {
                var definition = new BlockCatalogDefinition
                {
                    Name = "Default",
                    Entries = new IndexedUriDefinition[]
                    {
                        new IndexedUriDefinition { Index = 1, Uri = "../Blocks/Dirt.xml" },
                        new IndexedUriDefinition { Index = 2, Uri = "../Blocks/Grass.xml" },
                        new IndexedUriDefinition { Index = 3, Uri = "../Blocks/Mantle.xml" },
                        new IndexedUriDefinition { Index = 4, Uri = "../Blocks/Sand.xml" },
                        new IndexedUriDefinition { Index = 5, Uri = "../Blocks/Snow.xml" },
                        new IndexedUriDefinition { Index = 6, Uri = "../Blocks/Stone.xml" }
                    },
                    Dirt = 1,
                    Grass = 2,
                    Mantle = 3,
                    Sand = 4,
                    Snow = 5,
                    Stone = 6
                };
                SerializeAndDeserialize<BlockCatalogDefinition>("BlockCatalogs/Default", definition);
            }
            Console.WriteLine();

            #endregion

            #region 地形ノイズ コンポーネント

            Console.WriteLine("地形ノイズ コンポーネント");
            {
                // デバッグのし易さのために、各ノイズ インスタンスのコンポーネント名を明示する。
                var moduleTypeRegistry = new ModuleTypeRegistry();
                NoiseTypes.SetTypeDefinitionNames(moduleTypeRegistry);
                var componentInfoManager = new ModuleInfoManager(moduleTypeRegistry);
                var builder = new ModuleBundleBuilder(componentInfoManager);

                //------------------------------------------------------------
                //
                // フェード曲線
                //

                // デフォルトでは Perlin.FadeCurve は静的フィールドで共有状態なので、
                // ここで一つだけビルダへ登録しておく。
                builder.Add("DefaultFadeCurve", Perlin.DefaultFadeCurve);

                //------------------------------------------------------------
                //
                // 定数
                //

                var constZero = new Const
                {
                    Name = "Const Zero",
                    Value = 0
                };
                builder.Add("ConstZero", constZero);

                var constOne = new Const
                {
                    Name = "Const One",
                    Value = 1
                };
                builder.Add("ConstOne", constOne);

                //------------------------------------------------------------
                //
                // 低地形状
                //

                var lowlandPerlin = new Perlin
                {
                    Name = "Lowland Perlin",
                    Seed = 100
                };
                builder.Add("LowlandPerlin", lowlandPerlin);

                var lowlandFractal = new Billow
                {
                    Name = "Lowland Fractal",
                    OctaveCount = 2,
                    Persistence = 0.1f,
                    Source = lowlandPerlin
                };
                builder.Add("LowlandFractal", lowlandFractal);

                var lowlandScaleBias = new ScaleBias
                {
                    Name = "Lowland ScaleBias",
                    Scale = 0.225f,
                    Bias = -0.75f,
                    Source = lowlandFractal
                };
                builder.Add("LowlandScaleBias", lowlandScaleBias);

                // Y のスケールを 0 にすることとは、Y に 0 を指定することと同じ。
                // つまり、出力をハイトマップ上の高さとして扱うということ。
                var lowlandShape = new ScalePoint
                {
                    Name = "Lowland Shape",
                    ScaleY = 0,
                    Source = lowlandScaleBias
                };
                builder.Add("LowlandShape", lowlandShape);

                //------------------------------------------------------------
                //
                // 高地形状
                //

                var highlandPerlin = new Perlin
                {
                    Name = "Highland Perlin",
                    Seed = 200
                };
                builder.Add("HighlandPerlin", highlandPerlin);

                var highlandFractal = new SumFractal
                {
                    Name = "Highland Fractal",
                    OctaveCount = 4,
                    Frequency = 2,
                    Source = highlandPerlin
                };
                builder.Add("HighlandFractal", highlandFractal);

                var highlandShape = new ScalePoint
                {
                    Name = "Highland Shape",
                    ScaleY = 0,
                    Source = highlandFractal
                };
                builder.Add("HighlandShape", highlandShape);

                //------------------------------------------------------------
                //
                // 山地形状
                //

                var mountainPerlin = new Perlin
                {
                    Name = "Mountain Perlin",
                    Seed = 300
                };
                builder.Add("MountainPerlin", mountainPerlin);

                var mountainFractal = new RidgedMultifractal
                {
                    Name = "Mountain Fractal",
                    OctaveCount = 2,
                    Source = mountainPerlin
                };
                builder.Add("MountainFractal", mountainFractal);

                var mountainScaleBias = new ScaleBias
                {
                    Name = "Mountain ScaleBias",
                    Scale = 0.5f,
                    Bias = 0.25f,
                    Source = mountainFractal
                };
                builder.Add("MountainScaleBias", mountainScaleBias);

                var mountainShape = new ScalePoint
                {
                    Name = "Mountain Shape",
                    ScaleY = 0,
                    Source = mountainScaleBias
                };
                builder.Add("MountainShape", mountainShape);

                //------------------------------------------------------------
                //
                // 地形形状
                //

                var terrainTypePerlin = new Perlin
                {
                    Name = "Terrain Type Perlin",
                    Seed = 400
                };
                builder.Add("TerrainTypePerlin", terrainTypePerlin);

                var terrainTypeFractal = new SumFractal
                {
                    Name = "Terrain Type Fractal",
                    Frequency = 0.7f,
                    Lacunarity = 0.7f,
                    Source = terrainTypePerlin
                };
                builder.Add("TerrainTypeFractal", terrainTypeFractal);

                var terrainTypeScalePoint = new ScalePoint
                {
                    Name = "Terrain Type ScalePoint",
                    ScaleY = 0,
                    Source = terrainTypeFractal
                };
                builder.Add("TerrainTypeScalePoint", terrainTypeScalePoint);

                // 地形選択ノイズは同時に複数のモジュールから参照されるためキャッシュ。
                var terrainType = new Cache
                {
                    Name = "Terrain Type Cache",
                    Source = terrainTypeScalePoint
                };
                builder.Add("TerrainType", terrainType);

                //------------------------------------------------------------
                //
                // 高地山地ブレンド
                //

                var highlandMountainSelect = new Select
                {
                    Name = "Highland or Mountain Select",
                    LowerSource = highlandShape,
                    LowerBound = 0,
                    UpperSource = mountainShape,
                    UpperBound = 1000,
                    Controller = terrainType,
                    EdgeFalloff = 0.2f
                };
                builder.Add("HighlandMountainSelect", highlandMountainSelect);

                //------------------------------------------------------------
                //
                // 最終地形ブレンド
                //

                var terrainSelect = new Select
                {
                    Name = "Terrain Select",
                    LowerSource = lowlandShape,
                    LowerBound = 0,
                    UpperSource = highlandMountainSelect,
                    UpperBound = 1000,
                    Controller = terrainType,
                    EdgeFalloff = 0.5f
                };
                builder.Add("TerrainSelect", terrainSelect);

                var terrainSelectCache = new Cache
                {
                    Name = "Terrain Select Cache",
                    Source = terrainSelect
                };
                builder.Add("TerrainSelectCache", terrainSelectCache);

                //------------------------------------------------------------
                //
                // 地形密度
                //

                var terrainDensity = new SelectTerrainDensity
                {
                    Name = "Terrain Density",
                    Source = terrainSelectCache
                };
                builder.Add("TerrainDensity", terrainDensity);

                ////------------------------------------------------------------
                ////
                //// 洞窟形状
                ////

                //var cavePerlin = new Perlin
                //{
                //    Name = "Cave Perlin",
                //    Seed = 500
                //};
                //builder.Add("CavePerlin", cavePerlin);

                //var caveShape = new RidgedMultifractal
                //{
                //    Name = "Cave Shape",
                //    OctaveCount = 1,
                //    Frequency = 4,
                //    Source = cavePerlin
                //};
                //builder.Add("CaveShape", caveShape);

                //var caveAttenuateBias = new ScaleBias
                //{
                //    Name = "Cave Attenuate ScaleBias",
                //    Bias = 0.45f,
                //    Source = terrainSelectCache
                //};
                //builder.Add("CaveAttenuateBias", caveAttenuateBias);

                //var caveShapeAttenuate = new Multiply
                //{
                //    Name = "Cave Shape Attenuate",
                //    Source0 = caveShape,
                //    Source1 = caveAttenuateBias
                //};
                //builder.Add("CaveShapeAttenuate", caveShapeAttenuate);

                //var cavePerturbPerlin = new Perlin
                //{
                //    Name = "Cave Perturb Perlin",
                //    Seed = 600
                //};
                //builder.Add("CavePerturbPerlin", cavePerturbPerlin);

                //var cavePerturnFractal = new SumFractal
                //{
                //    Name = "Cave Perturb Fractal",
                //    OctaveCount = 6,
                //    Frequency = 3,
                //    Source = cavePerturbPerlin
                //};
                //builder.Add("CavePerturnFractal", cavePerturnFractal);

                //var cavePerturbScaleBias = new ScaleBias
                //{
                //    Name = "Cave Perturb ScaleBias",
                //    Scale = 0.5f,
                //    Source = cavePerturnFractal
                //};
                //builder.Add("CavePerturbScaleBias", cavePerturbScaleBias);

                //var cavePerturb = new Displace
                //{
                //    Name = "Cave Perturb",
                //    DisplaceX = cavePerturbScaleBias,
                //    DisplaceY = constZero,
                //    DisplaceZ = constZero,
                //    Source = caveShapeAttenuate
                //};
                //builder.Add("CavePerturb", cavePerturb);

                ////------------------------------------------------------------
                ////
                //// 洞窟密度
                ////

                //var caveDensity = new Select
                //{
                //    Name = "Cave Density",
                //    LowerBound = 0.2f,
                //    LowerSource = constOne,
                //    UpperBound = 1000,
                //    UpperSource = constZero,
                //    Controller = cavePerturb
                //};
                //builder.Add("CaveDensity", caveDensity);

                //------------------------------------------------------------
                //
                // 洞窟形状
                //

                var cavePerlin = new Perlin
                {
                    Name = "Cave Perlin",
                    Seed = 500
                };
                builder.Add("CavePerlin", cavePerlin);

                var caveShape = new SumFractal
                {
                    Name = "Cave Shape",
                    OctaveCount = 1,
                    Frequency = 4,
                    Lacunarity = 4,
                    Source = cavePerlin
                };
                builder.Add("CaveShape", caveShape);

                // サンプリング範囲を狭めて洞窟の広さを調整。
                var caveScalePoint = new ScalePoint
                {
                    Name = "Cave ScalePoint",
                    ScaleX = 0.25f,
                    ScaleY = 0.25f,
                    ScaleZ = 0.25f,
                    Source = caveShape
                };
                builder.Add("CaveScalePoint", caveScalePoint);

                //------------------------------------------------------------
                //
                // 洞窟密度
                //

                var caveDensity = new Select
                {
                    Name = "Cave Density",
                    LowerBound = 0.1f,
                    LowerSource = constOne,
                    UpperBound = 1000,
                    UpperSource = constZero,
                    Controller = caveScalePoint
                };
                builder.Add("CaveDensity", caveDensity);

                //------------------------------------------------------------
                //
                // 密度ブレンド
                //

                var finalDensity = new Multiply
                {
                    Name = "Final Density",
                    Source0 = terrainDensity,
                    Source1 = caveDensity
                };
                builder.Add("FinalDensity", finalDensity);

                //------------------------------------------------------------
                //
                // ブロック用座標変換
                //

                // プロシージャはブロック空間座標で XYZ を指定するため、
                // これらをノイズ空間のスケールへ変更。
                //
                // 16 という数値は、チャンク サイズに一致しているものの、
                // チャンク サイズに一致させるべきものではなく、
                // 期待する結果へノイズをスケーリングできれば何でも良い。
                // ただし、ブロック空間座標は int、ノイズ空間は float であるため、
                // 連続性のあるノイズを抽出するには 1 未満の float のスケールとする必要がある。
                //
                // XZ スケールは 16 から 32 辺りが妥当。
                //      小さすぎると微細な高低差が増えすぎる傾向。
                //      大きすぎると高低差が少なくなり過ぎる傾向。
                // Y スケールは 16 以下が妥当。
                //      16 以上は高低差が激しくなり過ぎる傾向。
                var finalScale = new ScalePoint
                {
                    Name = "Final Scale",
                    ScaleX = 1 / 16f,
                    ScaleY = 1 / 16f,
                    ScaleZ = 1 / 16f,
                    Source = finalDensity
                };
                builder.Add("FinalScale", finalScale);

                // 地形の起伏が現れる Y の位置へブロック空間座標を移動。
                // フラクタル ノイズには [-1, 1] を越える値を返すものもあるため、
                // 期待する Y の位置よりも少し下へ移動させるよう補正した方が良い。
                var target = new Displace
                {
                    Name = "Terrain Offset",
                    DisplaceX = constZero,
                    DisplaceY = new Const
                    {
                        Value = -(256 - 16 - 8)
                    },
                    DisplaceZ = constZero,
                    Source = finalScale
                };
                builder.Add("Target", target);

                ModuleBundleDefinition bundle = builder.Build();

                SerializeAndDeserialize<ModuleBundleDefinition>("Terrains/Default", bundle);
            }
            Console.WriteLine();

            #endregion

            #region バイオーム コンポーネント

            Console.WriteLine("バイオーム コンポーネント");
            {
                // デバッグのし易さのために、各ノイズ インスタンスのコンポーネント名を明示する。
                var moduleTypeRegistry = new ModuleTypeRegistry();
                NoiseTypes.SetTypeDefinitionNames(moduleTypeRegistry);
                moduleTypeRegistry.SetTypeDefinitionName(typeof(DefaultBiome));
                moduleTypeRegistry.SetTypeDefinitionName(typeof(DefaultBiome.Range));
                var componentInfoManager = new ModuleInfoManager(moduleTypeRegistry);
                var builder = new ModuleBundleBuilder(componentInfoManager);

                // デフォルトでは Perlin.FadeCurve は静的フィールドで共有状態なので、
                // ここで一つだけビルダへ登録しておく。
                builder.Add("DefaultFadeCurve", Perlin.DefaultFadeCurve);

                //------------------------------------------------------------
                //
                // 湿度
                //

                var humidityPerlin = new Perlin
                {
                    Seed = 100
                };
                var humidityFractal = new SumFractal
                {
                    Source = humidityPerlin,
                    OctaveCount = 3
                };
                var humidity = new ScaleBias
                {
                    Scale = 0.5f,
                    Bias = 0.5f,
                    Source = humidityFractal
                };
                builder.Add("HumidityPerlin", humidityPerlin);
                builder.Add("HumidityFractal", humidityFractal);
                builder.Add("Humidity", humidity);

                //------------------------------------------------------------
                //
                // 気温
                //

                var temperaturePerlin = new Perlin
                {
                    Seed = 200
                };
                var temperatureFractal = new SumFractal
                {
                    Source = temperaturePerlin,
                    OctaveCount = 3
                };
                var temperature = new ScaleBias
                {
                    Scale = 0.5f,
                    Bias = 0.5f,
                    Source = temperatureFractal
                };
                builder.Add("TemperaturePerlin", temperaturePerlin);
                builder.Add("TemperatureFractal", temperatureFractal);
                builder.Add("Temperature", temperature);

                //------------------------------------------------------------
                //
                // バイオーム
                //

                var biome = new DefaultBiome
                {
                    Name = "Default Biome",
                    HumidityNoise = humidity,
                    TemperatureNoise = temperature,
                    TerrainNoise = new MockNoise()
                };
                builder.Add("Target", biome);
                builder.AddExternalReference(biome.TerrainNoise, "title:Assets/Terrains/Default.xml");

                ModuleBundleDefinition bundle = builder.Build();

                SerializeAndDeserialize<ModuleBundleDefinition>("Biomes/Default", bundle);
            }
            Console.WriteLine();

            #endregion

            #region バイオーム カタログ定義

            Console.WriteLine("バイオーム カタログ定義");
            {
                var definition = new BiomeCatalogDefinition
                {
                    Name = "Default",
                    Entries = new IndexedUriDefinition[]
                    {
                        new IndexedUriDefinition
                        {
                            Index = 0, Uri = "../Biomes/Default.xml"
                        }
                    }
                };
                SerializeAndDeserialize<BiomeCatalogDefinition>("BiomeCatalogs/Default", definition);
            }
            Console.WriteLine();

            #endregion

            #region 単一バイオーム マネージャ コンポーネント

            Console.WriteLine("単一バイオーム マネージャ コンポーネント");
            {
                var biomeManager = new SingleBiomeManager
                {
                    Biome = new MockBiome()
                };

                var moduleTypeRegistry = new ModuleTypeRegistry();
                moduleTypeRegistry.SetTypeDefinitionName(typeof(SingleBiomeManager));
                var moduleInfoManager = new ModuleInfoManager(moduleTypeRegistry);
                var builder = new ModuleBundleBuilder(moduleInfoManager);
                builder.AddExternalReference(biomeManager.Biome, "../Biomes/Default.xml");
                builder.Add("Target", biomeManager);

                var bundle = builder.Build();

                SerializeAndDeserialize<ModuleBundleDefinition>("BiomeManagers/Single", bundle);
            }
            Console.WriteLine();

            #endregion

            #region 平坦地形生成コンポーネント

            Console.WriteLine("平坦地形生成コンポーネント");
            {
                var procedure = new FlatTerrainProcedure
                {
                    Height = 156
                };

                var moduleTypeRegistry = new ModuleTypeRegistry();
                moduleTypeRegistry.SetTypeDefinitionName(typeof(FlatTerrainProcedure));

                var moduleInfoManager = new ModuleInfoManager(moduleTypeRegistry);
                var builder = new ModuleBundleBuilder(moduleInfoManager);
                builder.Add("Target", procedure);

                var bundle = builder.Build();

                SerializeAndDeserialize<ModuleBundleDefinition>("ChunkProcedures/FlatTerrain", bundle);
            }
            Console.WriteLine();

            #endregion

            #region ノイズ地形生成コンポーネント

            Console.WriteLine("ノイズ地形生成コンポーネント");
            {
                var procedure = new DefaultTerrainProcedure();

                var moduleTypeRegistry = new ModuleTypeRegistry();
                NoiseTypes.SetTypeDefinitionNames(moduleTypeRegistry);
                moduleTypeRegistry.SetTypeDefinitionName(typeof(DefaultTerrainProcedure));

                var moduleInfoManager = new ModuleInfoManager(moduleTypeRegistry);
                var builder = new ModuleBundleBuilder(moduleInfoManager);
                builder.Add("Target", procedure);

                var bundle = builder.Build();

                SerializeAndDeserialize<ModuleBundleDefinition>("ChunkProcedures/NoiseTerrain", bundle);
            }
            Console.WriteLine();

            #endregion

            #region リージョン定義

            Console.WriteLine("リージョン定義");
            {
                var definition = new RegionDefinition
                {
                    Name = "Default",
                    Box = new IntBoundingBox
                    {
                        Min = new IntVector3(-128, 0, -128),
                        Max = new IntVector3(128, 16, 128)
                    },
                    TileCatalog = "../TileCatalogs/Default.xml",
                    BlockCatalog = "../BlockCatalogs/Default.xml",
                    BiomeManager = "../BiomeManagers/Single.xml",
                    ChunkProcedures = new string[]
                    {
                        "../ChunkProcedures/NoiseTerrain.xml"
                    }
                };
                SerializeAndDeserialize<RegionDefinition>("Regions/Default", definition);
            }
            Console.WriteLine();

            #endregion

            #region スカイ スフィア定義

            Console.WriteLine("スカイ スフィア定義");
            {
                var definition = new SkySphereDefinition
                {
                    Name = "DefaultSkySphere",
                    SunVisible = true,
                    SunThreshold = 0.999f
                };
                SerializeAndDeserialize<SkySphereDefinition>("Models/SkySphere", definition);
            }
            Console.WriteLine();

            #endregion

            #region レンズ フレア メッシュ定義

            Console.WriteLine("レンズ フレア メッシュ定義");
            {
                var definition = new LensFlareMeshDefinition
                {
                    Name = "LensFlare",
                    QuerySize = 100,
                    Flares = new LensFlareElementDefinition []
                    {
                        new LensFlareElementDefinition(-0.5f, 0.7f, new Color( 50,  25,  50).ToVector3(), "LensFlare1.png"),
                        new LensFlareElementDefinition( 0.3f, 0.4f, new Color(100, 255, 200).ToVector3(), "LensFlare1.png"),
                        new LensFlareElementDefinition( 1.2f, 1.0f, new Color(100,  50,  50).ToVector3(), "LensFlare1.png"),
                        new LensFlareElementDefinition( 1.5f, 1.5f, new Color( 50, 100,  50).ToVector3(), "LensFlare1.png"),

                        new LensFlareElementDefinition(-0.3f, 0.7f, new Color(200,  50,  50).ToVector3(), "LensFlare2.png"),
                        new LensFlareElementDefinition( 0.6f, 0.9f, new Color( 50, 100,  50).ToVector3(), "LensFlare2.png"),
                        new LensFlareElementDefinition( 0.7f, 0.4f, new Color( 50, 200, 200).ToVector3(), "LensFlare2.png"),

                        new LensFlareElementDefinition(-0.7f, 0.7f, new Color( 50, 100,  25).ToVector3(), "LensFlare3.png"),
                        new LensFlareElementDefinition( 0.0f, 0.6f, new Color( 25,  25,  25).ToVector3(), "LensFlare3.png"),
                        new LensFlareElementDefinition( 2.0f, 1.4f, new Color( 25,  50, 100).ToVector3(), "LensFlare3.png"),
                    },
                    GlowTexture = "LensFlareGlow.png",
                    GlowSize = 400,
                    LightName = "Sun",
                };
                SerializeAndDeserialize<LensFlareMeshDefinition>("Models/LensFlareMesh", definition);
            }
            Console.WriteLine();

            #endregion

            #region グラフィックス設定定義

            Console.WriteLine("グラフィックス設定定義");
            {
                var settings = new GraphicsSettings
                {
                    Shadow = new GraphicsShadowSettings(),
                    Dof = new GraphicsDofSettings()
                };
                SerializeAndDeserialize<GraphicsSettings>("GraphicsSettings", settings);
            }
            Console.WriteLine();

            #endregion

            #region チャンク定義

            Console.WriteLine("チャンク定義");
            {
                var settings = new ChunkSettings
                {
                    MinActiveRange = 8,
                    MaxActiveRange = 9
                };
                var definition = new ChunkSettingsDefinition
                {
                    ChunkSize = settings.ChunkSize,
                    VertexBuildConcurrencyLevel = settings.VertexBuildConcurrencyLevel,
                    UpdateBufferCountPerFrame = settings.UpdateBufferCountPerFrame,
                    MinActiveRange = settings.MinActiveRange,
                    MaxActiveRange = settings.MaxActiveRange,
                    ChunkStoreType = settings.ChunkStoreType,

                    ActivationCapacity = settings.PartitionManager.ActivationCapacity,
                    PassivationCapacity = settings.PartitionManager.PassivationCapacity,
                    PassivationSearchCapacity = settings.PartitionManager.PassivationSearchCapacity,
                    PriorActiveDistance = settings.PartitionManager.PriorActiveDistance,
                    ClusterSize = settings.PartitionManager.ClusterSize
                };

                SerializeAndDeserialize<ChunkSettingsDefinition>("ChunkSettings", definition);
            }
            Console.WriteLine();

            #endregion

            #region シーン設定定義

            Console.WriteLine("シーン設定定義");
            {
                var settings = new SceneSettings();
                settings.SkyColors.AddColors(new TimeColor[]
                {
                    new TimeColor(0,        Color.Black.ToVector3()),
                    new TimeColor(0.15f,    Color.Black.ToVector3()),
                    new TimeColor(0.25f,    Color.CornflowerBlue.ToVector3()),
                    new TimeColor(0.5f,     Color.CornflowerBlue.ToVector3()),
                    new TimeColor(0.75f,    Color.CornflowerBlue.ToVector3()),
                    new TimeColor(0.84f,    Color.Black.ToVector3()),
                    new TimeColor(1,        Color.Black.ToVector3())
                });
                settings.AmbientLightColors.AddColors(new TimeColor[]
                {
                    new TimeColor(0,        new Vector3(0.1f)),
                    new TimeColor(0.15f,    new Vector3(0.1f)),
                    new TimeColor(0.5f,     new Vector3(1)),
                    new TimeColor(0.84f,    new Vector3(0.1f)),
                    new TimeColor(1,        new Vector3(0.1f))
                });

                var definition = new SceneSettingsDefinition
                {
                    MidnightSunDirection = settings.MidnightSunDirection,
                    MidnightMoonDirection = settings.MidnightMoonDirection,
                    ShadowColor = settings.ShadowColor,
                    Sunlight = new DirectionalLightDefinition
                    {
                        Enabled = settings.Sunlight.Enabled,
                        DiffuseColor = settings.Sunlight.DiffuseColor,
                        SpecularColor = settings.Sunlight.SpecularColor
                    },
                    Moonlight = new DirectionalLightDefinition
                    {
                        Enabled = settings.Moonlight.Enabled,
                        DiffuseColor = settings.Moonlight.DiffuseColor,
                        SpecularColor = settings.Moonlight.SpecularColor
                    },
                    FogEnabled = settings.FogEnabled,
                    FogStart = settings.FogStart,
                    FogEnd = settings.FogEnd,
                    SecondsPerDay = settings.SecondsPerDay,
                    TimeStopped = settings.TimeStopped,
                    FixedSecondsPerDay = settings.FixedSecondsPerDay
                };

                if (settings.SkyColors.Count != 0)
                    definition.SkyColors = settings.SkyColors.ToArray();

                if (settings.AmbientLightColors.Count != 0)
                    definition.AmbientLightColors = settings.AmbientLightColors.ToArray();

                SerializeAndDeserialize<SceneSettingsDefinition>("SceneSettings", definition);
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

            #region 降雪パーティクル定義

            Console.WriteLine("降雪パーティクル");
            {
                var definition = new ParticleSystemDefinition
                {
                    Name = "Snow",
                    MaxParticleCount = 4000,
                    Duration = 5,
                    DurationRandomness = 0,
                    MinHorizontalVelocity = 0,
                    MaxHorizontalVelocity = 0,
                    MinVerticalVelocity = -10,
                    MaxVerticalVelocity = -10,
                    Gravity = new Vector3(-1, -1, 0),
                    EndVelocity = 1,
                    MinColor = Color.White.ToVector4(),
                    MaxColor = Color.White.ToVector4(),
                    MinRotateSpeed = 0,
                    MaxRotateSpeed = 0,
                    MinStartSize = 0.5f,
                    MaxStartSize = 0.5f,
                    MinEndSize = 0.2f,
                    MaxEndSize = 0.2f,
                    Texture = "title:Assets/Particles/Snow.png",
                    BlendState = BlendState.AlphaBlend
                };
                SerializeAndDeserialize<ParticleSystemDefinition>("Particles/Snow", definition);
            }
            Console.WriteLine();

            #endregion

            #region 降雨パーティクル定義

            Console.WriteLine("降雨パーティクル定義");
            {
                var definition = new ParticleSystemDefinition
                {
                    Name = "Rain",
                    MaxParticleCount = 8000,
                    Duration = 2,
                    DurationRandomness = 0,
                    MinHorizontalVelocity = 0,
                    MaxHorizontalVelocity = 0,
                    MinVerticalVelocity = -50,
                    MaxVerticalVelocity = -50,
                    Gravity = new Vector3(-1, -1, 0),
                    EndVelocity = 1,
                    MinColor = Color.White.ToVector4(),
                    MaxColor = Color.White.ToVector4(),
                    MinRotateSpeed = 0,
                    MaxRotateSpeed = 0,
                    MinStartSize = 0.5f,
                    MaxStartSize = 0.5f,
                    MinEndSize = 0.5f,
                    MaxEndSize = 0.5f,
                    Texture = "title:Assets/Particles/Rain.png",
                    BlendState = BlendState.AlphaBlend
                };
                SerializeAndDeserialize<ParticleSystemDefinition>("Particles/Rain", definition);
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
            if (generateJson)
            {
                JsonObjectSerializer.Instance.Indent = true;
                JsonObjectSerializer.Instance.IndentChar = '\t';

                var jsonPath = Serialize(JsonObjectSerializer.Instance, baseFileName, jsonExtension, graph);

                var jsonResult = Deserialize(JsonObjectSerializer.Instance, jsonPath, typeof(T));
            }

            if (generateXml)
            {
                XmlObjectSerializer.Instance.Indent = true;
                XmlObjectSerializer.Instance.IndentChar = '\t';

                var xmlPath = Serialize(XmlObjectSerializer.Instance, baseFileName, xmlExtension, graph);

                var xmlResult = Deserialize(XmlObjectSerializer.Instance, xmlPath, typeof(T));
            }

            Console.WriteLine("成功: {0}", baseFileName);
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
