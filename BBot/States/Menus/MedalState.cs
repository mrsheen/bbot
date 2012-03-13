using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    
    public class MedalState : BaseMenuState
    {
        public MedalState()
        {
            AssetName = "wholegame.medal";
            MinimumConfidence = 350000;

            transitionClickOffset.X = 270;
            transitionClickOffset.Y = 345;


            transitionState = new GameOverState();
        }

        public override void Update()
        {
            findStates.Push(new PlayNowState());
            findStates.Push(new MenuState());
            findStates.Push(new StarState());

            base.Update();
        }


    }


}
