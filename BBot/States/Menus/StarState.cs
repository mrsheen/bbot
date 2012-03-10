using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public partial class StarState : BaseMenuState
    {


        public StarState()
        {

            AssetName = "wholegame.star";
            MinimumConfidence = 400000;
            matchOffset = new Point(0, -2);
        }

        public override void Start()
        {
            base.Start();

            clickX = game.GameExtentsOnScreen.Value.X + 210;
            clickY = game.GameExtentsOnScreen.Value.Y + 390;
            transitionState = new GameOverState();
        }

        public override void Update()
        {
            findStates.Push(new GameOverState());
            base.Update();
        }
    }
}
