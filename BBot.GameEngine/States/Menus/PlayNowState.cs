using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    public class PlayNowState : BaseMenuState
    {

        public PlayNowState()
        {
            Name = "Start Game";

            gameScreen.AssetName =  "wholegame.playnow";
            gameScreen.MinimumConfidence =  200000;
       
            transitionClickOffset.X = 265;
            transitionClickOffset.Y = 365;

            transitionState = new PlayingState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            base.Update(cancelToken);
        }

    }
}
