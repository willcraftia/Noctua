﻿#region Using

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

            Rectangle bounds;
            CalculateTileBounds(index, out bounds);

            EnsureColorBuffer();

            //------------------------
            // TileMap

            GetColorBuffer(context, tile.Texture);
            SetColorBuffer(context, TileMap, ref bounds);
        }

        public void ClearMaps(DeviceContext context)
        {
            for (byte i = 0; i < byte.MaxValue; i++)
                ClearMaps(context, i);
        }

        public void ClearMaps(DeviceContext context, byte index)
        {
            EnsureColorBuffer();
            ClearColorBuffer();

            Rectangle bounds;
            CalculateTileBounds(index, out bounds);

            SetColorBuffer(context, TileMap, ref bounds);
        }

        public Vector2 GetTexCoord(Vector2 baseTexCoord, byte index)
        {
            var result = baseTexCoord;

            result.X /= TileLength;
            result.Y /= TileLength;
            result.X += (index % TileLength) / (float) TileLength;
            result.Y += (index / TileLength) / (float) TileLength;

            return result;
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
            texture.Width = TextureSize;
            texture.Height = TextureSize;
            texture.Initialize();
            return texture;
        }

        void CalculateTileBounds(byte index, out Rectangle bounds)
        {
            bounds = new Rectangle
            {
                X = (index % TileLength) * Tile.Size,
                Y = (index / TileLength) * Tile.Size,
                Width = Tile.Size,
                Height = Tile.Size
            };
        }

        void GetColorBuffer(DeviceContext context, Texture2D texture)
        {
            if (texture == null) return;

            texture.GetData(context, colorBuffer);
        }

        void SetColorBuffer(DeviceContext context, Texture2D texture, ref Rectangle bounds)
        {
            texture.SetData(context, 0, bounds, colorBuffer, 0, colorBuffer.Length);
        }

        void SetColorBuffer(DeviceContext context, Texture2D texture, ref Rectangle bounds, ref Color color)
        {
            for (int i = 0; i < colorBuffer.Length; i++)
                colorBuffer[i] = color;
            
            SetColorBuffer(context, texture, ref bounds);
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
