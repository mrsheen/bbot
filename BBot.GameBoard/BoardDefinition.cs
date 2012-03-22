using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BBot.GameDefinitions
{

    [Serializable()]
    public class BoardDefinition
    {
        public readonly Size BoardSize = new Size(320, 320); // The size of the Bejeweled gem grid (in pixels, GridSize * CellSize)
        public readonly int CellSize = 40; // Size of each cell in the grid (in pixels)
        public readonly int GridSize = 8; // Size of grid (number of rows/columns)

        public Gem[,] Board; // Two-dimensional array of game pieces representing the board

        private Bitmap bmpRawBoard;
        private Bitmap bmpGeneratedBoard;

        private Gem[,] fullBoard; // Matrix to hold the colour present in each grid cell, with a buffer to simplify matching at edges

        public BoardDefinition()
        {
            Board = new Gem[GridSize, GridSize];
            fullBoard = new Gem[GridSize + 6, GridSize + 6];
        }
        /*
            *Scenario 1*
            Given an image of a game board, synthesize a representation into a useful form. This will use a set of known heuristics for each 
            game piece, and make a best-guess effort to identify game pieces on the game board.
        */

        public void GetBoardFromImage(Bitmap bmpBoardImage)
        {
            bmpRawBoard = bmpBoardImage;
            // Fill Board with gems
        }

        /*
            *Scenario 2*
            Given a game board, find any possible moves. This will use an exhaustive search to determine any valid moves which generate a match
            for the current board. Also, find best possible move. This will make a best-guess effort to sort valid moves by likelihood to produce
            large chains or multiple matches.
        */
        public void GetValidMovesFromBoard()
        {

        }

        /*
            *Scenario 3*
            Given a game board and a valid move, determine subsequent game board state. This will utilise game play rules to simulate the effects
            of a move.
        */
        public void MakeMoveOnBoard()
        {

        }

        /*
            *Scenario 4* 
            Given an image of a game board, improve known heuristics by manually identifying game pieces. This will use suggested matches to overwrite
            known heuristics for game pieces.
        */
        public void IdentifyGamePiece()
        {

        }

        /*
            *Scenario 5*
            Given a game board, create a graphical representation. This will generate an image based on the board.
        */
        public Bitmap GetBoardAsImage()
        {
            return new Bitmap(BoardSize.Width, BoardSize.Height);
        }

        #region Private Helpers

        // Scan the gem Board and capture a coloured pixel from each cell
        private bool ScanGrid()
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
            /*
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

            */

            // Across
            for (int x = 0; x < GridSize; x++)
            {
                // Down
                for (int y = 0; y < GridSize; y++)
                {
                    int boxLeft = left + (CellSize * x);
                    int boxTop = top + (CellSize * y);



                    /*
                    // Get image of current gem
                    Rectangle rect = new Rectangle(boxLeft, boxTop, CellSize - 2 * (CellSize / CellExtractionFactor), CellSize - 2 * (CellSize / CellExtractionFactor));
                    Bitmap cropped = bmpBoard.Clone(rect, bmpBoard.PixelFormat);

                    ImageStatistics stats = new ImageStatistics(cropped);

                    // Capture a colour from this gem
                    Gem PieceColor = new Gem { Name = GemColor.None, Modifiers = GemModifier.None, Color = Color.FromArgb(255, (int)stats.Red.Mean, (int)stats.Green.Mean, (int)stats.Blue.Mean) };

                    // Calculate best score
                    double bestScore = 255 * 3; // what should this be set to?
                    double curScore = 0;

                    foreach (Gem gem in listGemColorStats)
                    {

                        curScore = Math.Pow(gem.Color.R - stats.Red.Mean, 2)
                                 + Math.Pow(gem.Color.G - stats.Green.Mean, 2)
                                 + Math.Pow(gem.Color.B - stats.Blue.Mean, 2);


                        if (curScore < bestScore)
                        {
                            PieceColor = gem;
                            bestScore = curScore;

                        }
                    }
                    scores.Add(bestScore);
                    */
                    /*
                    // Store it in the Board matrix at the correct position
                    Board.Board[x + 3, y + 3] = PieceColor;

                    Color newColor = Color.FromArgb(255, (int)stats.Red.Mean, (int)stats.Green.Mean, (int)stats.Blue.Mean);
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
                                game.DebugAction(String.Format("Written out unknown gamepiece {0},{1}", x, y));

                                //System.Diagnostics.Debugger.Break();
                            }


                        }

                    }
                    
                    if (!listGemColorStats.Contains(PieceColor))
                    {

                        //listGemColorStats.Add(PieceColor, Color.FromArgb( stats.Red.Mean, stats.Green.Mean, stats.Blue.Mean });


                    }*/
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

            return bMatchedAllPieces && unmatchedCount < 3;

        }


        // 
        // Code adapted from William Henry's Java bot: http://mytopcoder.com/bejeweledBot
        // TODO: I did this the easy way by always moving the cell we are currently looking at (just to get the app running), but this isn't the most efficient method
        private void DoMoves()
        {
            /*
            // Across
            for (int y = GridSize + 3 - 1; y >= 3; y--)
            {
                // Down
                for (int x = 3; x < GridSize + 3; x++)
                {
                    // x
                    // -
                    // x
                    // x
                    if (MatchColours(Board.Board[x, y], Board.Board[x, y + 2]) && MatchColours(Board.Board[x, y + 2], Board.Board[x, y + 3]))
                    {
                        MakeMove(game, x, y, x, y + 1);
                    }


                    // - x
                    // x
                    // x

                    if (MatchColours(Board.Board[x, y], Board.Board[x - 1, y + 1]) && MatchColours(Board.Board[x - 1, y + 1], Board.Board[x - 1, y + 2]))
                    {
                        MakeMove(game, x, y, (x - 1), y);
                    }
                    // x
                    // - x
                    // x

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y + 1]) && MatchColours(Board.Board[x + 1, y + 1], Board.Board[x, y + 2]))
                    {
                        MakeMove(game, (x + 1), (y + 1), x, (y + 1));
                    }

                    // x
                    // x
                    // - x

                    if (MatchColours(Board.Board[x, y], Board.Board[x, y + 1]) && MatchColours(Board.Board[x, y + 1], Board.Board[x + 1, y + 2]))
                    {
                        MakeMove(game, (x + 1), (y + 2), x, (y + 2));
                    }

                    // x
                    // x
                    // -
                    // x

                    if (MatchColours(Board.Board[x, y], Board.Board[x, y + 1]) && MatchColours(Board.Board[x, y + 1], Board.Board[x, y + 3]))
                    {
                        MakeMove(game, x, (y + 3), x, (y + 2));
                    }

                    // x -
                    //   x
                    //   x

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y + 1]) && MatchColours(Board.Board[x + 1, y + 1], Board.Board[x + 1, y + 2]))
                    {
                        MakeMove(game, x, y, (x + 1), y);
                    }

                    //   x
                    // x -
                    //   x

                    if (MatchColours(Board.Board[x, y], Board.Board[x - 1, y + 1]) && MatchColours(Board.Board[x - 1, y + 1], Board.Board[x, y + 2]))
                    {
                        MakeMove(game, (x - 1), (y + 1), (x), (y + 1));
                    }

                    //   x
                    //   x
                    // x -

                    if (MatchColours(Board.Board[x, y], Board.Board[x, y + 1]) && MatchColours(Board.Board[x, y + 1], Board.Board[x - 1, y + 2]))
                    {
                        MakeMove(game, (x - 1), (y + 2), x, (y + 2));
                    }

                    // xx-x

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y]) && MatchColours(Board.Board[x + 1, y], Board.Board[x + 3, y]))
                    {
                        MakeMove(game, (x + 3), y, (x + 2), y);
                    }

                    // x--
                    // -xx

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y + 1]) && MatchColours(Board.Board[x + 1, y + 1], Board.Board[x + 2, y + 1]))
                    {
                        MakeMove(game, x, y, x, (y + 1));
                    }

                    // -x-
                    // x-x

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y + 1]) && MatchColours(Board.Board[x + 1, y + 1], Board.Board[x - 1, y + 1]))
                    {
                        MakeMove(game, x, y, x, (y + 1));
                    }

                    // --x
                    // xx-

                    if (MatchColours(Board.Board[x, y], Board.Board[x - 1, y + 1]) && MatchColours(Board.Board[x - 1, y + 1], Board.Board[x - 2, y + 1]))
                    {
                        MakeMove(game, x, y, x, (y + 1));
                    }

                    // x-xx

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 2, y]) && MatchColours(Board.Board[x + 2, y], Board.Board[x + 3, y]))
                    {
                        MakeMove(game, x, y, (x + 1), y);
                    }

                    // -xx
                    // x--

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y]) && MatchColours(Board.Board[x + 1, y], Board.Board[x - 1, y + 1]))
                    {
                        MakeMove(game, (x - 1), (y + 1), (x - 1), y);
                    }

                    // x-x
                    // -x-

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y + 1]) && MatchColours(Board.Board[x + 1, y + 1], Board.Board[x + 2, y]))
                    {
                        MakeMove(game, (x + 1), (y + 1), (x + 1), (y));
                    }

                    // xx-
                    // --x

                    if (MatchColours(Board.Board[x, y], Board.Board[x + 1, y]) && MatchColours(Board.Board[x + 1, y], Board.Board[x + 2, y + 1]))
                    {

                        MakeMove(game, (x + 2), (y + 1), (x + 2), y);
                    }

                }
            }

            */
        }



        private void MakeMove(int x1, int y1, int x2, int y2)
        {
            /*
            int primaryDelay = MAX_DELAY;


            if (!(CheckDelay(x1, y1) && CheckDelay(x2, y2)))
                return;


            //if (debugMode)
            //game.DebugAction(String.Format("Move made: {0},{1} - {2},{3}", new Object[] { x1 - 2, y1 - 2, x2 - 2, y2 - 2 }));
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
            //SendInputClass.Click(mouseX1, mouseY1);
            //System.Threading.Thread.Sleep(10);
            //SendInputClass.Click(mouseX2, mouseY2);
            //System.Threading.Thread.Sleep(10);
            game.DebugAction(string.Format("clickX: {0}, clickY: {1}", mouseX1, mouseY1));
            //Thread.Sleep(1500);
            */
            //SendInputClass.Click(startPoint.X - 90, startPoint.Y + 150);


        }


        private Bitmap GenerateBoardImage()
        {
            Bitmap bmpBoardGems = new Bitmap(BoardSize.Width, BoardSize.Height);


            /*
            using (Graphics g = Graphics.FromImage(bmpBoardGems))
            {
                // Across
                for (int x = 0; x < Board.GridSize; x++)
                {
                    // Down
                    for (int y = 0; y < Board.GridSize; y++)
                    {
                        Gem boardGem = Board.Board[x + 3, y + 3];



                        g.FillRectangle(new SolidBrush(GemDefinitions.GetDisplayColorForGem(boardGem.Name)), x * Board.Cellsize, y * Board.Cellsize, Board.Cellsize, Board.Cellsize);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Star))
                            g.FillRectangle(new SolidBrush(Color.White), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Hypercube))
                            g.FillRectangle(new SolidBrush(Color.LimeGreen), x * Board.Cellsize, y * Board.Cellsize, Board.Cellsize, Board.Cellsize);


                        if (boardGem.Modifiers.HasFlag(GemModifier.Multiplier))
                            g.FillRectangle(new SolidBrush(Color.HotPink), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Background))
                            g.FillRectangle(new SolidBrush(Color.Black), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);

                        if (boardGem.Modifiers.HasFlag(GemModifier.Coin))
                            g.FillRectangle(new SolidBrush(Color.Yellow), x * Board.Cellsize + (Board.Cellsize / 4), y * Board.Cellsize + (Board.Cellsize / 4), Board.Cellsize / 2, Board.Cellsize / 2);
                    }
                }
            }*/

            return bmpBoardGems;

        }

        #endregion



    }
}
