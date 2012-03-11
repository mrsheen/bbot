using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public partial class PlayNowState : BaseMenuState
    {

        public PlayNowState()
        {
            AssetName = "wholegame.playnow";
            MinimumConfidence = 200000;
            matchOffset = new Point(1, 0);

        }

        public override void Start()
        {
            base.Start();

            clickX = game.GameExtents.Value.X + 265;
            clickY = game.GameExtents.Value.Y + 365;
            transitionState = new PlayingState();
        }

        public override void Update()
        {
            base.Update();
        }

    }
}
