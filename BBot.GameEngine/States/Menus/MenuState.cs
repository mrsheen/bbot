using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.GameEngine.States
{
    public class MenuState : BaseMenuState
    {
        
        public MenuState()
        {
            AssetName = "wholegame.menu";
            MinimumConfidence = 200000;

            transitionClickOffset.X = 340;
            transitionClickOffset.Y = 190;

            transitionState = new ConfirmRestartState();
        }

        public override void Update()
        {
            findStates.Push(new ConfirmRestartState());
            
            base.Update();
        }


    }
}
