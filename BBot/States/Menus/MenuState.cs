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

        public override void Init(GameEngine gameRef)
        {
            base.Init(gameRef);

            clickX = game.GameExtentsOnScreen.Value.X + 340;
            clickY = game.GameExtentsOnScreen.Value.Y + 190;
            transitionState = new PlayNowState();
        }

        public override void Update()
        {
            findStates.Push(new ConfirmRestartState());
            
            base.Update();
        }


    }
}
