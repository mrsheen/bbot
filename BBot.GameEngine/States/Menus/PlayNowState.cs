using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.GameEngine.States
{
    public class PlayNowState : BaseMenuState
    {

        public PlayNowState()
        {
            AssetName = "wholegame.playnow";
            MinimumConfidence = 200000;
       
            transitionClickOffset.X = 265;
            transitionClickOffset.Y = 365;

            transitionState = new PlayingState();
        }

        public override void Update()
        {
            base.Update();
        }

    }
}
