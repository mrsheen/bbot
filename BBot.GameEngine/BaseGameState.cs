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

        public string Name;

        public BaseGameState()
        {
            gameScreen = new GameScreenDefinition();
            gameScreen.AssetName =  "-none-";
            gameScreen.MinimumConfidence =  1;

            Name = "Unnamed game state";
        }

        public virtual void Init(BBotGameEngine gameRef)
        {
            gameEngine = gameRef;

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
           
            if (!gameEngine.GameExtents.HasValue)
                return;

            try
            {
                this.Update(cancelToken);
                this.Draw(cancelToken);
            }
            catch (OperationCanceledException) { }

        }

        public virtual void Update(CancellationToken cancelToken) { }

        public virtual void Draw(CancellationToken cancelToken) { }
        
        #region FindStateFromScreen


        private Thread FindThread;

        private void TryFindState()
        {

            //FindStateFromScreen(false);

            if (!gameEngine.GameExtents.HasValue)
                gameEngine.StateManager.PopState();

            return;
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
            
            if (!gameEngine.GameExtents.HasValue)
            {// Add buffer to given search area
                searchLocation.X += 20;
                searchLocation.Y += 20;
                searchLocation.Width += 40;
                searchLocation.Height += 40;                
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
