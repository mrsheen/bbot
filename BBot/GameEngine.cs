using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Threading;
using BBot.States;

namespace BBot
{
    public class GameEngine
    {
        public readonly Size GameSize = new Size(525, 435);

        private const int TickPeriod = 50;

        public Object GameScreenLOCK = new Object();

        public Bitmap GameScreen; // Holds the captured area bitmap for each iteration
        private Rectangle ScreenRectangle;
        public Rectangle? GameExtents;


        public Bitmap PreviewScreen; // Holds the preview bitmap for each iteration

        public bool DebugMode { get; set; }

        public Stack<GameEvent> EventStack = new Stack<GameEvent>();
        public GameStateManager StateManager;

        private System.Threading.Timer tickTimer;


        public GameEngine(Rectangle screenBounds)
        {
            ScreenRectangle = screenBounds;
            GameScreen = new Bitmap(ScreenRectangle.Width, ScreenRectangle.Height);

            StateManager = new GameStateManager(this);

            StateManager.PushState(new UnknownState()); // Set initial state to menu

            EventStack.Push(new GameEvent(EngineEventType.ENGINE_INIT, null)); // Create engine_init event

            tickTimer = new Timer(new TimerCallback(GameTick), null, 0, TickPeriod);
        }


        public void Cleanup()
        {
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
            if (!GameExtents.HasValue)
            { // Extents found from whole screen, value will be +100/-100 from screen location
                newGameLocation.X += ScreenRectangle.X;
                newGameLocation.Y += ScreenRectangle.Y;
            }

            GameExtents = new Rectangle(newGameLocation, GameSize);
        }

        public void GameTick(object o)
        {
            if (tickTimer == null)
                return;
            // Game tick, do something
            if (Monitor.TryEnter(StateManager))
            {
                try
                {
                    // This gets fired every N seconds in order to perform moves
                    if (CaptureArea())
                        return;
                    
                    StateManager.Run();
                }
                finally
                {
                    Monitor.Exit(StateManager);
                }
            }

        }

        // Capture the specified screen area which we set up earlier
        public bool CaptureArea()
        {
            if (Monitor.TryEnter(GameScreenLOCK))
            {
                try
                {
                    if (GameExtents.HasValue && GameScreen.Size != GameExtents.Value.Size)
                        GameScreen = new Bitmap(GameExtents.Value.Width, GameExtents.Value.Height);

                    using (Graphics graphics = Graphics.FromImage(GameScreen))
                    {
                        graphics.CopyFromScreen(
                            GameExtents.HasValue ? GameExtents.Value.Location : ScreenRectangle.Location,
                            new Point(0, 0),
                            GameExtents.HasValue ? GameExtents.Value.Size : ScreenRectangle.Size);
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

        public FindBitmap.MatchingPoint FindStateFromScreen(Type classType, bool bQuickCheck = false)
        {
            BaseGameState state = new UnknownState();
            try
            {
                state = (BaseGameState)Activator.CreateInstance(classType);
            }
            catch (Exception)
            {

            }

            return state.FindStateFromScreen(bQuickCheck);
        }


    }

    public class GameStateManager
    {
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
            if (RunThread != null)
            {
                RunThread.Abort();
                RunThread = null;
            }

            while (states.Count > 0)
                states.Pop().Cleanup();

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
            if (states.Count == 0)
                return;

            if (states.Peek().HandleEvents())
                return;

            if (RunThread == null || RunThread.ThreadState == ThreadState.Stopped)
            {
                RunThread = new Thread(new ThreadStart(states.Peek().Run));
                RunThread.Name = String.Format("RunThread-{0}-{1}", states.Peek().AssetName, DateTime.Now);

            }
            if (RunThread.IsAlive)
                return;

            RunThread.Start();
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
        START_PLAYING,
        PAUSE_PLAYING,
        RESUME_PLAYING,
        FINISH_PLAYING,
        CHANGE_MENU



    }



}
