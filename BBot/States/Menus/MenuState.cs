using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public class MenuState : BaseMenuState
    {
        
        public MenuState()
        {
            AssetName = "wholegame.menu";
            MinimumConfidence = 200000;

        }

        public override void Start()
        {
            base.Start();


            clickX = game.GameExtents.Value.X + 340;
            clickY = game.GameExtents.Value.Y + 190;
            transitionState = new ConfirmRestartState();
        }

        public override void Update()
        {
            findStates.Push(new ConfirmRestartState());
            
            base.Update();
        }


    }
}
