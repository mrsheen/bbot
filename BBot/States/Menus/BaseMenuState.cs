using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public abstract class BaseMenuState : BaseGameState
    {
        protected Stack<BaseGameState> findStates = new Stack<BaseGameState>();
        protected Point transitionClickOffset; // location of transition button relative to game screen
        protected BaseGameState transitionState;
        

        private Bitmap bmpPreviousGameScreen;
        private bool bFirstRun = false;

        private DateTime previousTimestamp;
        private int checkCount;
        private const int DeepSearch = 3;

        protected BaseMenuState() { }
        
        public override void Init(GameEngine gameRef)
        {
            base.Init(gameRef);

            bmpPreviousGameScreen = new Bitmap(1, 1);
            bFirstRun = true;
            SendInputClass.Move(0, 0); //!TODO!Move to a better location

            previousTimestamp = DateTime.Now.AddMilliseconds(-5001);
            checkCount = 3;

        }


        public override bool HandleEvents()
        {
            if (base.HandleEvents())
                return true;

            while (game.EventStack.Count > 0)
            {
                GameEvent myEvent = game.EventStack.Pop();

                if (myEvent.eventType == EngineEventType.CHANGE_MENU)
                {
                    game.StateManager.ChangeState((BaseGameState)myEvent.parameters);
                    return true;
                }


                if (myEvent.eventType == EngineEventType.RESUME_PLAYING)
                {

                    game.StateManager.ChangeState((BaseGameState)myEvent.parameters);
                    return true;
                }

            }

            if (bFirstRun)
            {
                System.Threading.Thread.Sleep(200);
                bFirstRun = false;
                return true;
            }




            return false;
        }

        public override void Update()
        {
            //!TODO!Timer on this
            if ((DateTime.Now - previousTimestamp).TotalMilliseconds < 5000)
            {
                findStates.Clear(); // need to empty this, it fills up from multiple Update() calls
                return; // Only every five seconds
            }
            game.Debug("Looking for menus from within unknown state");
            checkCount++;
            

            MatchingPoint match;

            findStates.Push(this);

            game.Debug("Looking for states from " + this.AssetName);
            game.Debug("Other states: " + findStates.Count);
            

            while (findStates.Count > 0)
            {
                BaseGameState state = findStates.Pop();
                if (state != this)
                    state.Init(game);
                //Look for different state (first time check exact only)
                match = state.FindStateFromScreen(checkCount >= DeepSearch || state.AssetName.Equals(this.AssetName));
                if (match.Confident)
                {
                    // Check for this menu
                    if (this.AssetName.Equals(state.AssetName))
                    {
                        // Click 'yes' button to confirm restart
                        SendInputClass.Click(
                            game.GameExtents.Value.X + transitionClickOffset.X,
                            game.GameExtents.Value.Y + transitionClickOffset.Y);
                        System.Threading.Thread.Sleep(400);
                        SendInputClass.Move(0, 0);
                        // Assume next state
                        game.EventStack.Push(new GameEvent(EngineEventType.CHANGE_MENU, transitionState));
                        return;
                    }

                    // Started playing
                    game.EventStack.Push(new GameEvent(EngineEventType.CHANGE_MENU, state));
                    game.Debug("MenuState found: " + state.AssetName);
                    return;
                }

            }


            PlayingState playingState = new PlayingState();
            playingState.Init(game);
            match = playingState.FindStateFromScreen(true);
            if (match.Confident)
            {
                // Started playing
                game.EventStack.Push(new GameEvent(EngineEventType.RESUME_PLAYING, playingState));
                game.Debug("Playingstate found");
                return;
            }

            //game.Debug("Nothing found, playing again");
            game.Debug("Nothing found");

            checkCount = checkCount % DeepSearch;
            
            previousTimestamp = DateTime.Now;

            return;
        }

        public override void Draw()
        {
            //lock (game.GameScreen)
            //{
            //    // Save copy of current screen for next time
            //    bmpPreviousGameScreen = game.GameScreen;
            //}
        }


    }
}
