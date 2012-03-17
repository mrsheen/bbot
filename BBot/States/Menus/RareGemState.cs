using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public class RareGemState : BaseMenuState
    {


        public RareGemState()
        {
            AssetName = "wholegame.raregem";
            MinimumConfidence = 300000;

            transitionClickOffset.X = 210;
            transitionClickOffset.Y = 390;

            transitionState = new PlayNowState();
        }

        public override void Update()
        {
            findStates.Push(new MenuState());
            findStates.Push(new PlayNowState());
            base.Update();
        }
    }
}
