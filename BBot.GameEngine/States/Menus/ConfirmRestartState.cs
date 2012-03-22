using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{

    public class ConfirmRestartState : BaseMenuState
    {
        public ConfirmRestartState()
        {
            Name = "Confirm Restart";

            gameScreen.AssetName =  "wholegame.confirmrestart";
            gameScreen.MinimumConfidence =  300000;
            
            transitionClickOffset.X = 405;
            transitionClickOffset.Y = 260;


            transitionState = new PlayNowState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new PlayNowState());
            findStates.Push(new MenuState());

            base.Update(cancelToken);
        }


    }
}
