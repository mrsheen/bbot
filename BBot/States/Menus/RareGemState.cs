using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public partial class RareGemState : BaseMenuState
    {


        public RareGemState()
        {
            AssetName = "wholegame.raregem";
            MinimumConfidence = 300000;
        }

        public override void Start()
        {
            base.Start();
            clickX = game.GameExtents.Value.X + 210;
            clickY = game.GameExtents.Value.Y + 390;
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
