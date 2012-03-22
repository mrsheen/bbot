using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    public class RareGemState : BaseMenuState
    {


        public RareGemState()
        {
            Name = "Rare Gem";

            gameScreen.AssetName =  "wholegame.raregem";
            gameScreen.MinimumConfidence =  300000;

            transitionClickOffset.X = 210;
            transitionClickOffset.Y = 390;

            transitionState = new PlayNowState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new MenuState());
            findStates.Push(new PlayNowState());
            base.Update(cancelToken);
        }
    }
}
