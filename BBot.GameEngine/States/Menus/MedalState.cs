using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    
    public class MedalState : BaseMenuState
    {
        public MedalState()
        {
            Name = "Medal Award";

            gameScreen.AssetName =  "wholegame.medal";
            gameScreen.MinimumConfidence =  350000;

            transitionClickOffset.X = 270;
            transitionClickOffset.Y = 345;


            transitionState = new GameOverState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new PlayNowState());
            findStates.Push(new MenuState());
            findStates.Push(new StarState());

            base.Update(cancelToken);
        }


    }


}
