using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    public class UnknownState : BaseMenuState
    {
        public UnknownState()
        {
            Name = "Game Finder";

            gameScreen.AssetName =  "wholegame.background";
            gameScreen.MinimumConfidence = 1;
        }


        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new MedalState());
            findStates.Push(new StarState());
            findStates.Push(new RareGemState());
            findStates.Push(new MenuState());
            findStates.Push(new ConfirmRestartState());
            findStates.Push(new GameOverState());
            findStates.Push(new PlayNowState());

            base.Update(cancelToken);
        }
 
        
        
    }
}
