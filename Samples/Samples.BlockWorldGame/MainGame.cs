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

        /// <summary>
        /// マウスの初期位置。
        /// </summary>
        Point initialMousePosition = new Point(WindowWidth / 2, WindowHeight / 2);

        WorldManager worldManager;

        float cameraRotationVelocity = 0.3f;

        float cameraMoveVelocity = 30.0f;

        float cameraDashFactor = 2;

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
            worldManager.Load("title:Assets/Regions/Default.xml");

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

        void UpdateCamera(GameTime gameTime)
        {
            var camera = worldManager.SceneManager.ActiveCamera;
            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // マウス移動でカメラの姿勢を制御。
            if (initialMousePosition.X != currentMouseState.X ||
                initialMousePosition.Y != currentMouseState.Y)
            {
                var yawAmount = -(currentMouseState.X - initialMousePosition.X);
                camera.Yaw(yawAmount * cameraRotationVelocity * deltaTime);

                var pitchAmount = -(currentMouseState.Y - initialMousePosition.Y);
                camera.Pitch(pitchAmount * cameraRotationVelocity * deltaTime);

                Mouse.SetPosition(initialMousePosition.X, initialMousePosition.Y);
            }

            // 移動距離 = 速度 * 時間。
            var distance = cameraMoveVelocity * deltaTime;

            // [Shift] 押下中は移動速度が二倍。
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) ||
                currentKeyboardState.IsKeyDown(Keys.RightShift))
            {
                distance *= cameraDashFactor;
            }

            // [w/s/a/d/q/z] でカメラ移動。
            // ただし、[Control] 押下中は他コマンドの実行が発生するため移動無効。
            if (currentKeyboardState.IsKeyUp(Keys.LeftControl))
            {
                // Z 軸移動。
                if (currentKeyboardState.IsKeyDown(Keys.W))
                    camera.MoveRelative(camera.Position * distance);
                if (currentKeyboardState.IsKeyDown(Keys.S))
                    camera.MoveRelative(camera.Position * (-distance));

                // X 軸移動。
                if (currentKeyboardState.IsKeyDown(Keys.D))
                    camera.MoveRelative(camera.Right * distance);
                if (currentKeyboardState.IsKeyDown(Keys.A))
                    camera.MoveRelative(camera.Right * (-distance));

                // Y 軸移動。
                if (currentKeyboardState.IsKeyDown(Keys.Q))
                    camera.MoveRelative(camera.Up * distance);
                if (currentKeyboardState.IsKeyDown(Keys.Z))
                    camera.MoveRelative(camera.Up * (-distance));
            }

            camera.Update();
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
