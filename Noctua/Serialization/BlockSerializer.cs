#region Using

using System;
using System.IO;
using Libra.IO;
using Noctua.Asset;
using Noctua.Models;

#endregion

namespace Noctua.Serialization
{
    public sealed class BlockSerializer : AssetSerializer<Block>
    {
        public override object ReadAsset(Stream stream, IResource resource)
        {
            var definition = ReadObject<BlockDefinition>(stream);

            var block = new Block
            {
                Name            = definition.Name,
                MeshPrototype   = Load<Mesh>(resource, definition.Mesh),
                Fluid           = definition.Fluid,
                ShadowCasting   = definition.ShadowCasting,
                Shape           = definition.Shape,
                Mass            = definition.Mass,
                StaticFriction  = definition.StaticFriction,
                DynamicFriction = definition.DynamicFriction,
                Restitution     = definition.Restitution
            };

            block.Tiles[Side.Top]       = Load<Tile>(resource, definition.TopTile);
            block.Tiles[Side.Bottom]    = Load<Tile>(resource, definition.BottomTile);
            block.Tiles[Side.Front]     = Load<Tile>(resource, definition.FrontTile);
            block.Tiles[Side.Back]      = Load<Tile>(resource, definition.BackTile);
            block.Tiles[Side.Left]      = Load<Tile>(resource, definition.LeftTile);
            block.Tiles[Side.Right]     = Load<Tile>(resource, definition.RightTile);
            block.BuildMesh();

            // 1 つでも半透明タイルを含んでいたら半透明ブロックとする。
            bool translucent = false;
            foreach (var tile in block.Tiles)
            {
                if (tile.Translucent)
                {
                    translucent = true;
                    break;
                }
            }
            block.Translucent = translucent;

            return block;
        }

        public override void WriteAsset(Stream stream, IResource resource, object asset)
        {
            var block = asset as Block;

            var definition = new BlockDefinition
            {
                Name            = block.Name,
                Mesh            = CreateRelativeUri(resource, block.MeshPrototype),
                TopTile         = CreateRelativeUri(resource, block.Tiles[Side.Top]),
                BottomTile      = CreateRelativeUri(resource, block.Tiles[Side.Bottom]),
                FrontTile       = CreateRelativeUri(resource, block.Tiles[Side.Front]),
                BackTile        = CreateRelativeUri(resource, block.Tiles[Side.Back]),
                LeftTile        = CreateRelativeUri(resource, block.Tiles[Side.Left]),
                RightTile       = CreateRelativeUri(resource, block.Tiles[Side.Right]),
                Fluid           = block.Fluid,
                ShadowCasting   = block.ShadowCasting,
                Shape           = block.Shape,
                Mass            = block.Mass,
                StaticFriction  = block.StaticFriction,
                DynamicFriction = block.DynamicFriction,
                Restitution     = block.Restitution
            };

            WriteObject(stream, definition);
        }
    }
}
