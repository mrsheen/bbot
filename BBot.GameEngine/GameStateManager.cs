using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBot.GameEngine.States;
using System.Threading;

namespace BBot.GameEngine
{
    public class GameStateManager
    {
        public readonly Object StateManagerLOCK = new Object();
        private Stack<BaseGameState> states;
        public int NumberOfStates { get { return states.Count; } }
        private CancellationTokenSource ctsRunState;

        private BBotGameEngine game;
        private Thread RunThread;

        public GameStateManager(BBotGameEngine gameRef)
        {
            game = gameRef;
            states = new Stack<BaseGameState>();
            ctsRunState = new CancellationTokenSource();
        }

        public void Cleanup()
        {
            game.DebugAction(String.Format("Cleaning up {0} state{1}{2}",
                states.Count,
                states.Count > 1 ? "s" : "",
                (RunThread!= null && RunThread.ThreadState == ThreadState.Running) ?
                ", waiting for currently executing thread to finish" : ""));
            
            ctsRunState.Cancel();

            if (RunThread != null)
            {
                if (RunThread.ThreadState == ThreadState.Running)
                    RunThread.Join();

                RunThread = null;
            }

            while (states.Count > 0)
                states.Pop().Cleanup();
        }

        public void ChangeState(BaseGameState newState)
        {
            game.DebugAction(String.Format("Changing state from {0} to {1}", states.Count > 0 ? states.Peek().Name : "-none-", newState.Name));
            if (states.Count > 0)
                states.Pop().Cleanup();

            states.Push(newState);
            states.Peek().Init(game);
        }

        public void PushState(BaseGameState newState)
        {
            game.DebugAction(String.Format("Pushing state from {0} to {1}", states.Count > 0 ? states.Peek().Name : "-none-", newState.Name));
            if (states.Count > 0)
                states.Peek().Pause();

            states.Push(newState);
            states.Peek().Init(game);
        }

        public BaseGameState PopState()
        {
            BaseGameState state = null;
            if (states.Count > 0)
            {
                state = states.Pop();
                state.Cleanup();
            }

            if (states.Count > 0)
                states.Peek().Resume();

            return state;
        }

        public void Run()
        {
            if (ctsRunState != null && ctsRunState.IsCancellationRequested)
                return;

            if (Monitor.TryEnter(StateManagerLOCK))
            {
                try
                {
                    if (states.Count == 0)
                        return;

                    if (RunThread == null || RunThread.ThreadState == ThreadState.Stopped)
                    {
                        RunThread = new Thread(new ThreadStart(RunCurrentState));
                        RunThread.Name = String.Format("RunThread-{0}-{1}", states.Peek().Name, DateTime.Now);
                        return;
                    }

                    if (RunThread.IsAlive)
                        return;

                    try
                    {
                        if (RunThread.ThreadState == ThreadState.Unstarted)
                            RunThread.Start();
                    }
                    catch (Exception) { }
                }
                finally
                {
                    Monitor.Exit(StateManagerLOCK);
                }
            }


        }

        private void RunCurrentState()
        {
            if (ctsRunState == null)
                ctsRunState = new CancellationTokenSource();

            if (ctsRunState.IsCancellationRequested)
                return;

            states.Peek().Run(ctsRunState.Token);
        }
    }
}
