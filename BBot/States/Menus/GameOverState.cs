using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    
    public class GameOverState : BaseMenuState
    {
        public GameOverState()
        {
            AssetName = "wholegame.gameover";
            MinimumConfidence = 350000;
        }

        public override void Start()
        {
            base.Start();

            clickX = game.GameExtentsOnScreen.Value.X + 260;
            clickY = game.GameExtentsOnScreen.Value.Y + 315;
            transitionState = new RareGemState();
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
