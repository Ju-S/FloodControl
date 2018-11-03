using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FloodControl
{
    public class FloodControl : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D playingPieces, backgroundScreen, titleScreen;
        GameBoard gameBoard;
        Vector2 gameBoardDisplayOrigin = new Vector2(70, 89);
        int playerScore = 0,
            currentLevel = 0,
            linesCompetedThisLevel = 0;
        enum GameStates { TitleScreen, Playing, GameEnd };
        GameStates gameState = GameStates.TitleScreen;
        Rectangle EmptyPiece = new Rectangle(1, 247, 40, 40);
        const float
            MinTimeSinceLastInput = 0.25f,
            MaxFloodCounter = 100.0f,
            floodAccelerationPerLevel = 2.0f;
        float
            timeSinceLastInput = 0.0f,
            gameOverTimer,
            floodCount = 0.0f,
            timeSinceLastFloodIncrease = 0.0f,
            timeBetweenFloodIncreases = 1.0f,
            floodIncreaseAmount = 0.0f;
        SpriteFont pericles36Font;
        Vector2
            scorePosition = new Vector2(605, 215),
            gameOverLocation = new Vector2(200, 260),
            waterOverlayStart = new Vector2(85, 245),
            waterPosition = new Vector2(478, 338),
            levelTextPosition = new Vector2(512, 215);
        Queue<ScoreZoom> ScoreZooms = new Queue<ScoreZoom>();
        const int
            MaxWaterHeight = 244,
            WaterWidth = 297;
        bool 
            pieceDrawn = false,
            complete = false;
        string
            gameEnd;
        Color endColor;

        public FloodControl()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            gameBoard = new GameBoard();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            playingPieces = Content.Load<Texture2D>("Textures/Tile_Sheet");
            backgroundScreen = Content.Load<Texture2D>("Textures/Background");
            titleScreen = Content.Load<Texture2D>("Textures/TitleScreen");
            pericles36Font = Content.Load<SpriteFont>("Fonts/Pericles36");
        }

        protected override void UnloadContent()
        {

        }

        private int DetermineScore(int SquareCount)
        {
            return (int)((Math.Pow((SquareCount / 5), 2) + SquareCount) * 10);
        }

        private void CheckScoringChain(List<Vector2> WaterChain)
        {
            if (WaterChain.Count > 0)
            {
                Vector2 LastPipe = WaterChain[WaterChain.Count - 1];
                if (LastPipe.X == GameBoard.GameBoardWidth - 1)
                {
                    if (gameBoard.HasConnector(
                        (int)LastPipe.X, (int)LastPipe.Y, "Right"))
                    {
                        playerScore += DetermineScore(WaterChain.Count);
                        linesCompetedThisLevel++;
                        ScoreZooms.Enqueue(new ScoreZoom("+" +
                        DetermineScore(WaterChain.Count).ToString(),
                        new Color(1.0f, 0.0f, 0.0f, 0.4f)));
                        floodCount = MathHelper.Clamp(floodCount -
                            (DetermineScore(WaterChain.Count) / 30), 0.0f, 100.0f);
                        foreach (Vector2 ScoringSquare in WaterChain)
                        {
                            gameBoard.AddFadingPiece((int)ScoringSquare.X, (int)ScoringSquare.Y,
                                gameBoard.GetSquare((int)ScoringSquare.X, (int)ScoringSquare.Y));
                            gameBoard.SetSquare((int)ScoringSquare.X,
                                (int)ScoringSquare.Y, "Empty");
                        }
                        if(linesCompetedThisLevel >= 5)
                            StartNewLevel();
                    }
                }
            }
        }

        private void HandleMouseInput(MouseState mouseState)
        {
            int x = ((mouseState.X - (int)gameBoardDisplayOrigin.X) /
                GamePiece.PieceWidth),
                y = ((mouseState.Y - (int)gameBoardDisplayOrigin.Y) /
                GamePiece.PieceHeight);

            if ((x >= 0) && (x < GameBoard.GameBoardWidth) &&
                (y >= 0) && (y < GameBoard.GameBoardHeight))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    gameBoard.AddRotatingPiece(x, y,
                        gameBoard.GetSquare(x, y), false);
                    gameBoard.RotatePiece(x, y, false);
                    timeSinceLastInput = 0.0f;
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    gameBoard.AddRotatingPiece(x, y,
                        gameBoard.GetSquare(x, y), true);
                    gameBoard.RotatePiece(x, y, true);
                    timeSinceLastInput = 0.0f;
                }
            }
        }

        private void UpdateScoreZooms()
        {
            int dequeueCounter = 0;
            foreach (ScoreZoom zoom in ScoreZooms)
            {
                zoom.Update();
                if (zoom.IsCompleted)
                    dequeueCounter++;
            }
            for (int d = 0; d < dequeueCounter; d++)
                ScoreZooms.Dequeue();
        }

        private void StartNewLevel()
        {
            currentLevel++;
            floodCount = 0;
            linesCompetedThisLevel = 0;
            floodIncreaseAmount += floodAccelerationPerLevel;
            gameBoard.ClearBoard();
            gameBoard.GenerateNewPieces(false);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            switch (gameState)
            {
                case GameStates.TitleScreen:
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        gameBoard.ClearBoard();
                        gameBoard.GenerateNewPieces(false);
                        playerScore = 0;
                        currentLevel = 0;
                        StartNewLevel();
                        floodIncreaseAmount = 2.0f;
                        gameState = GameStates.Playing;
                    }
                    break;
                case GameStates.Playing:
                    timeSinceLastFloodIncrease +=
                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceLastFloodIncrease >= timeBetweenFloodIncreases)
                    {
                        floodCount += floodIncreaseAmount;
                        timeSinceLastFloodIncrease = 0.0f;
                        if (floodCount >= MaxFloodCounter || currentLevel == 5)
                        {
                            if(floodCount >= MaxFloodCounter)
                                complete = false;
                            else
                                complete = true;
                            gameOverTimer = 3.0f;
                            gameState = GameStates.GameEnd;
                        }
                    }
                    timeSinceLastInput += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (gameBoard.ArePiecesAnimating())
                        gameBoard.UpdateAnimatedPieces();
                    else
                    {
                        gameBoard.ResetWater();
                        gameBoard.ResetWater();
                        for (int y = 0; y < GameBoard.GameBoardHeight; y++)
                            CheckScoringChain(gameBoard.GetWaterChain(y));
                        gameBoard.GenerateNewPieces(true);
                        if (timeSinceLastInput >= MinTimeSinceLastInput)
                            HandleMouseInput(Mouse.GetState());
                    }
                    UpdateScoreZooms();
                    break;
                case GameStates.GameEnd:
                    gameOverTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (gameOverTimer <= 0)
                        gameState = GameStates.TitleScreen;
                    break;
            }
            base.Update(gameTime);
        }

        private void DrawEmptyPiece(int pixelX, int pixelY)
        {
            spriteBatch.Draw(
                playingPieces,
                new Rectangle(pixelX, pixelY,
                GamePiece.PieceWidth, GamePiece.PieceHeight),
                EmptyPiece,
                Color.White);
        }

        private void DrawStandardPiece(int x, int y, int pixelX, int pixelY)
        {
            spriteBatch.Draw(
                playingPieces,
                new Rectangle(pixelX, pixelY,
                GamePiece.PieceWidth, GamePiece.PieceHeight),
                gameBoard.GetSourceRect(x, y),
                Color.White);
        }

        private void DrawFallingPiece(int pixelX, int pixelY, string positionName)
        {
            spriteBatch.Draw(
                playingPieces,
                new Rectangle(pixelX, pixelY -
                gameBoard.fallingPieces[positionName].VerticalOffset,
                GamePiece.PieceWidth, GamePiece.PieceHeight),
                gameBoard.fallingPieces[positionName].GetSouceRect(),
                Color.White);
        }

        private void DrawFadingPiece(int pixelX, int pixelY, string positionName)
        {
            spriteBatch.Draw(
                playingPieces,
                new Rectangle(pixelX, pixelY,
                GamePiece.PieceWidth, GamePiece.PieceHeight),
                Color.White * gameBoard.fadingPieces[positionName].alphaLevel);
        }

        private void DrawRotationPiece(int pixelX, int pixelY, string positionName)
        {
            spriteBatch.Draw(
                 playingPieces,
                new Rectangle(pixelX + (GamePiece.PieceWidth / 2),
                pixelY + (GamePiece.PieceHeight / 2),
                GamePiece.PieceWidth, GamePiece.PieceHeight),
                gameBoard.rotatingPieces[positionName].GetSouceRect(),
                Color.White,
                gameBoard.rotatingPieces[positionName].RotationAmount,
                new Vector2(GamePiece.PieceWidth / 2,
                GamePiece.PieceHeight / 2),
                SpriteEffects.None, 0.0f);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (gameState == GameStates.TitleScreen)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(
                    titleScreen,
                    new Rectangle(0, 0,
                    this.Window.ClientBounds.Width, this.Window.ClientBounds.Height),
                    Color.White);
                spriteBatch.End();
            }
            if (gameState == GameStates.Playing)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(
                    backgroundScreen,
                    new Rectangle(0, 0,
                    this.Window.ClientBounds.Width, this.Window.ClientBounds.Height),
                    Color.White);
                for (int x = 0; x < GameBoard.GameBoardWidth; x++)
                {
                    for (int y = 0; y < GameBoard.GameBoardHeight; y++)
                    {
                        int pixelX = (int)gameBoardDisplayOrigin.X +
                            (x * GamePiece.PieceWidth),
                            pixelY = (int)gameBoardDisplayOrigin.Y +
                            (y * GamePiece.PieceHeight);

                        DrawEmptyPiece(pixelX, pixelY);

                        pieceDrawn = false;

                        string positionName = x.ToString() + "_" + y.ToString();

                        if (gameBoard.rotatingPieces.ContainsKey(positionName))
                        {
                            DrawRotationPiece(pixelX, pixelY, positionName);
                            pieceDrawn = true;
                        }
                        if (gameBoard.fadingPieces.ContainsKey(positionName))
                        {
                            DrawFadingPiece(pixelX, pixelY, positionName);
                            pieceDrawn = true;
                        }
                        if (gameBoard.fallingPieces.ContainsKey(positionName))
                        {
                            DrawFallingPiece(pixelX, pixelY, positionName);
                            pieceDrawn = true;
                        }
                        if (!pieceDrawn)
                        {
                            DrawStandardPiece(x, y, pixelX, pixelY);
                        }
                    }
                }
                foreach (ScoreZoom zoom in ScoreZooms)
                {
                    spriteBatch.DrawString(pericles36Font, zoom.Text,
                        new Vector2(this.Window.ClientBounds.Width / 2,
                        this.Window.ClientBounds.Height / 2),
                        zoom.DrawColor, 0.0f,
                        new Vector2(pericles36Font.MeasureString(zoom.Text).X / 2,
                        pericles36Font.MeasureString(zoom.Text).Y / 2),
                        zoom.Scale, SpriteEffects.None, 0.0f);
                }
                spriteBatch.DrawString(
                    pericles36Font,
                    playerScore.ToString(),
                    scorePosition,
                    Color.Black);
                spriteBatch.DrawString(
                    pericles36Font,
                    currentLevel.ToString(),
                    levelTextPosition,
                    Color.Black);
                if (!pieceDrawn)
                {
                    int waterHeight = (int)(MaxWaterHeight * (floodCount / 100) + 20);
                    spriteBatch.Draw(backgroundScreen,
                        new Rectangle(
                            (int)waterPosition.X,
                            (int)waterPosition.Y + (MaxWaterHeight - waterHeight),
                            WaterWidth,
                            waterHeight),
                        new Rectangle(
                            (int)waterOverlayStart.X,
                            (int)waterOverlayStart.Y + (MaxWaterHeight - waterHeight),
                            WaterWidth,
                            waterHeight),
                        new Color(255, 255, 255, 180));
                }
                spriteBatch.End();
            }
            if (gameState == GameStates.GameEnd)
            {
                spriteBatch.Begin();
                if (complete)
                {
                    gameEnd = "     C L E A R !";
                    endColor = Color.Yellow;
                }
                else
                {
                    gameEnd = "G A M E O V E R !";
                    endColor = Color.Red;
                }
                spriteBatch.DrawString(pericles36Font,
                    gameEnd,
                    gameOverLocation,
                    endColor);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}