using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    public class StarState : BaseMenuState
    {


        public StarState()
        {
            Name = "Star Award";
            gameScreen.AssetName =  "wholegame.star";
            gameScreen.MinimumConfidence =  400000;
            
            transitionClickOffset.X = 210;
            transitionClickOffset.Y = 390;

            transitionState = new GameOverState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new MedalState());
            findStates.Push(new GameOverState());
            base.Update(cancelToken);
        }
    }
}
