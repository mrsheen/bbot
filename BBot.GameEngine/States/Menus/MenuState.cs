using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace BBot.GameEngine.States
{
    public class MenuState : BaseMenuState
    {
        
        public MenuState()
        {
            Name = "Game Paused Menu";

            gameScreen.AssetName =  "wholegame.menu";
            gameScreen.MinimumConfidence =  200000;

            transitionClickOffset.X = 340;
            transitionClickOffset.Y = 190;

            transitionState = new ConfirmRestartState();
        }

        public override void Update(CancellationToken cancelToken)
        {
            findStates.Push(new ConfirmRestartState());
            
            base.Update(cancelToken);
        }


    }
}
