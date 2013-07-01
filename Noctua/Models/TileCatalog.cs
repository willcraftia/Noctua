#region Using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Libra;
using Libra.Graphics;

#endregion

namespace Noctua.Models
{
    public sealed class TileCatalog : KeyedCollection<byte, Tile>
    {
        public const int MaxTileCount = byte.MaxValue;

        public const int TextureSize = 256;

        public const int TileLength = 16;

        Color[] colorBuffer;

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

            EnsureColorBuffer();

            GetColorBuffer(context, tile.Texture);
            SetColorBuffer(context, TileMap, tile.Index);
        }

        public void ClearMaps(DeviceContext context)
        {
            for (byte i = 0; i < MaxTileCount; i++)
                ClearMaps(context, i);
        }

        public void ClearMaps(DeviceContext context, byte arrayIndex)
        {
            EnsureColorBuffer();
            ClearColorBuffer();

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
            texture.Initialize();
            return texture;
        }

        void GetColorBuffer(DeviceContext context, Texture2D source)
        {
            if (source == null) return;

            source.GetData(context, colorBuffer);
        }

        void SetColorBuffer(DeviceContext context, Texture2D destination, int arrayIndex)
        {
            destination.SetData(context, arrayIndex, 0, null, colorBuffer, 0, colorBuffer.Length);
        }

        void SetColorBuffer(DeviceContext context, Texture2D destination, int arrayIndex, ref Color color)
        {
            for (int i = 0; i < colorBuffer.Length; i++)
                colorBuffer[i] = color;

            SetColorBuffer(context, destination, arrayIndex);
        }

        void EnsureColorBuffer()
        {
            if (colorBuffer == null) colorBuffer = new Color[Tile.Size * Tile.Size];
        }

        void ClearColorBuffer()
        {
            for (int i = 0; i < colorBuffer.Length; i++) colorBuffer[i] = Color.Black;
        }

        #region ToString

        public override string ToString()
        {
            return "{Name:" + Name + "}";
        }

        #endregion
    }
}
