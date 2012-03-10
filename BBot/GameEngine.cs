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

        public Bitmap GameScreen; // Holds the captured area bitmap for each iteration
        public Rectangle ScreenRectangle;
        public Rectangle? GameExtentsOnScreen;
        

        public Bitmap PreviewScreen; // Holds the preview bitmap for each iteration

        public bool DebugMode { get; set; }

        public Stack<GameEvent> EventStack = new Stack<GameEvent>();
        public GameStateManager StateManager;

        private System.Threading.Timer tickTimer;
        

        public GameEngine(int screenWidth, int screenHeight)
        {
            GameScreen = new Bitmap(GameSize.Width, GameSize.Height);

            ScreenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

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

        public void UpdateGameExtents(int x, int y)
        {            
            GameExtentsOnScreen = new Rectangle(new Point(x, y), GameSize);
        }

        
        public void GameTick(object o)
        {
            if (tickTimer == null)
                return;
            // Game tick, do something
            if (Monitor.TryEnter(StateManager))
            {
                try {
                    // This gets fired every N seconds in order to perform moves
                CaptureArea();
                if (StateManager.HandleEvents())
                    return;


                StateManager.Update();
                StateManager.Draw();
                }
                finally
                {
                    Monitor.Exit(StateManager);
                }
            }
            
        }

        // Capture the specified screen area which we set up earlier
        private void CaptureArea()
        {
            int captureX = 0, captureY = 0;
            if (GameExtentsOnScreen.HasValue)
            {
                captureX = GameExtentsOnScreen.Value.X;
                captureY = GameExtentsOnScreen.Value.Y;
            }
            lock (GameScreen)
            {
                using (Graphics graphics = Graphics.FromImage(GameScreen))
                {
                    graphics.CopyFromScreen(captureX, captureY, 0, 0, GameSize);
                }
            }
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
        private Thread UpdateThread;
        private Thread StartThread;
        

        private GameEngine game;

        public GameStateManager(GameEngine gameRef)
        {
            game = gameRef;
            states = new Stack<BaseGameState>();

        }

        public void Cleanup()
        {
            if (UpdateThread != null)
            {
                UpdateThread.Abort();
                UpdateThread = null;
            }

            if (StartThread != null)
            {
                StartThread.Abort();
                StartThread = null;
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
            states.Peek().Start();

            Thread.Sleep(500);
            
        }

        public void PushState(BaseGameState newState)
        {
            game.Debug(String.Format("Pushing state from {0} to {1}", states.Count > 0 ? states.Peek().AssetName : "-none-", newState.AssetName));
            if (states.Count > 0)
                states.Peek().Pause();

            states.Push(newState);
            states.Peek().Init(game);
            SendInputClass.Move(0, 0);
            Thread.Sleep(500);
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


        public bool HandleEvents()
        {
            if (states.Count == 0)
                return true;

            try
            {
                if (StartThread == null || StartThread.ThreadState == ThreadState.Stopped)
                    StartThread = new Thread(new ThreadStart(StartState));

                if (StartThread.IsAlive)
                    throw new ApplicationException("BaseGameState.Start() already requested");


                StartThread.Start();
                //states.Peek().Start();
            }
            catch (Exception)
            {
                
            }


            return states.Peek().HandleEvents();
        }

        private void StartState()
        {
            try
            {
                if (states.Count > 0)
                    states.Peek().Start();
            }
            catch (Exception) { }
        }

        public void Update()
        {
            if (states.Count > 0)
            {
                if (UpdateThread == null || UpdateThread.ThreadState == ThreadState.Stopped)
                    UpdateThread = new Thread(new ThreadStart(states.Peek().Update));

                if (UpdateThread.IsAlive)
                    return;

                
                UpdateThread.Start();
                //states.Peek().Update(game);
            }
        }
        public void Draw()
        {
            if (states.Count > 0)
                states.Peek().Draw();
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
