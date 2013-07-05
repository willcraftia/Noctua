#region Using

using System;
using System.Collections.ObjectModel;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    public sealed class TileCatalog : KeyedCollection<byte, Tile>
    {
        public const int MaxTileCount = byte.MaxValue;

        const int TextureSize = 256;

        const int TileLength = 16;

        const int TileMipLevels = 5;

        Color[][] tileMipChain;

        public string Name { get; set; }

        public Texture2D TileMap { get; private set; }

        public TileCatalog(Device device)
        {
            if (device == null) throw new ArgumentNullException("device");

            TileMap = CreateMap(device);
        }

        //
        // TODO
        //
        // DrawMaps と ClearMaps は、後で効率を考えて修正すること。
        //

        public void DrawMaps(DeviceContext context)
        {
            for (int i = 0; i < Count; i++)
                DrawMaps(context, Items[i].Index);
        }

        public void DrawMaps(DeviceContext context, byte index)
        {
            var tile = this[index];

            EnsureTileMipChain();

            GetTileMipChain(context, tile.Texture);
            SetColorBuffer(context, TileMap, tile.Index);
        }

        public void ClearMaps(DeviceContext context)
        {
            for (byte i = 0; i < MaxTileCount; i++)
                ClearMaps(context, i);
        }

        public void ClearMaps(DeviceContext context, byte arrayIndex)
        {
            EnsureTileMipChain();
            ClearTileMipChain();

            SetColorBuffer(context, TileMap, arrayIndex);
        }

        protected override byte GetKeyForItem(Tile item)
        {
            return item.Index;
        }

        protected override void SetItem(int index, Tile item)
        {
            item.Catalog = this;

            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, Tile item)
        {
            item.Catalog = this;

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].Catalog = null;

            base.RemoveItem(index);
        }

        Texture2D CreateMap(Device device)
        {
            var texture = device.CreateTexture2D();
            texture.Width = Tile.Size;
            texture.Height = Tile.Size;
            texture.ArraySize = MaxTileCount;
            texture.MipLevels = TileMipLevels;
            texture.Initialize();
            return texture;
        }

        void GetTileMipChain(DeviceContext context, Texture2D source)
        {
            if (source == null) return;

            for (int i = 0; i < tileMipChain.Length; i++)
            {
                var mip = tileMipChain[i];
                context.GetData(source, 0, i, null, mip, 0, mip.Length);
            }
        }

        void SetColorBuffer(DeviceContext context, Texture2D destination, int arrayIndex)
        {
            for (int i = 0; i < tileMipChain.Length; i++)
            {
                var mip = tileMipChain[i];
                context.SetData(destination, arrayIndex, i, null, mip, 0, mip.Length);
            }
        }

        void EnsureTileMipChain()
        {
            if (tileMipChain == null)
            {
                tileMipChain = new Color[TileMipLevels][];

                int size = Tile.Size;
                for (int i = 0; i < TileMipLevels; i++)
                {
                    tileMipChain[i] = new Color[size * size];
                    size /= 2;
                }
            }
        }

        void ClearTileMipChain()
        {
            for (int i = 0; i < tileMipChain.Length; i++)
            {
                var mip = tileMipChain[i];
                for (int j = 0; j < mip.Length; j++)
                    mip[j] = Color.Black;
            }
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
