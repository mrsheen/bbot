using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Threading;
using BBot.States;
using System.Configuration;

namespace BBot
{
    public class GameEngine
    {
        public readonly Size GameSize = new Size(525, 435);

        private const int TickPeriod = 50;

        public readonly Object GameScreenLOCK = new Object();



        public Bitmap GameScreen; // Holds the captured area bitmap for each iteration
        private Rectangle ScreenRectangle;
        public Rectangle? GameExtents;
        public Rectangle _SuggestedSearchArea;



        public Bitmap PreviewScreen; // Holds the preview bitmap for each iteration

        public bool DebugMode { get; set; }

        public Stack<GameEvent> EventStack = new Stack<GameEvent>();
        public GameStateManager StateManager;
        public FindBitmapWorker findBitmapWorker = new FindBitmapWorker();

        public bool StopRequested = false;

        private System.Threading.Timer tickTimer;


        public GameEngine(Rectangle screenBounds)
        {
            ScreenRectangle = screenBounds;
            GameScreen = new Bitmap(ScreenRectangle.Width, ScreenRectangle.Height);

            StateManager = new GameStateManager(this);
            StateManager.PushState(new UnknownState()); // Set initial state to menu


            EventStack.Push(new GameEvent(EngineEventType.ENGINE_INIT, null)); // Create engine_init event

            
            GameExtents = new Rectangle(Properties.Settings.Default.GameExtents, Properties.Settings.Default.GameSize);
            CaptureArea();
        }

        public void Start()
        {
            tickTimer = new Timer(new TimerCallback(GameTick), null, 0, TickPeriod);
        }


        public void Cleanup()
        {
            StopRequested = true;
            if (tickTimer != null)
            {
                tickTimer.Dispose(); // Stop timer
                tickTimer = null;
            }
            EventStack.Push(new GameEvent(EngineEventType.ENGINE_SHUTDOWN, null)); // Create engine_shutdown event
            lock (StateManager)
            {
                StateManager.Cleanup();
            }
        }

        public delegate void DebugDelegate(string debugMessage);
        public event DebugDelegate DebugEvent;

        public void Debug(string debugMessage)
        {
            if (DebugEvent != null)
                DebugEvent(debugMessage);
        }

        private const int SearchBuffer = 40;
        public Rectangle SuggestedSearchArea
        {
            get
            {
                if (_SuggestedSearchArea == null)
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
            Properties.Settings.Default.GameExtents = newGameLocation;
            Properties.Settings.Default.GameSize = GameSize;

            Properties.Settings.Default.Save();

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
    }

    public class GameStateManager
    {
        public readonly Object StateManagerLOCK = new Object();
        private Stack<BaseGameState> states;

        private GameEngine game;
        private Thread RunThread;

        public GameStateManager(GameEngine gameRef)
        {
            game = gameRef;
            states = new Stack<BaseGameState>();

        }

        public void Cleanup()
        {

            game.findBitmapWorker.StopRequested = true;

            if (RunThread != null)
            {
                if (states.Count > 0)
                    states.Peek().StopRequested = true;

                if (RunThread.ThreadState == ThreadState.Running)
                    RunThread.Join();
                RunThread = null;
            }

            while (states.Count > 0)
                states.Pop().Cleanup();

            game.findBitmapWorker.StopRequested = false;

        }

        public void ChangeState(BaseGameState newState)
        {
            game.Debug(String.Format("Changing state from {0} to {1}", states.Count > 0 ? states.Peek().AssetName : "-none-", newState.AssetName));
            if (states.Count > 0)
                states.Pop().Cleanup();

            states.Push(newState);
            states.Peek().Init(game);
        }

        public void PushState(BaseGameState newState)
        {
            game.Debug(String.Format("Pushing state from {0} to {1}", states.Count > 0 ? states.Peek().AssetName : "-none-", newState.AssetName));
            if (states.Count > 0)
                states.Peek().Pause();

            states.Push(newState);
            states.Peek().Init(game);
            //SendInputClass.Move(0, 0);// TODO Move to form pause button in some way
        }

        public BaseGameState PopState()
        {
            BaseGameState state = null;
            if (states.Count > 0)
            {
                state = states.Pop();
                state.Cleanup();
            }

            if (states.Count > 0)
                states.Peek().Resume();

            return state;
        }
        public void Run()
        {
            if (Monitor.TryEnter(StateManagerLOCK))
            {
                try
                {

                    if (states.Count == 0)
                        return;

                    // Make certain bitmapworker will run
                    game.findBitmapWorker.StopRequested = false;

                    if (states.Peek().HandleEvents())
                        return;

                    if (RunThread == null || RunThread.ThreadState == ThreadState.Stopped)
                    {
                        RunThread = new Thread(new ThreadStart(states.Peek().Run));
                        RunThread.Name = String.Format("RunThread-{0}-{1}", states.Peek().AssetName, DateTime.Now);
                        return;
                    }

                    if (RunThread.IsAlive)
                        return;

                    try
                    {
                        if (RunThread.ThreadState == ThreadState.Unstarted)
                            RunThread.Start();
                    }
                    catch (Exception) { }
                }
                finally
                {
                    Monitor.Exit(StateManagerLOCK);
                }
            }


        }
    }



    public struct GameEvent
    {
        public EngineEventType eventType;
        public Object parameters;
        public GameEvent(EngineEventType type, Object p)
        {
            eventType = type;
            parameters = p;
        }


    }

    public enum EngineEventType
    {
        ENGINE_INIT,
        ENGINE_SHUTDOWN,
        FIND_STATE_HINT,
        START_PLAYING,
        PAUSE_PLAYING,
        RESUME_PLAYING,
        FINISH_PLAYING,
        CHANGE_MENU



    }



}
