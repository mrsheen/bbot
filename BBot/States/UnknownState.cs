using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public partial class UnknownState : BaseGameState
    {
        
        public UnknownState()
        {
            AssetName = "-none-";
            MinimumConfidence =1;

        }

        private Bitmap bmpPreviousGameScreen;
        private DateTime previousTimestamp;
        private int checkCount;
        private const int DeepSearch = 3;


        public override void Init(GameEngine gameRef)
        {
            base.Init(gameRef);

            bmpPreviousGameScreen = new Bitmap(1, 1);
            previousTimestamp = DateTime.Now.AddMilliseconds(-5001);
            checkCount = 0;
        }
        public override void Cleanup() { }

        public override void Pause() { }

        public override void Resume() { }

        public override bool HandleEvents()
        {
            while (game.EventStack.Count > 0)
            {
                GameEvent myEvent = game.EventStack.Pop();

                if (myEvent.eventType == EngineEventType.CHANGE_MENU)
                {
                    game.StateManager.PushState((BaseGameState)myEvent.parameters);
                    return true;
                }

                if (myEvent.eventType == EngineEventType.START_PLAYING)
                {
                    game.StateManager.PushState(new PlayingState());
                    return true;
                }

            }
            return false;
        }

        public override void Update()
        {

            //!TODO!Timer on this
            if ((DateTime.Now - previousTimestamp).TotalMilliseconds < 5000)
                return; // Only every five seconds
            game.Debug("Looking for menus from within unknown state");
            checkCount++;
            FindBitmap.MatchingPoint match;

            // Look for change in screen to signify external action
            if (1==1) //if (bmpPreviousGameScreen != game.BoardScreen)
            {//!TODO!Replace with actual call to bitmap compare
                /*
                // Look for different state
                match = game.FindStateFromScreen(PlayingState.Instance);
                if (match.Confident)
                {
                    // Started playing
                    game.EventStack.Push(new GameEvent(EngineEventType.START_PLAYING, null));
                    game.Debug("PlayingState found");
                    game.GameExtentsOnScreen = new Rectangle(match.X, match.Y, game.GameSize.Width, game.GameSize.Height);
                    return;
                }
                */

                Stack<BaseGameState> menuStates = new Stack<BaseGameState>();
                menuStates.Push(new StarState());
                
                menuStates.Push(new RareGemState());
                menuStates.Push(new MenuState());
                menuStates.Push(new ConfirmRestartState());
                menuStates.Push(new GameOverState());
                
                menuStates.Push(new PlayNowState());
                //menuStates.Push(new GameOverState()); //TESTING

                while (menuStates.Count > 0)
                {
                    BaseGameState state = menuStates.Pop();
                    //Look for different state
                    game.Debug("Looking for state: " + state.AssetName);
                    state.Init(game); // Manually init game reference
                    match = state.FindStateFromScreen(checkCount < DeepSearch);
                    if (match.Confident)
                    {
                        // Started playing
                        game.EventStack.Push(new GameEvent(EngineEventType.CHANGE_MENU, state));
                        game.Debug("MenuState found: " + state.AssetName);
                        
                        return;
                    }

                }

                
            }
            game.Debug("Nothing found");

            checkCount = checkCount % DeepSearch;

            previousTimestamp = DateTime.Now;
        }

        public override void Draw()
        {
            lock (game.GameScreen)
            {
                // Save copy of current screen for next time
                bmpPreviousGameScreen = game.GameScreen;
            }
        }

        
        
    }
}
