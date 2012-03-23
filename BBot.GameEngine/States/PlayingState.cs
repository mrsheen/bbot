using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using BBot.GameDefinitions;

namespace BBot.GameEngine.States
{
    public class PlayingState : BaseGameState
    {
        Point pauseClickOffset = new Point(0,0);
        BoardDefinition currentBoard = new BoardDefinition();

        Timer timer;

        public PlayingState()
        {
            Name = "Playing";

            gameScreen.AssetName =  "wholegame.playing";
            gameScreen.MinimumConfidence =  200000;

            /*
            
            bmpHeatmap = new Bitmap(BoardSize.Width, BoardSize.Height);

            */

            pauseClickOffset.X = 90;
            pauseClickOffset.Y = 390;
            


        }

        //!TODO!Fill out pause/resume for ticker
        public override void Cleanup()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }

        }

        public override void Pause() {
            gameEngine.DebugAction("Game paused");
            gameEngine.MakeMove(
                        gameEngine.GameExtents.Value.X + pauseClickOffset.X,
                        gameEngine.GameExtents.Value.Y + pauseClickOffset.Y);
            System.Threading.Thread.Sleep(200);
        }


        public override void Update(CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                Pause();
                throw new OperationCanceledException();
            }

            if (timer == null)
            {
                Thread.Sleep(500);
                timer = new Timer(new TimerCallback(GameOver), null, 60 * 1000, Timeout.Infinite);
            }

            if (Monitor.TryEnter(gameEngine.GameScreenLOCK,20))
            {
                try 
                {
                    using (Bitmap bmpBoard = gameEngine.GameScreen.CloneExact())
                    {
                        currentBoard.GetBoardFromImage(bmpBoard);
                    }
                }
                finally 
                {
                    Monitor.Exit(gameEngine.GameScreenLOCK);
                }
            }


            foreach (GameScreenMove move in currentBoard.GetValidMovesFromBoard(cancelToken))
            {// for each move
                // do move
                gameEngine.MakeMove(move.FirstPiece.X + gameEngine.GameExtents.Value.X, move.FirstPiece.Y + gameEngine.GameExtents.Value.Y);
                gameEngine.MakeMove(move.SecondPiece.X + gameEngine.GameExtents.Value.X, move.SecondPiece.Y + gameEngine.GameExtents.Value.Y);
            }
        }

        public override void Draw(CancellationToken cancelToken)
        {
            //TickDownDelay(gameEngine);

        }

        private void GameOver(Object obj)
        {
            gameEngine.DebugAction("Game Over! Waiting for Last Hurrah . . .");
            
            Thread.Sleep(10 * 1000);

            gameEngine.DebugAction("Hurrah!");
            gameEngine.StateManager.ChangeState(new StarState());
        }

        /*

        private DateTime heatmapTimestamp = DateTime.Now;
        private DateTime delayTimestamp = DateTime.Now;
        private void TickDownDelay(BBotGameEngine game)
        {
            // Loop variables
            int iX;
            int iY;
            byte bIntense;
            
            float fRatio = 1F / MAX_DELAY;
            Heatmap.ClearHeatpoints();
            // Tick for each delay
            for (int i = 0; i < GridSize + 6; i++)
            {
                for (int j = 0; j < GridSize + 6; j++)
                {
                    delay[i, j] = delay[i, j] - ((DateTime.Now - delayTimestamp).Milliseconds);// tMove.Interval;
                    if (delay[i, j] < 0)
                        delay[i, j] = 0;

                    iX = CellSize * (i - 3) + CellSize/2;
                    iY = CellSize * (j - 3) + CellSize/2;
                    bIntense = (byte)(delay[i, j] * fRatio * Byte.MaxValue);
                    // Add heat point to heat points list
                    Heatmap.AddHeatpoint(new Heatmap.HeatPoint(iX, iY, bIntense));
                }
            }

            delayTimestamp = DateTime.Now;
            
            Bitmap bmpNewPreview = BoardDefinition.GenerateBoardImage(Board);
            //bmpNewPreview = GenerateHeatmap(game, bmpNewPreview);


            if (Monitor.TryEnter(game.PreviewScreenLOCK, 10))
            {
                try
                {
                    game.PreviewScreen = bmpNewPreview.Clone(new Rectangle(0, 0, bmpNewPreview.Width, bmpNewPreview.Height), bmpNewPreview.PixelFormat);
                }
                finally
                {
                    Monitor.Exit(game.PreviewScreenLOCK);
                }
            }
        }
   


        private bool CheckDelay(int x, int y)
        {
            return (delay[x, y] == 0);

        }

        private void SetDelay(int x, int y, int delayCount)
        {
            if (x < 0 || y < 0 || x >= GridSize + 6 || y >= GridSize + 6)
                return;


            int dampeningRadius = 4;
            double a = -2.0 * dampeningRadius * dampeningRadius / Math.Log(0.00001);

            // Tick for each delay
            for (int i = 0; i < GridSize + 6; i++)
            {
                for (int j = 0; j < GridSize + 6; j++)
                {

                    double radius = Math.Sqrt(Math.Pow(x - i, 2) + Math.Pow(y - j, 2));
                    double height = Math.Exp(-radius * radius / a);
                    double dampFactor = height * delayCount;
                    if (dampFactor < 10)
                        continue;

                    delay[i, j] += (int)dampFactor;
                    if (delay[i, j] > MAX_DELAY)
                        delay[i, j] = MAX_DELAY;
                }
            }



        }*/

    

    }
}

