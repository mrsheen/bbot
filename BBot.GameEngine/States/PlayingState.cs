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

        public PlayingState()
        {

            AssetName = "wholegame.playing";
            MinimumConfidence = 200000;

            /*listGemColorStats = GemDefinitions.GetGemDefinitions();
            delay = new int[GridSize + 6, GridSize + 6];
            bmpHeatmap = new Bitmap(BoardSize.Width, BoardSize.Height);
            bHuzzah = false;

            */

            pauseClickOffset.X = 90;
            pauseClickOffset.Y = 390;
            
        }

        //!TODO!Fill out pause/resume for ticker
        public override void Cleanup() {
            /*
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
            if (!bHuzzah)
            {
                Pause();
            }*/

        }
        //!TODO!Fill out pause/resume for ticker
        public override void Pause() {
            //SendInputClass.Click(
                           //game.GameExtents.Value.X + pauseClickOffset.X,
                           //game.GameExtents.Value.Y + pauseClickOffset.Y);
            System.Threading.Thread.Sleep(200);
        }

        //public override void Resume() { }

        public override bool HandleEvents()
        {
            /*
            if (base.HandleEvents() || bHuzzah)
                return true;

            if (timer == null)
            {
                Thread.Sleep(500);
                timer = new Timer(new TimerCallback(GameOver), game, 66 * 1000, Timeout.Infinite);
            }
            */
            return false;
            
        }

        public override void Update()
        {
            /*
            if (bHuzzah)
                return;

            

            GetBoardFromGame(game);

            if (!bContinue)
                return;

            if (ScanGrid())
            {
                System.Diagnostics.Debugger.Break();
                DoMoves(game);

            }*/

        }

        public override void Draw()
        {
            TickDownDelay(game);


            
            

        }

        #region Private fields

        #endregion

        #region Private Methods

        private void GameOver(Object state)
        {
            //SendInputClass.Move(0, 0);
            GameEngine game = (GameEngine)state;
            //bHuzzah = true;
            Thread.Sleep(10 * 1000);
            game.StateManager.ChangeState(new StarState());
        }


        private void GetBoardFromGame(GameEngine game)
        {
            /*
            bContinue = false;
            if (Monitor.TryEnter(game.GameScreenLOCK,20))
            {
                try {
                     bmpBoard = game.GameScreen.Clone(new Rectangle(BoardLocationOnGame, BoardSize), game.GameScreen.PixelFormat);
                     if (game.GameScreen == null || bmpBoard == null)
                         return;
                     bContinue = true;
                }
                finally 
                {
                    Monitor.Exit(game.GameScreenLOCK);
                }
            }*/
        }
 

        // Check if two colours match
        private bool MatchColours(Gem a, Gem b)
        {
            return (a.Equals(b));

        }

        private DateTime heatmapTimestamp = DateTime.Now;
        private DateTime delayTimestamp = DateTime.Now;
        private void TickDownDelay(GameEngine game)
        {
            // Loop variables
            int iX;
            int iY;
            byte bIntense;
            /*
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
            }*/

            delayTimestamp = DateTime.Now;
            /*
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
            }*/
        }


        /*


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


        #endregion

    }
}

