using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Threading;
using BBot.GameEngine.States;
using BBot.GameDefinitions;
using System.Configuration;

namespace BBot.GameEngine
{
    public sealed class BBotGameEngine : IDisposable
    {
        public readonly Size GameSize = new Size(525, 435);

        private const int TickPeriod = 50;
        private const int SearchBuffer = 40;

        public readonly Object GameScreenLOCK = new Object();
        public readonly Object PreviewScreenLOCK = new Object();
        


        public Bitmap GameScreen; // Holds the captured area bitmap for each iteration
        private Rectangle ScreenRectangle;
        public Rectangle? GameExtents;
        public Rectangle _SuggestedSearchArea;



        public Bitmap PreviewScreen; // Holds the preview bitmap for each iteration

        public bool DebugMode { get; set; }

        public GameStateManager StateManager;

        // CTS to completely stop engine, and dispose of all resources
        private CancellationTokenSource cancellationTokenSource;

        private System.Threading.Timer tickTimer;

        private bool bStarted = false;

        public BBotGameEngine(Rectangle screenBounds)
        {
            ScreenRectangle = screenBounds;
            GameScreen = new Bitmap(ScreenRectangle.Width, ScreenRectangle.Height);

            StateManager = new GameStateManager(this);

            CaptureArea();
        }

        public void Start()
        {
            DebugAction("Starting game engine");

            if (bStarted)
                return;

            bStarted = true;

            if (StateManager.NumberOfStates == 0)
                StateManager.PushState(new UnknownState()); // Set initial state to menu


            tickTimer = new Timer(new TimerCallback(GameTick), null, 0, TickPeriod);
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Stop()
        {
            DebugAction("Stopping game engine");

            if (tickTimer != null)
            {
                tickTimer.Dispose(); // Stop timer
                tickTimer = null;
            }
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }

            lock (StateManager)
            {
                StateManager.Cleanup();
            }

            bStarted = false;
            DebugAction("Game engine stopped");
        }

        public Action<String> DebugAction { get; set; }
        
        public Rectangle SuggestedSearchArea
        {
            get
            {
                if (_SuggestedSearchArea == null || _SuggestedSearchArea.Width == 0)
                    _SuggestedSearchArea = ScreenRectangle;

                return _SuggestedSearchArea;
            }
            private set
            {
                _SuggestedSearchArea = value;

                _SuggestedSearchArea.X -= SearchBuffer;
                _SuggestedSearchArea.Y -= SearchBuffer;
                _SuggestedSearchArea.Width += SearchBuffer*2;
                _SuggestedSearchArea.Height += SearchBuffer*2;

                // Constrain to visible screen
                _SuggestedSearchArea.X = Math.Max(_SuggestedSearchArea.X, 0);
                _SuggestedSearchArea.X = Math.Min(_SuggestedSearchArea.X, ScreenRectangle.Width + ScreenRectangle.X);

                _SuggestedSearchArea.Y = Math.Max(_SuggestedSearchArea.Y, 0);
                _SuggestedSearchArea.Y = Math.Min(_SuggestedSearchArea.Y, ScreenRectangle.Height + ScreenRectangle.Y);

                _SuggestedSearchArea.Width = Math.Max(_SuggestedSearchArea.Width, 0);
                _SuggestedSearchArea.Width = Math.Min(_SuggestedSearchArea.Width, ScreenRectangle.Width);

                _SuggestedSearchArea.Height = Math.Max(_SuggestedSearchArea.Height, 0);
                _SuggestedSearchArea.Height = Math.Min(_SuggestedSearchArea.Height, ScreenRectangle.Height);
            }
        }

        /// <summary>
        /// Save location of game screen, relative to captured area
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateSuggestedSearch(Rectangle newSearchRectangle, bool fromScreenRectangle = false)
        {
            if (fromScreenRectangle)
            { // Update extents from whole screen
                newSearchRectangle.X += ScreenRectangle.X;
                newSearchRectangle.Y += ScreenRectangle.Y;
            }

            SuggestedSearchArea = newSearchRectangle;
            GameExtents = null;
        }

        /// <summary>
        /// Save location of game screen, relative to captured area
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateGameExtents(int x, int y)
        {
            Point newGameLocation = new Point(x, y);

            if (GameExtents.HasValue)
            { // Update known extents, value will be +1/-1 from previous value
                newGameLocation.X += GameExtents.Value.X;
                newGameLocation.Y += GameExtents.Value.Y;
            }
            else
            { // Extents found from search area, value will be +100/-100
                newGameLocation.X += SuggestedSearchArea.X;// -SearchBuffer;
                newGameLocation.Y += SuggestedSearchArea.Y;// -SearchBuffer;
            }

            GameExtents = new Rectangle(newGameLocation, GameSize);

            // Write game location to config file
            /*Properties.Settings.Default.GameExtents = newGameLocation;
            Properties.Settings.Default.GameSize = GameSize;

            Properties.Settings.Default.Save();
            */
            // Set suggested search area to found game extents (adds on a buffer)
            SuggestedSearchArea = GameExtents.Value;
        }

        public void GameTick(object o)
        {
            if (tickTimer == null)
                return;
            // Game tick, do something

            // This gets fired every N seconds in order to perform moves
            if (CaptureArea())
                return;

            StateManager.Run();

        }

        // Capture the specified screen area which we set up earlier
        public bool CaptureArea()
        {
            if (Monitor.TryEnter(GameScreenLOCK))
            {
                try
                {
                    Rectangle searchRectangle = SuggestedSearchArea;

                    if (GameExtents.HasValue && GameExtents.Value != searchRectangle)
                        SuggestedSearchArea = searchRectangle = GameExtents.Value;

                    if (GameScreen.Size != searchRectangle.Size)
                        GameScreen = new Bitmap(searchRectangle.Width, searchRectangle.Height);

                    using (Graphics graphics = Graphics.FromImage(GameScreen))
                    {
                        graphics.CopyFromScreen(searchRectangle.Location, new Point(0, 0), searchRectangle.Size);
                    }

                    return false;
                }
                finally
                {
                    Monitor.Exit(GameScreenLOCK);
                }
            }

            return true;
        }

        public void Dispose()
        {
            Stop();

            // free managed resources
            if (GameScreen != null)
            {
                GameScreen.Dispose();
                GameScreen = null;
            }
            if (PreviewScreen != null)
            {
                PreviewScreen.Dispose();
                PreviewScreen = null;
            }

        }
    }



}
