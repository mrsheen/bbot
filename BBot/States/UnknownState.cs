using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace BBot.States
{
    public class UnknownState : BaseMenuState
    {
        public UnknownState()
        {
            AssetName = "wholegame.background";
            MinimumConfidence =1;
        }


        public override void Update()
        {
            findStates.Push(new MedalState());
            findStates.Push(new StarState());
            findStates.Push(new RareGemState());
            findStates.Push(new MenuState());
            findStates.Push(new ConfirmRestartState());
            findStates.Push(new GameOverState());
            findStates.Push(new PlayNowState());

            base.Update();
        }

        public override void Init(GameEngine gameRef)
        {
            base.Init(gameRef);

            // Manually override start flag
            bStarted = true;
        }
        
        
        
    }
}
