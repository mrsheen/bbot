using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{

    public partial class ConfirmRestartState : BaseMenuState
    {
        public ConfirmRestartState()
        {

            AssetName = "wholegame.confirmrestart";
            MinimumConfidence = 300000;
            matchOffset = new Point(1, 0);
        }

        public override void Start()
        {
            base.Start();


            clickX = game.GameExtentsOnScreen.Value.X + 405;
            clickY = game.GameExtentsOnScreen.Value.Y + 260;
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
