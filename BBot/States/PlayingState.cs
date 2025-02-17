﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AForge.Imaging;
using System.Threading;
using AForge.Imaging.Filters;
using BBot.GamePieces;

namespace BBot.States
{
    public class PlayingState : BaseGameState
    {
        Point pauseClickOffset = new Point(0,0);

        public PlayingState()
        {

            AssetName = "wholegame.playing";
            MinimumConfidence = 200000;

            listGemColorStats = GemDefinitions.GetGemDefinitions();
            delay = new int[GridSize + 6, GridSize + 6];
            bmpHeatmap = new Bitmap(BoardSize.Width, BoardSize.Height);
            bHuzzah = false;



            pauseClickOffset.X = 90;
            pauseClickOffset.Y = 390;
            
        }

        //!TODO!Fill out pause/resume for ticker
        public override void Cleanup() {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
            if (!bHuzzah)
            {
                Pause();
            }

        }
        //!TODO!Fill out pause/resume for ticker
        public override void Pause() {
            SendInputClass.Click(
                           game.GameExtents.Value.X + pauseClickOffset.X,
                           game.GameExtents.Value.Y + pauseClickOffset.Y);
            System.Threading.Thread.Sleep(200);
        }

        //public override void Resume() { }

        public override bool HandleEvents()
        {
            if (base.HandleEvents() || bHuzzah)
                return true;

            if (timer == null)
            {
                Thread.Sleep(500);
                timer = new Timer(new TimerCallback(GameOver), game, 66 * 1000, Timeout.Infinite);
            }
            
            return false;
            
        }

        public override void Update()
        {
            if (bHuzzah)
                return;

            

            GetBoardFromGame(game);

            if (!bContinue)
                return;

            if (ScanGrid())
                DoMoves(game);

        }

        public override void Draw()
        {
            TickDownDelay(game);


            
            

        }

        #region Private fields

        private bool bContinue = false;

        private readonly Size BoardSize = new Size(320, 320); // The size of the Bejeweled gem grid
        private readonly Point BoardLocationOnGame = new Point(175, 55);
        private const int CellSize = 40; // Size of each cell in the grid
        private const int GridSize = 8;


        private const int MAX_DELAY = 500;

        private Bitmap bmpHeatmap;
        private Bitmap bmpBoard;

        private Gem[,] Board = new Gem[GridSize + 6, GridSize + 6]; // Matrix to hold the colour present in each grid cell

        private System.Threading.Timer timer;

        private List<Gem> listGemColorStats;
        private static int[,] delay;
        private bool bHuzzah = false;

        #endregion

        #region Private Methods

        private void GameOver(Object state)
        {
            SendInputClass.Move(0, 0);
            GameEngine game = (GameEngine)state;
            bHuzzah = true;
            Thread.Sleep(10 * 1000);
            game.StateManager.ChangeState(new StarState());
        }


        private void GetBoardFromGame(GameEngine game)
        {
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
            }
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

            Bitmap bmpNewPreview = GenerateBoardImage(game);
            bmpNewPreview = GenerateHeatmap(game, bmpNewPreview);


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


        private Bitmap GenerateBoardImage(GameEngine game)
        {
            Bitmap bmpBoardGems = new Bitmap(BoardSize.Width,BoardSize.Height);

            using (Graphics g = Graphics.FromImage(bmpBoardGems))
            {
                // Across
                for (int x = 0; x < GridSize; x++)
                {
                    // Down
                    for (int y = 0; y < GridSize; y++)
                    {
                        Gem boardGem = Board[x + 3, y + 3];
                        


                        g.FillRectangle(new SolidBrush(GemDefinitions.GetDisplayColorForGem(boardGem.Name)), x * CellSize, y * CellSize, CellSize, CellSize);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Star))
                            g.FillRectangle(new SolidBrush(Color.White), x * CellSize + (CellSize / 4), y * CellSize + (CellSize / 4), CellSize / 2, CellSize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Hypercube))
                            g.FillRectangle(new SolidBrush(Color.LimeGreen), x * CellSize, y * CellSize , CellSize, CellSize);


                        if (boardGem.Modifiers.HasFlag(GemModifier.Multiplier ))
                            g.FillRectangle(new SolidBrush(Color.HotPink), x * CellSize + (CellSize / 4), y * CellSize + (CellSize / 4), CellSize / 2, CellSize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Background))
                            g.FillRectangle(new SolidBrush(Color.Black), x * CellSize + (CellSize / 4), y * CellSize + (CellSize / 4), CellSize / 2, CellSize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Coin))
                            g.FillRectangle(new SolidBrush(Color.Yellow), x * CellSize + (CellSize / 4), y * CellSize + (CellSize / 4), CellSize / 2, CellSize / 2);
                    }
                }
            }

            return bmpBoardGems;
            
        }

        private Bitmap GenerateHeatmap(GameEngine game, Bitmap bmpNewPreview)
        {
            //if ((DateTime.Now - heatmapTimestamp).Seconds < 0.05)
            //    return bmpNewPreview; // Only update image every 50 milliseconds

            Bitmap tempHeatmapMask = new Bitmap(bmpNewPreview);

            // Call CreateIntensityMask, give it the memory bitmap, and store the result back in the memory bitmap
            bmpHeatmap = Heatmap.CreateIntensityMask(tempHeatmapMask);

            // Colorize the memory bitmap and assign it as the picture boxes image
            bmpHeatmap = Heatmap.Colorize(bmpHeatmap, 255);
            heatmapTimestamp = DateTime.Now;

            // Add together
            Add filterAdd = new Add(bmpHeatmap);
            filterAdd.ApplyInPlace(bmpNewPreview);

            return bmpNewPreview;
        }

        // 
        // Code adapted from William Henry's Java bot: http://mytopcoder.com/bejeweledBot
        // TODO: I did this the easy way by always moving the cell we are currently looking at (just to get the app running), but this isn't the most efficient method
        private void DoMoves(GameEngine game)
        {
            // Across
            for (int y = GridSize + 3-1; y >= 3; y--)
            {
                // Down
                for (int x = 3; x < GridSize + 3; x++)
                {
                    // x
                    // -
                    // x
                    // x
                    if (MatchColours(Board[x, y], Board[x, y + 2]) && MatchColours(Board[x, y + 2], Board[x, y + 3]))
                    {
                        MakeMove(game, x, y, x, y + 1);
                    }


                    // - x
                    // x
                    // x

                    if (MatchColours(Board[x, y], Board[x - 1, y + 1]) && MatchColours(Board[x - 1, y + 1], Board[x - 1, y + 2]))
                    {
                        MakeMove(game, x, y, (x - 1), y);
                    }
                    // x
                    // - x
                    // x

                    if (MatchColours(Board[x, y], Board[x + 1, y + 1]) && MatchColours(Board[x + 1, y + 1], Board[x, y + 2]))
                    {
                        MakeMove(game, (x + 1), (y + 1), x, (y + 1));
                    }

                    // x
                    // x
                    // - x

                    if (MatchColours(Board[x, y], Board[x, y + 1]) && MatchColours(Board[x, y + 1], Board[x + 1, y + 2]))
                    {
                        MakeMove(game, (x + 1), (y + 2), x, (y + 2));
                    }

                    // x
                    // x
                    // -
                    // x

                    if (MatchColours(Board[x, y], Board[x, y + 1]) && MatchColours(Board[x, y + 1], Board[x, y + 3]))
                    {
                        MakeMove(game, x, (y + 3), x, (y + 2));
                    }

                    // x -
                    //   x
                    //   x

                    if (MatchColours(Board[x, y], Board[x + 1, y + 1]) && MatchColours(Board[x + 1, y + 1], Board[x + 1, y + 2]))
                    {
                        MakeMove(game, x, y, (x + 1), y);
                    }

                    //   x
                    // x -
                    //   x

                    if (MatchColours(Board[x, y], Board[x - 1, y + 1]) && MatchColours(Board[x - 1, y + 1], Board[x, y + 2]))
                    {
                        MakeMove(game, (x - 1), (y + 1), (x), (y + 1));
                    }

                    //   x
                    //   x
                    // x -

                    if (MatchColours(Board[x, y], Board[x, y + 1]) && MatchColours(Board[x, y + 1], Board[x - 1, y + 2]))
                    {
                        MakeMove(game, (x - 1), (y + 2), x, (y + 2));
                    }

                    // xx-x

                    if (MatchColours(Board[x, y], Board[x + 1, y]) && MatchColours(Board[x + 1, y], Board[x + 3, y]))
                    {
                        MakeMove(game, (x + 3), y, (x + 2), y);
                    }

                    // x--
                    // -xx

                    if (MatchColours(Board[x, y], Board[x + 1, y + 1]) && MatchColours(Board[x + 1, y + 1], Board[x + 2, y + 1]))
                    {
                        MakeMove(game, x, y, x, (y + 1));
                    }

                    // -x-
                    // x-x

                    if (MatchColours(Board[x, y], Board[x + 1, y + 1]) && MatchColours(Board[x + 1, y + 1], Board[x - 1, y + 1]))
                    {
                        MakeMove(game, x, y, x, (y + 1));
                    }

                    // --x
                    // xx-

                    if (MatchColours(Board[x, y], Board[x - 1, y + 1]) && MatchColours(Board[x - 1, y + 1], Board[x - 2, y + 1]))
                    {
                        MakeMove(game, x, y, x, (y + 1));
                    }

                    // x-xx

                    if (MatchColours(Board[x, y], Board[x + 2, y]) && MatchColours(Board[x + 2, y], Board[x + 3, y]))
                    {
                        MakeMove(game, x, y, (x + 1), y);
                    }

                    // -xx
                    // x--

                    if (MatchColours(Board[x, y], Board[x + 1, y]) && MatchColours(Board[x + 1, y], Board[x - 1, y + 1]))
                    {
                        MakeMove(game, (x - 1), (y + 1), (x - 1), y);
                    }

                    // x-x
                    // -x-

                    if (MatchColours(Board[x, y], Board[x + 1, y + 1]) && MatchColours(Board[x + 1, y + 1], Board[x + 2, y]))
                    {
                        MakeMove(game, (x + 1), (y + 1), (x + 1), (y));
                    }

                    // xx-
                    // --x

                    if (MatchColours(Board[x, y], Board[x + 1, y]) && MatchColours(Board[x + 1, y], Board[x + 2, y + 1]))
                    {

                        MakeMove(game, (x + 2), (y + 1), (x + 2), y);
                    }

                }
            }


        }


        private void MakeMove(GameEngine game, int x1, int y1, int x2, int y2)
        {
            int primaryDelay = MAX_DELAY;


            if (!(CheckDelay(x1, y1) && CheckDelay(x2, y2)))
                return;


            //if (debugMode)
            //game.Debug(String.Format("Move made: {0},{1} - {2},{3}", new Object[] { x1 - 2, y1 - 2, x2 - 2, y2 - 2 }));
            // debugConsole.AppendText(String.Format("Move made: {0},{1} - {2},{3}", new Object[] { x1 - 2, y1 - 2, x2 - 2, y2 - 2 }) + System.Environment.NewLine);
            if (!game.GameExtents.HasValue)
                return; // Last minute catch to ensure we have accurate location for mouse clicks

            
            SetDelay(x1, y1, primaryDelay);

            Rectangle startPoint = game.GameExtents.Value;
            startPoint.X += BoardLocationOnGame.X;
            startPoint.Y += BoardLocationOnGame.Y;

            int mouseX1 = startPoint.X + (CellSize * (x1 - 3)) + (CellSize / 2);
            int mouseY1 = startPoint.Y + (CellSize * (y1 - 3)) + (CellSize / 2);

            int mouseX2 = startPoint.X + (CellSize * (x2 - 3)) + (CellSize / 2);
            int mouseY2 = startPoint.Y + (CellSize * (y2 - 3)) + (CellSize / 2);

            
            
            
            //System.Threading.Thread.Sleep(10);
            SendInputClass.Click(mouseX1, mouseY1);
            //System.Threading.Thread.Sleep(10);
            SendInputClass.Click(mouseX2, mouseY2);
            //System.Threading.Thread.Sleep(10);
            game.Debug(string.Format("clickX: {0}, clickY: {1}", mouseX1, mouseY1));
            //Thread.Sleep(1500);

            SendInputClass.Click(startPoint.X - 90, startPoint.Y + 150);
            

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



        }


        private DateTime backgroundMatchTimestamp = DateTime.Now;
        // Scan the gem Board and capture a coloured pixel from each cell
        public bool ScanGrid()
        {
            bool bMatchedAllPieces = true;
            int unmatchedCount = 0;

            Color[,] pieceColors = new Color[8, 8];
            List<double> scores = new List<double>();

            const int CellExtractionFactor = 8; // 8ths, so skip first 5 px

            int top = CellSize / CellExtractionFactor;//topOffset;
            int left = CellSize / CellExtractionFactor; //leftOffset;
            
            string workingPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), DateTime.Now.ToString("hhmm"));
            //System.IO.Directory.CreateDirectory(workingPath+"known");
            //System.IO.Directory.CreateDirectory(workingPath + "unknown");

            // Mask out board
            Bitmap bmpMask = GetBitmap("board.mask", bmpBoard.Size, bmpBoard.PixelFormat);
            if (bmpMask != null)
            {
                Subtract subFilter = new Subtract(bmpMask);

                if (subFilter != null)
                    subFilter.ApplyInPlace(bmpBoard);

                Bitmap bmpRenderedBoard;
                if (game.DebugMode)
                {
                    bmpRenderedBoard = bmpBoard.Clone(new Rectangle(0, 0, bmpBoard.Width, bmpBoard.Height), bmpBoard.PixelFormat);
                    

                    using (Graphics g = Graphics.FromImage(bmpRenderedBoard))
                    {
                        System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
                        cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                        cm.Matrix43 = 1.0F;

                        System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

                        ia.SetColorMatrix(cm);

                        g.DrawImage(bmpBoard, new Rectangle(0, 0, bmpBoard.Width, bmpBoard.Height), 0, 0, bmpBoard.Width, bmpBoard.Height, GraphicsUnit.Pixel, ia);
                    }
                }
            }



            // Across
            for (int x = 0; x < GridSize; x++)
            {
                // Down
                for (int y = 0; y < GridSize; y++)
                {
                    int boxLeft = left + (CellSize * x);
                    int boxTop = top + (CellSize * y);


                    
                    
                    // Get image of current gem
                    Rectangle rect = new Rectangle(boxLeft, boxTop, CellSize - 2 * (CellSize / CellExtractionFactor), CellSize - 2 * (CellSize / CellExtractionFactor));
                    Bitmap cropped = bmpBoard.Clone(rect, bmpBoard.PixelFormat);

                    ImageStatistics stats = new ImageStatistics(cropped);
                    
                    // Capture a colour from this gem
                    Gem PieceColor = new Gem { Name = GemColor.None, Modifiers = GemModifier.None, Color = Color.FromArgb(255, (int)stats.Red.Mean, (int)stats.Green.Mean, (int)stats.Blue.Mean) };
                    
                    // Calculate best score
                    double bestScore = 255*3; // what should this be set to?
                    double curScore = 0;
                    
                    foreach (Gem gem in listGemColorStats)
                    {

                        curScore = Math.Pow(gem.Color.R-stats.Red.Mean, 2)
                                 + Math.Pow(gem.Color.G-stats.Green.Mean, 2)
                                 + Math.Pow(gem.Color.B-stats.Blue.Mean, 2);


                        if (curScore < bestScore)
                        {
                            PieceColor = gem;
                            bestScore = curScore;

                        }
                    }
                    scores.Add(bestScore);
                    // Store it in the Board matrix at the correct position
                    Board[x + 3, y + 3] = PieceColor;
                    
                    Color newColor = Color.FromArgb(255,(int)stats.Red.Mean, (int)stats.Green.Mean, (int)stats.Blue.Mean);
                    pieceColors[x, y] = newColor;

                    if (game.DebugMode)
                    {
                        if (PieceColor.Name == GemColor.None || PieceColor.Modifiers == GemModifier.Background)
                        {
                            unmatchedCount++;
                            bMatchedAllPieces = false; // Didn't match one of the pieces, break on this
                            if ((DateTime.Now - backgroundMatchTimestamp).Seconds > 5)
                            {
                                System.IO.Directory.CreateDirectory(workingPath);
                                
                                string colorName = string.Format("_{0}_{1}_{2}_", (int)newColor.R, (int)newColor.G, (int)newColor.B);
                                string gemName = string.Format("{0}.{1}", PieceColor.Name, PieceColor.Modifiers);
                                string basePath = System.IO.Path.Combine(workingPath, gemName);

                                string thisPath = string.Format("{0}{1}.bmp", basePath, colorName);

                                cropped.Save(thisPath);
                                game.Debug(String.Format("Written out unknown gamepiece {0},{1}", x,y));

                                //System.Diagnostics.Debugger.Break();
                            }

                            
                        }
                        
                    }

                    if (!listGemColorStats.Contains(PieceColor))
                    {

                        //listGemColorStats.Add(PieceColor, Color.FromArgb( stats.Red.Mean, stats.Green.Mean, stats.Blue.Mean });
                        
                        
                    }
                    /*
                    if (debugMode)
                    {
                        if (!knownColors.Contains(PieceColor))
                        {
                            string currentFilePath = Path.Combine(workingPath, string.Format("{0}.bmp",knownColors.Count));

                            using (FileStream fs = new FileStream(currentFilePath, FileMode.Create, FileAccess.Write))
                            {
                                cropped.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                            }
                            debugConsole.AppendText("Saved color " + PieceColor + " to file " + currentFilePath + System.Environment.NewLine);
                            knownColors.Add(PieceColor);
                        }

                    }*/
                }
            }
            if (bMatchedAllPieces)
                backgroundMatchTimestamp = DateTime.Now;
            
            return bMatchedAllPieces && unmatchedCount < 3;

            // Build known averages
            
            //RED
            /*
             * 
             *
             * 0,0 0,2 0,5 0,6
             * 1,4 1,6
             * 3,2
             * 5,2
             * 6,7
             */
            /*
            List<Tuple<int,int>> redPairs = new List<Tuple<int,int>>();

            redPairs.Add(new Tuple<int,int>(0,0));
            redPairs.Add(new Tuple<int, int>(0, 2));
            redPairs.Add(new Tuple<int, int>(0, 5));
            redPairs.Add(new Tuple<int, int>(0, 6));
            redPairs.Add(new Tuple<int, int>(1, 3));
            redPairs.Add(new Tuple<int, int>(1, 5));
            redPairs.Add(new Tuple<int, int>(3, 2));
            redPairs.Add(new Tuple<int, int>(5, 2));
            redPairs.Add(new Tuple<int,int>(6,7));

            //BLUE
            /*
             * 
             *
             * 1,1 1,2
             * 2,6 2,7
             * 3,6
             * 4,7
             * 5,0 5,3 5,6
             * 6,5 6,6
             * 7,3 7,5
             */
            /*
            List<Tuple<int, int>> bluePairs = new List<Tuple<int, int>>();

            bluePairs.Add(new Tuple<int, int>(1, 1));
            bluePairs.Add(new Tuple<int, int>(1, 2));
            bluePairs.Add(new Tuple<int, int>(2, 6));
            bluePairs.Add(new Tuple<int, int>(2, 7));
            bluePairs.Add(new Tuple<int, int>(3, 6));
            bluePairs.Add(new Tuple<int, int>(4, 7));
            bluePairs.Add(new Tuple<int, int>(5, 0));
            bluePairs.Add(new Tuple<int, int>(5, 3));
            bluePairs.Add(new Tuple<int, int>(5, 6));
            bluePairs.Add(new Tuple<int, int>(6, 5));
            bluePairs.Add(new Tuple<int, int>(6, 6));
            bluePairs.Add(new Tuple<int, int>(7, 3));
            bluePairs.Add(new Tuple<int, int>(7, 5));
            */

            //GREEN
            /*
             * 
             *
             * 0,7
             * 3,1 3,5
             * 4,5
             * 6,4
             * 7,1
             */
            /*
            List<Tuple<int, int>> greenPairs = new List<Tuple<int, int>>();

            greenPairs.Add(new Tuple<int, int>(0, 7));
            greenPairs.Add(new Tuple<int, int>(3, 1));
            greenPairs.Add(new Tuple<int, int>(3, 5));
            greenPairs.Add(new Tuple<int, int>(4, 5));
            greenPairs.Add(new Tuple<int, int>(6, 4));
            greenPairs.Add(new Tuple<int, int>(7, 1));

            */
            //YELLOW
            /*
             * 0,3
             * 1,7
             * 2,1 2,3 2,4
             * 4,0
             * 5,5
             * 6,2
             * 7,6 7,7
             */
            /*
            List<Tuple<int, int>> yellowPairs = new List<Tuple<int, int>>();

            yellowPairs.Add(new Tuple<int, int>(0, 3));
            yellowPairs.Add(new Tuple<int, int>(1, 7));
            yellowPairs.Add(new Tuple<int, int>(2, 1));
            yellowPairs.Add(new Tuple<int, int>(2, 3));
            yellowPairs.Add(new Tuple<int, int>(2, 4));
            yellowPairs.Add(new Tuple<int, int>(4, 0));
            yellowPairs.Add(new Tuple<int, int>(5, 5));
            yellowPairs.Add(new Tuple<int, int>(6, 2));
            yellowPairs.Add(new Tuple<int, int>(7, 6));
            yellowPairs.Add(new Tuple<int, int>(7, 7));
            */

            //PURPLE
            /*
             * 1,0 1,4 1,6
             * 2,0
             * 4,2 4,6
             * 7,0 7,2
             */
            /*
            List<Tuple<int, int>> purplePairs = new List<Tuple<int, int>>();

            purplePairs.Add(new Tuple<int, int>(1, 0));
            purplePairs.Add(new Tuple<int, int>(1, 4));
            purplePairs.Add(new Tuple<int, int>(1, 6));
            purplePairs.Add(new Tuple<int, int>(2,0));
            purplePairs.Add(new Tuple<int, int>(4, 2));
            purplePairs.Add(new Tuple<int, int>(4, 6));
            purplePairs.Add(new Tuple<int, int>(7, 0));
            purplePairs.Add(new Tuple<int, int>(7, 2));
            */
            //WHITE
            /*
             * 0,1 
             * 2,2
             * 3,3 3,4
             * 6,1 6,3
             * 7,4
             */
            /*
            List<Tuple<int, int>> whitePairs = new List<Tuple<int, int>>();

            whitePairs.Add(new Tuple<int, int>(0, 1));
            whitePairs.Add(new Tuple<int, int>(2, 2));
            whitePairs.Add(new Tuple<int, int>(3, 3));
            whitePairs.Add(new Tuple<int, int>(3, 4));
            whitePairs.Add(new Tuple<int, int>(6, 1));
            whitePairs.Add(new Tuple<int, int>(6, 3));
            whitePairs.Add(new Tuple<int, int>(7, 4));
            */
            //ORANGE
            /*
             * 0,4 
             * 2,5
             * 3,0 3,7
             * 4,1 4,3 4,4
             * 5,1 5,4
             * 6,0
             */
            /*
            List<Tuple<int, int>> orangePairs = new List<Tuple<int, int>>();

            orangePairs.Add(new Tuple<int, int>(0, 4));
            orangePairs.Add(new Tuple<int, int>(2, 5));
            orangePairs.Add(new Tuple<int, int>(3, 0));
            orangePairs.Add(new Tuple<int, int>(3, 7));
            orangePairs.Add(new Tuple<int, int>(4, 1));
            orangePairs.Add(new Tuple<int, int>(4, 3));
            orangePairs.Add(new Tuple<int, int>(4, 4));
            orangePairs.Add(new Tuple<int, int>(5, 1));
            orangePairs.Add(new Tuple<int, int>(5, 4));
            orangePairs.Add(new Tuple<int, int>(6, 0));



            double rMean = 0;
            
            double gMean = 0;

            double bMean = 0;

            foreach (Tuple<int, int> pair in purplePairs)
            {
                rMean += pieceColors[pair.Item1,pair.Item2].R;
                
                gMean += pieceColors[pair.Item1, pair.Item2].G;

                bMean += pieceColors[pair.Item1, pair.Item2].B;
            }

            rMean = rMean / purplePairs.Count;
            gMean = gMean / purplePairs.Count;
            bMean = bMean / purplePairs.Count;
            listGemColorStats.Clear();
            listGemColorStats.Add(Color.Blue, Color.FromArgb( rMean, gMean, bMean });
            */
        }
        #endregion

    }
}

