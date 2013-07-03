#region Using

using System;
using System.Text;
using Libra;
using Libra.Games;
using Libra.Games.Debugging;
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
        /// 中間マップ表示。
        /// </summary>
        TextureDisplay textureDisplay;

        /// <summary>
        /// フレーム レート計測器。
        /// </summary>
        FrameRateMeasure frameRateMeasure;

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

        float cameraMoveVelocity = 10.0f;

        float cameraDashFactor = 2;

        StringBuilder hudText = new StringBuilder();

        bool hudVisible = true;

        public MainGame()
        {
            graphicsManager = new GraphicsManager(this);

            content = new XnbManager(Services, "Content");

            graphicsManager.PreferredBackBufferWidth = WindowWidth;
            graphicsManager.PreferredBackBufferHeight = WindowHeight;

            textureDisplay = new TextureDisplay(this);
            const float scale = 0.1f;
            textureDisplay.TextureWidth = (int) (WindowWidth * scale);
            textureDisplay.TextureHeight = (int) (WindowHeight * scale);
            textureDisplay.Visible = false;
            //textureDisplay.Visible = true;
            Components.Add(textureDisplay);

            frameRateMeasure = new FrameRateMeasure(this);
            Components.Add(frameRateMeasure);
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

            var sceneManager = worldManager.SceneManager;

            textureDisplay.Textures.Add(sceneManager.DepthMap);
            textureDisplay.Textures.Add(sceneManager.NormalMap);
            
            for (int i = 0; i < sceneManager.LightSceneMaps.Count; i++)
            {
                if (sceneManager.LightSceneMaps[i] != null)
                    textureDisplay.Textures.Add(sceneManager.LightSceneMaps[i]);
            }

            if (sceneManager.FinalLightSceneMap != null)
                textureDisplay.Textures.Add(sceneManager.FinalLightSceneMap);

            if (sceneManager.BaseSceneMap != null)
                textureDisplay.Textures.Add(sceneManager.BaseSceneMap);

            base.Draw(gameTime);
        }

        void HandleInput(GameTime gameTime)
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (currentKeyboardState.IsKeyUp(Keys.F1) && lastKeyboardState.IsKeyDown(Keys.F1))
                hudVisible = !hudVisible;

            if (currentKeyboardState.IsKeyUp(Keys.F2) && lastKeyboardState.IsKeyDown(Keys.F2))
                textureDisplay.Visible = !textureDisplay.Visible;

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
                var yawAmount = -(currentMouseState.X - initialMousePosition.X) * 0.25f;
                camera.Yaw(yawAmount * cameraRotationVelocity * deltaTime);

                var pitchAmount = -(currentMouseState.Y - initialMousePosition.Y) * 0.25f;
                camera.Pitch(pitchAmount * cameraRotationVelocity * deltaTime);

                Mouse.SetPosition(initialMousePosition.X, initialMousePosition.Y);
            }

            // 移動距離 = 速度 * 時間。
            var distance = cameraMoveVelocity * deltaTime;

            // [Shift] 押下中は移動速度が二倍。
            if (currentKeyboardState.IsKeyDown(Keys.ShiftKey))
            {
                distance *= cameraDashFactor;
            }

            // [w/s/a/d/q/z] でカメラ移動。
            // ただし、[Control] 押下中は他コマンドの実行が発生するため移動無効。
            if (currentKeyboardState.IsKeyUp(Keys.LeftControl))
            {
                // Z 軸移動。
                if (currentKeyboardState.IsKeyDown(Keys.W))
                    camera.Move(camera.Direction * distance);
                if (currentKeyboardState.IsKeyDown(Keys.S))
                    camera.Move(camera.Direction * (-distance));

                // X 軸移動。
                if (currentKeyboardState.IsKeyDown(Keys.D))
                    camera.Move(camera.Right * distance);
                if (currentKeyboardState.IsKeyDown(Keys.A))
                    camera.Move(camera.Right * (-distance));

                // Y 軸移動。
                if (currentKeyboardState.IsKeyDown(Keys.Q))
                    camera.Move(camera.Up * distance);
                if (currentKeyboardState.IsKeyDown(Keys.Z))
                    camera.Move(camera.Up * (-distance));
            }

            camera.Update();
        }

        void DrawHud()
        {
            if (!hudVisible) return;

            spriteBatch.Begin();

            CreateInformationText();
            spriteBatch.DrawString(spriteFont, hudText, new Vector2(9, 9), Color.Black);
            spriteBatch.DrawString(spriteFont, hudText, new Vector2(8, 8), Color.Yellow);

            CreateHelp2Text();
            spriteBatch.DrawString(spriteFont, hudText, new Vector2(1050, 550), Color.Black);
            spriteBatch.DrawString(spriteFont, hudText, new Vector2(1049, 549), Color.Yellow);

            spriteBatch.End();
        }

        void CreateInformationText()
        {
            hudText.Length = 0;

            hudText.Append("FPS: ").AppendNumber(frameRateMeasure.FrameRate).AppendLine();

            hudText.Append("Screen: ");
            hudText.AppendNumber(graphicsManager.PreferredBackBufferWidth).Append('x');
            hudText.AppendNumber(graphicsManager.PreferredBackBufferHeight).AppendLine();

            var chunkManager = worldManager.ChunkManager;
            hudText.Append("Chunk: ");
            hudText.Append("A(").AppendNumber(chunkManager.ClusterCount).Append(":");
            hudText.AppendNumber(chunkManager.Count).Append(") ");
            hudText.Append("W(").AppendNumber(chunkManager.ActivationCount).Append(") ");
            hudText.Append("P(").AppendNumber(chunkManager.PassivationCount).Append(")").AppendLine();

            hudText.Append("Mesh: ").AppendNumber(chunkManager.MeshCount).Append(" ");
            hudText.Append("Inter: ").AppendNumber(chunkManager.ActiveVertexBuilderCount).Append("/");
            hudText.AppendNumber(chunkManager.TotalVertexBuilderCount).AppendLine();

            hudText.Append("ChunkVertex: ");
            hudText.Append("Max(").AppendNumber(chunkManager.MaxVertexCount).Append(") ");
            hudText.Append("Total(").AppendNumber(chunkManager.TotalVertexCount).Append(")").AppendLine();

            hudText.Append("ChunkIndex: ");
            hudText.Append("Max(").AppendNumber(chunkManager.MaxIndexCount).Append(") ");
            hudText.Append("Total(").AppendNumber(chunkManager.TotalIndexCount).Append(")").AppendLine();

            var sceneManager = worldManager.SceneManager;
            hudText.Append("SceneObejcts: ").AppendNumber(sceneManager.RenderedSceneObjectCount).Append("/");
            hudText.AppendNumber(sceneManager.SceneObjectCount).AppendLine();

            //if (worldManager.ShadowMap != null)
            //{
            //    var shadowMapMonitor = worldManager.ShadowMap.Monitor;
            //    hudText.Append("ShadowCaster: ");
            //    for (int i = 0; i < shadowMapMonitor.SplitCount; i++)
            //    {
            //        if (0 < i) hudText.Append(":");
            //        hudText.AppendNumber(shadowMapMonitor[i].ShadowCasterCount);
            //    }
            //    hudText.Append("/").AppendNumber(shadowMapMonitor.TotalShadowCasterCount).AppendLine();
            //}

            hudText.Append("MoveVelocity: ");
            hudText.AppendNumber(cameraMoveVelocity).AppendLine();

            var camera = worldManager.SceneManager.ActiveCamera;
            hudText.Append("Eye: ");
            hudText.Append("P(");
            hudText.AppendNumber(camera.Position.X).Append(", ");
            hudText.AppendNumber(camera.Position.Y).Append(", ");
            hudText.AppendNumber(camera.Position.Z).Append(") ");
            hudText.Append("D(");
            hudText.AppendNumber(camera.Direction.X).Append(", ");
            hudText.AppendNumber(camera.Direction.Y).Append(", ");
            hudText.AppendNumber(camera.Direction.Z).Append(")").AppendLine();

            var mouseState = Mouse.GetState();
            hudText.Append("Mouse (").AppendNumber(mouseState.X).Append(", ").AppendNumber(mouseState.Y).Append(")").AppendLine();

            //hudText.Append("Brush: (");
            //if (brushManager.ActiveBrush is StickyBrush)
            //{
            //    var stickyBrush = brushManager.ActiveBrush as StickyBrush;
            //    hudText.Append(stickyBrush.PaintSide).Append(": ");
            //}
            //hudText.AppendNumber(brushManager.ActiveBrush.Position.X).Append(", ");
            //hudText.AppendNumber(brushManager.ActiveBrush.Position.Y).Append(", ");
            //hudText.AppendNumber(brushManager.ActiveBrush.Position.Z).Append(")").AppendLine();

            hudText.Append("Near/Far: ").AppendNumber(camera.NearClipDistance).Append("/").AppendNumber(camera.FarClipDistance);
        }

        void CreateHelp2Text()
        {
            hudText.Length = 0;

            hudText.Append("[F1] HUD On/Off").AppendLine();
            hudText.Append("[F2] Inter-maps On/Off").AppendLine();
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
