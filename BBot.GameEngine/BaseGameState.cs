using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Contexts;
using System.IO;
using System.Linq;
using System.Threading;
using BBot.GameDefinitions;

namespace BBot.GameEngine.States
{
    public class BaseGameState
    {
        internal BBotGameEngine gameEngine;
        internal GameScreenDefinition gameScreen;

        private CancellationTokenSource ctsFindGamescreen = new CancellationTokenSource();

        public string Name;

        public BaseGameState()
        {
            gameScreen = new GameScreenDefinition(ctsFindGamescreen.Token);
            gameScreen.AssetName =  "-none-";
            gameScreen.MinimumConfidence =  1;

            Name = "Unnamed game state";
        }

        public virtual void Init(BBotGameEngine gameRef)
        {
            gameEngine = gameRef;
            gameScreen.ImageDebugAction = gameEngine.ImageDebugAction;
        }


        public virtual void Cleanup() {
            if (FindThread != null)
            {
                Thread.Sleep(10);
                if (Thread.CurrentThread.Equals(FindThread))
                    return;

                FindThread.Join();
                FindThread = null;

            }
        }

        public virtual void Pause() { }

        public virtual void Resume() { }

        public void Run(CancellationToken cancelToken)
        {
            cancelToken.Register(() => ctsFindGamescreen.Cancel());


            try
            {
                if (!gameEngine.GameExtents.HasValue)
                {
                    using (Bitmap screenSearchArea = GetSearchAreaBitmap())
                    {
                        TryFindState();
                    }
                }

                this.Update(cancelToken);
                this.Draw(cancelToken);
            }
            catch (OperationCanceledException) { }

        }

        public virtual void Update(CancellationToken cancelToken) { }

        public virtual void Draw(CancellationToken cancelToken) { }
        
        #region FindStateFromScreen


        private Thread FindThread;

        public MatchingPoint TryFindState(bool quickSearch = false)
        {
            MatchingPoint match;
            using (Bitmap screenSearchArea = GetSearchAreaBitmap())
            {

                match = gameScreen.FindGameScreenInImage(screenSearchArea, quickSearch);
                if (match.Confident)
                    gameEngine.UpdateGameExtents(match.X, match.Y);
            }

            return match;
        }


        private Bitmap GetSearchAreaBitmap() // PixelFormat format)
        {
            gameEngine.CaptureArea(); // Get search bitmap
                        
            // Will be overwritten by current gamescreen from game
            Rectangle searchLocation = new Rectangle(0, 0, gameScreen.GameScreenSize.Width, gameScreen.GameScreenSize.Height);

            if (Monitor.TryEnter(gameEngine.GameScreenLOCK, 1000))
            {
                try
                {
                    // Set search bounds to captured area
                    searchLocation = new Rectangle(0, 0, gameEngine.GameScreen.Width, gameEngine.GameScreen.Height);

                }
                finally
                {
                    Monitor.Exit(gameEngine.GameScreenLOCK);
                }
            }
            
            // Copy gamescreen to search area (this will add buffer of black pixels if required
            Bitmap searchArea = new Bitmap(searchLocation.Width,searchLocation.Height, PixelFormat.Format32bppArgb);

            if (Monitor.TryEnter(gameEngine.GameScreenLOCK,1000))
            {
                try
                {
                    using (Graphics g = Graphics.FromImage(searchArea))
                    {
                        g.DrawImage(gameEngine.GameScreen, searchLocation.Location);
                    }
                }
                finally
                {
                    Monitor.Exit(gameEngine.GameScreenLOCK);
                }
            }

            return searchArea;
        }


        #endregion

    }
}
