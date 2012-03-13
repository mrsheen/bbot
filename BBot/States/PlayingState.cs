using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AForge.Imaging;
using System.Threading;

namespace BBot.States
{
    public class PlayingState : BaseGameState
    {

        public PlayingState()
        {

            AssetName = "wholegame.playing";
            MinimumConfidence = 200000;

            BuildGemColorStats();
            delay = new int[GridSize + 4, GridSize + 4];
            bmpHeatmap = new Bitmap(BoardSize.Width, BoardSize.Height);
            bHuzzah = false;


            
            
        }

        //!TODO!Fill out pause/resume for ticker
        //public override void Cleanup() {}
        //!TODO!Fill out pause/resume for ticker
        // override void Pause() { }

        //public override void Resume() { }

        public override bool HandleEvents()
        {
            if (base.HandleEvents() || bHuzzah)
                return true;

            if (timer == null)
                timer = new Timer(new TimerCallback(GameOver), game, 65 * 1000, Timeout.Infinite);
            
            return false;
            
        }

        public override void Update()
        {
            if (bHuzzah)
                return; 

            GetBoardFromGame(game);
            ScanGrid(game);
            DoMoves(game);
        }

        public override void Draw()
        {
            TickDownDelay(game);
            game.PreviewScreen = bmpHeatmap;

        }

        #region Private fields


        private readonly Size BoardSize = new Size(320, 320); // The size of the Bejeweled gem grid
        private readonly Point BoardLocationOnGame = new Point(175, 54);
        private const int CellSize = 40; // Size of each cell in the grid
        private const int GridSize = 8;


        private const int MAX_DELAY = 400;

        private const int HEATMAP_UPDATE = 2000;
        private int heatmapCount = 0;

        private Bitmap bmpHeatmap;
        private Bitmap bmpBoard;

        private Color[,] Board = new Color[GridSize + 4, GridSize + 4]; // Matrix to hold the colour present in each grid cell


        //private static List<Color> knownColors = new List<Color>();
        //private string workingPath = Path.Combine(Directory.GetCurrentDirectory(), DateTime.Now.ToString("hhmm"));

        private System.Threading.Timer timer;

        private Dictionary<Color, List<double>> listGemColorStats;
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
            if (Monitor.TryEnter(game.GameScreenLOCK,10))
            {
                try {
                     bmpBoard = game.GameScreen.Clone(new Rectangle(BoardLocationOnGame, BoardSize), game.GameScreen.PixelFormat);
                }
                finally 
                {
                    Monitor.Exit(game.GameScreenLOCK);
                }
            }
            //lock (game.GameScreenLOCK)
            //{
            //    bmpBoard = game.GameScreen.Clone(new Rectangle(BoardLocationOnGame, BoardSize), game.GameScreen.PixelFormat);
            //}
        }

        private void BuildGemColorStats()
        {
            //RedMeanB    GreenMeanB    BlueMeanB
            listGemColorStats = new Dictionary<Color, List<double>>();
            listGemColorStats.Add(Color.White, new List<double> { 218.79, 218.79, 218.79 });
            listGemColorStats.Add(Color.Purple, new List<double> { 176.7325, 38.1, 177.2425 });
            listGemColorStats.Add(Color.Blue, new List<double> { 16.975, 109.5675, 204.7625 });
            listGemColorStats.Add(Color.Green, new List<double> { 43.06, 214.2775, 75.365 });
            listGemColorStats.Add(Color.Yellow, new List<double> { 227.01, 197.165, 28 });
            listGemColorStats.Add(Color.Orange, new List<double> { 238.49, 143.535, 55.7425 });
            listGemColorStats.Add(Color.Red, new List<double> { 242.855, 31.28, 62.07 });
        }

        // Check if two colours match
        private bool MatchColours(Color a, Color b)
        {
            return (a.Equals(b));

        }


        private void TickDownDelay(GameEngine game)
        {

            // Loop variables
            int iX;
            int iY;
            byte bIntense;

            float fRatio = 1F / MAX_DELAY;
            Heatmap.ClearHeatpoints();
            // Tick for each delay
            for (int i = 0; i < GridSize + 4; i++)
            {
                for (int j = 0; j < GridSize + 4; j++)
                {
                    delay[i, j] = delay[i, j] - 10;// tMove.Interval;
                    if (delay[i, j] < 0)
                        delay[i, j] = 0;

                    iX = CellSize * (i - 2);
                    iY = CellSize * (j - 2);
                    bIntense = (byte)(delay[i, j] * fRatio * Byte.MaxValue);
                    // Add heat point to heat points list
                    Heatmap.AddHeatpoint(new Heatmap.HeatPoint(iX, iY, bIntense));
                }
            }

            if (heatmapCount < HEATMAP_UPDATE)
            {
                heatmapCount++;
                return;
            }

            heatmapCount = 0;

            //lock (game.GameScreen)
            //{
            //    // Call CreateIntensityMask, give it the memory bitmap, and store the result back in the memory bitmap
            //    bmpHeatmap = Heatmap.CreateIntensityMask(game.GameScreen);
            //}
            // Colorize the memory bitmap and assign it as the picture boxes image
            bmpHeatmap = Heatmap.Colorize(bmpHeatmap, 255);

        }

        // 
        // Code adapted from William Henry's Java bot: http://mytopcoder.com/bejeweledBot
        // TODO: I did this the easy way by always moving the cell we are currently looking at (just to get the app running), but this isn't the most efficient method
        private void DoMoves(GameEngine game)
        {
            // Across
            for (int y = 2; y < GridSize + 2; y++)
            {
                // Down
                for (int x = 2; x < GridSize + 2; x++)
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
            int primaryDelay = MAX_DELAY / 2;


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

            int mouseX1 = startPoint.X + (CellSize * (x1 - 2)) + (CellSize / 2);
            int mouseY1 = startPoint.Y + (CellSize * (y1 - 2)) + (CellSize / 2);

            int mouseX2 = startPoint.X + (CellSize * (x2 - 2)) + (CellSize / 2);
            int mouseY2 = startPoint.Y + (CellSize * (y2 - 2)) + (CellSize / 2);

            //System.Threading.Thread.Sleep(1000);
            SendInputClass.Click(mouseX1, mouseY1);
            //System.Threading.Thread.Sleep(500);
            SendInputClass.Click(mouseX2, mouseY2);


            SendInputClass.Move(startPoint.X - 90, startPoint.Y + 150);
            

        }


        private bool CheckDelay(int x, int y)
        {
            return (delay[x, y] == 0);

        }

        private void SetDelay(int x, int y, int delayCount)
        {
            if (x < 0 || y < 0 || x >= GridSize + 4 || y >= GridSize + 4)
                return;


            int dampeningRadius = 4;
            double a = -2.0 * dampeningRadius * dampeningRadius / Math.Log(0.00005);

            // Tick for each delay
            for (int i = 0; i < GridSize + 4; i++)
            {
                for (int j = 0; j < GridSize + 4; j++)
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


        // Scan the gem Board and capture a coloured pixel from each cell
        private void ScanGrid(GameEngine game)
        {
            int top = 10;//topOffset;
            int left = 10; //leftOffset;

            // Across
            for (int y = 0; y < GridSize; y++)
            {
                // Down
                for (int x = 0; x < GridSize; x++)
                {
                    int boxLeft = (left + (CellSize * x));
                    int boxTop = (top + (CellSize * y));


                    // Capture a colour from this gem
                    Color PieceColor = bmpBoard.GetPixel(boxLeft, boxTop);

                    // Get image of current gem
                    Rectangle rect = new Rectangle(boxLeft, boxTop, CellSize / 2, CellSize / 2);
                    Bitmap cropped = bmpBoard.Clone(rect, bmpBoard.PixelFormat);

                    ImageStatistics stats = new ImageStatistics(cropped);

                    // Calculate best score
                    double bestScore = 255;
                    double curScore = 0;
                    foreach (KeyValuePair<Color, List<double>> item in listGemColorStats)
                    {
                        curScore = Math.Pow(item.Value[0] / 255 - stats.Red.Mean / 255, 2)
                            + Math.Pow(item.Value[1] / 255 - stats.Green.Mean / 255, 2)
                            + Math.Pow(item.Value[2] / 255 - stats.Blue.Mean / 255, 2);
                        if (curScore < bestScore)
                        {
                            PieceColor = item.Key;
                            if (item.Key == Color.YellowGreen)
                                PieceColor = Color.Yellow;
                            if (item.Key == Color.MediumPurple)
                                PieceColor = Color.Purple;
                            bestScore = curScore;
                        }
                    }

                    // Store it in the Board matrix at the correct position
                    Board[x + 2, y + 2] = PieceColor;

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
        }
        #endregion

    }
}

