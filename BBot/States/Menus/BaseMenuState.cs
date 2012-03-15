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

        protected BaseMenuState() { }
        
        public override void Init(GameEngine gameRef)
        {
            base.Init(gameRef);

            bmpPreviousGameScreen = new Bitmap(1, 1);
            bFirstRun = true;
            SendInputClass.Move(0, 0); //!TODO!Move to a better location
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
                    game.StateManager.PushState((BaseGameState)myEvent.parameters);
                    return true;
                }


                if (myEvent.eventType == EngineEventType.RESUME_PLAYING)
                {
                    game.StateManager.PushState(new PlayingState());
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

            MatchingPoint match;

            findStates.Push(this);

            game.Debug("Looking for states from " + this.AssetName);
            

            while (findStates.Count > 0)
            {
                BaseGameState state = findStates.Pop();
                state.Init(game);
                //Look for different state
                match = state.FindStateFromScreen(state.AssetName.Equals(this.AssetName));
                if (match.Confident)
                {
                    // Check for this menu
                    if (this.AssetName.Equals(state.AssetName))
                    {
                        // Click 'yes' button to confirm restart
                        SendInputClass.Click(
                            game.GameExtents.Value.X + transitionClickOffset.X,
                            game.GameExtents.Value.Y + transitionClickOffset.Y);
                        System.Threading.Thread.Sleep(50);
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
            match = playingState.FindStateFromScreen();
            if (match.Confident)
            {
                // Started playing
                game.EventStack.Push(new GameEvent(EngineEventType.RESUME_PLAYING, null));
                game.Debug("Playingstate found");
                return;
            }

            //game.Debug("Nothing found, playing again");
            game.Debug("Nothing found");


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
