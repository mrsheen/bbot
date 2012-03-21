using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.GameEngine.States
{

    public class ConfirmRestartState : BaseMenuState
    {
        public ConfirmRestartState()
        {

            AssetName = "wholegame.confirmrestart";
            MinimumConfidence = 300000;
            
            transitionClickOffset.X = 405;
            transitionClickOffset.Y = 260;


            transitionState = new PlayNowState();
        }

        public override void Update()
        {
            findStates.Push(new PlayNowState());
            findStates.Push(new MenuState());

            base.Update();
        }


    }
}
