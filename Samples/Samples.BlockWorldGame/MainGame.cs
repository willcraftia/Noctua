#region Using

using System;
using Libra;
using Libra.Games;
using Libra.Graphics;
using Libra.Input;
using Libra.Xnb;
using Noctua.Models;

#endregion

namespace Samples.BlockWorldGame
{
    public sealed class MainGame : Game
    {
        /// <summary>
        /// ウィンドウの幅。
        /// </summary>
        const int WindowWidth = 1280;

        /// <summary>
        /// ウィンドウの高さ。
        /// </summary>
        const int WindowHeight = 720;

        /// <summary>
        /// Libra のグラフィックス マネージャ。
        /// </summary>
        GraphicsManager graphicsManager;

        /// <summary>
        /// Libra の XNA コンテント マネージャ。
        /// </summary>
        XnbManager content;

        /// <summary>
        /// スプライト バッチ。
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// フォント。
        /// </summary>
        SpriteFont spriteFont;

        /// <summary>
        /// 前回の更新処理におけるキーボード状態。
        /// </summary>
        KeyboardState lastKeyboardState;

        /// <summary>
        /// 現在の更新処理におけるキーボード状態。
        /// </summary>
        KeyboardState currentKeyboardState;

        /// <summary>
        /// 前回の更新処理におけるマウス状態。
        /// </summary>
        MouseState lastMouseState;

        /// <summary>
        /// 現在の更新処理におけるマウス状態。
        /// </summary>
        MouseState currentMouseState;

        WorldManager worldManager;

        public MainGame()
        {
            graphicsManager = new GraphicsManager(this);

            content = new XnbManager(Services, "Content");

            graphicsManager.PreferredBackBufferWidth = WindowWidth;
            graphicsManager.PreferredBackBufferHeight = WindowHeight;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(DeviceContext);
            spriteFont = content.Load<SpriteFont>("hudFont");

            worldManager = new WorldManager(DeviceContext);
            worldManager.Load("title:Assets/Regions/Default.json");

        }

        protected override void Update(GameTime gameTime)
        {
            // これをしないとデバッグできない。
            if (!IsActive) return;

            // ユーザ入力処理。
            HandleInput(gameTime);

            // カメラ更新。
            UpdateCamera(gameTime);

            // ワールド更新。
            worldManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // ワールド描画。
            worldManager.Draw(gameTime);

            // HUD 描画。
            DrawHud();

            base.Draw(gameTime);
        }

        void UpdateCamera(GameTime gameTime)
        {
        }

        void HandleInput(GameTime gameTime)
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (!worldManager.Closing && !worldManager.Closed && currentKeyboardState.IsKeyDown(Keys.Escape))
                worldManager.Close();

            if (worldManager.Closed)
                Exit();
        }

        void DrawHud()
        {
        }
    }

    #region Program

    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new MainGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
