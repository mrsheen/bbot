using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using BBot.GameDefinitions;


namespace BBot.GameEngine.States
{
    public abstract class BaseMenuState : BaseGameState
    {
        protected Stack<BaseGameState> findStates = new Stack<BaseGameState>();
        protected Point transitionClickOffset; // location of transition button relative to game screen
        protected BaseGameState transitionState;
        

        private Bitmap bmpPreviousGameScreen;

        private DateTime previousTimestamp;
        private int checkCount;
        private const int DeepSearch = 3;

        protected BaseMenuState() { }
        
        public override void Init(BBotGameEngine gameRef)
        {
            base.Init(gameRef);

            bmpPreviousGameScreen = new Bitmap(1, 1);
            //SendInputClass.Move(0, 0); //!TODO!Move to a better location

            previousTimestamp = DateTime.Now.AddMilliseconds(-5001);
            checkCount = 3;

        }

        public override void Update(CancellationToken token)
        {
            //!TODO!Timer on this
            if ((DateTime.Now - previousTimestamp).TotalMilliseconds < 5000)
            {
                findStates.Clear(); // need to empty this, it fills up from multiple Update(CancellationToken cancelToken) calls
                return; // Only every five seconds
            }
            
            checkCount++;

            
            MatchingPoint match;

            findStates.Push(this);

            gameEngine.DebugAction("Looking for states from " + this.Name);
            gameEngine.DebugAction("Other states: " + findStates.Count);
            

            while (findStates.Count > 0)
            {
                BaseGameState state = findStates.Pop();
                if (state != this)
                    state.Init(gameEngine);
                //Look for different state (first time check exact only)
                match = state.TryFindState(checkCount >= DeepSearch || state.Name.Equals(this.Name));
                if (match.Confident)
                {
                    // Check for this menu
                    if (this.Name.Equals(state.Name))
                    {
                        // Click 'yes' button to confirm restart
                        gameEngine.MakeMove(
                            gameEngine.GameExtents.Value.X + transitionClickOffset.X,
                            gameEngine.GameExtents.Value.Y + transitionClickOffset.Y);
                        System.Threading.Thread.Sleep(400);
                        // Assume next state
                        gameEngine.StateManager.ChangeState(transitionState);
                        return;
                    }

                    // Started playing
                    gameEngine.StateManager.ChangeState(state);
                    gameEngine.DebugAction("MenuState found: " + state.Name);
                    return;
                }

            }

            /*
            PlayingState playingState = new PlayingState();
            playingState.Init(game);
            match = playingState.FindStateFromScreen(true);
            if (match.Confident)
            {
                // Started playing
                game.EventStack.Push(new GameEvent(EngineEventType.RESUME_PLAYING, playingState));
                game.DebugAction("Playingstate found");
                return;
            }
            */
            //game.DebugAction("Nothing found, playing again");
            gameEngine.DebugAction("Nothing found");

            checkCount = checkCount % DeepSearch;
            
            previousTimestamp = DateTime.Now;

            return;
        }

        public override void Draw(CancellationToken cancelToken)
        {
            //lock (game.GameScreen)
            //{
            //    // Save copy of current screen for next time
            //    bmpPreviousGameScreen = game.GameScreen;
            //}
        }


    }
}
