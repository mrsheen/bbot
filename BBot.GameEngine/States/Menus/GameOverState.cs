using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    
    public class GameOverState : BaseMenuState
    {
        public GameOverState()
        {
            Name = "Game Results";

            gameScreen.AssetName =  "wholegame.gameover";
            gameScreen.MinimumConfidence =  350000;

            transitionClickOffset.X = 260;
            transitionClickOffset.Y = 315;


            transitionState = new RareGemState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new MedalState());
            findStates.Push(new PlayNowState());
            findStates.Push(new MenuState());
            findStates.Push(new StarState());

            base.Update(cancelToken);
        }


    }


}
