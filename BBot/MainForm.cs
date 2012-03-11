/*
The MIT License

Copyright (c) 2011 Mark Ashley Bell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/


using System.Windows.Forms;
using System;
using System.Drawing;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BBot.States;

namespace BBot
{
    public partial class MainForm : Form
    {
        GameEngine gameEngine;

        private System.Windows.Forms.Timer tUpdateDisplay = new System.Windows.Forms.Timer(); // Timer that performs the moves

        public MainForm()
        {
            InitializeComponent();
            GenerateContextOptions();

            // Set up the timer that performs the moves
            tUpdateDisplay.Tick += new EventHandler(UpdateDisplay_tick);
            tUpdateDisplay.Interval = 1000; // Perform a move every N milliseconds
            tUpdateDisplay.Enabled = true;
            tUpdateDisplay.Stop();



            // Shift-Ctrl-Alt Escape will exit the play loop
            WIN32.RegisterHotKey(Handle, 100, WIN32.KeyModifiers.Control | WIN32.KeyModifiers.Alt | WIN32.KeyModifiers.Shift, Keys.Escape);

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            // Put the window at top right
            //this.Location = new Point(Screen.PrimaryScreen.Bounds.Width + 20, 0);
            this.Location = new Point(20, 20);

            // Initially set the preview image
            //this.preview.Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("BBot.Assets.Instruction.bmp"));
            ckbDebug.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugMode"]);

            //InitGameEngine(); // Start manually

        }

        private void InitGameEngine()
        {
            DebugMessage("Starting game engine");
            //gameEngine = new GameEngine(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            gameEngine = new GameEngine(Screen.AllScreens[1].Bounds);

            //System.IO.Directory.CreateDirectory(workingPath);

            gameEngine.DebugEvent += new GameEngine.DebugDelegate(DebugMessage);
            FindBitmap.ImageSearchEvent += new FindBitmap.ImageSearchDelegate(ImageSearch);

            playButton.Enabled = true;

            tUpdateDisplay.Start();
        }

        private void KillGameEngine()
        {
            tUpdateDisplay.Stop();

            if (startGameThread != null)
            {
                startGameThread.Abort();
                startGameThread= null;
            }

            if (gameEngine != null)
                gameEngine.Cleanup();

            gameEngine = null;

            playButton.Enabled = false;
        }

        private void UpdateDisplay_tick(object sender, EventArgs e)
        {
            //lock (gameEngine.GameScreen)
            //{
            //    //preview.Image = gameEngine.GameScreen.Clone(new Rectangle(0, 0, gameEngine.GameScreen.Width, gameEngine.GameScreen.Height), gameEngine.GameScreen.PixelFormat);
            //}
        }

        private void DebugMessage(string debugMessage)
        {
            if (debugConsole.InvokeRequired)
            {
                debugConsole.Invoke(new MethodInvoker(() => { DebugMessage(debugMessage); }));

            }
            else
            {

                debugConsole.AppendText(String.Format("{0} - {1}", DateTime.Now, debugMessage) + Environment.NewLine);
                debugConsole.ScrollToCaret();
            }
        }

        private void ImageSearch(FindBitmap.ImageSearchDetails details)
        {
            plotValue(details);
        }

        private Dictionary<FindBitmap.ImageSearchDetailsType, int> counts = new Dictionary<FindBitmap.ImageSearchDetailsType, int>();
        private void plotValue(FindBitmap.ImageSearchDetails details)
        {
            if (pictureBox1.InvokeRequired)
            {
                pictureBox1.Invoke(new MethodInvoker(() => { plotValue(details); }));

            }
            else
            {
                if (!counts.ContainsKey(details.type))
                    counts.Add(details.type, 0);

                using (Graphics g = Graphics.FromHwnd(pictureBox1.Handle))
                {
                    SolidBrush brush = new SolidBrush(Color.White);
                    if (details.type.Equals(FindBitmap.ImageSearchDetailsType.MatchCertainty))
                        g.FillRectangle(brush, new Rectangle(counts[details.type] - 1, 0, 5, pictureBox1.Height));

                    brush.Color = GetColor(details);
                    g.FillRectangle(brush, new Rectangle(counts[details.type] - 1, pictureBox1.Height - GetAdjustedValue(details.currentValue) - 1, 2, 2));

                    brush.Color = Color.Red;
                    g.FillRectangle(brush, new Rectangle(counts[details.type] - 1, pictureBox1.Height - GetAdjustedValue(details.thresholdValue), 1, 1));

                }

                counts[details.type]++;

                if (counts[details.type] > pictureBox1.Width)
                {
                    counts[details.type] = 0;
                }
            }

        }

        private int GetAdjustedValue(double currentValue)
        {
            return (int)(currentValue * pictureBox1.Height / 100D);
        }

        private Color GetColor(FindBitmap.ImageSearchDetails details)
        {
            switch (details.type)
            {
                case FindBitmap.ImageSearchDetailsType.CertaintyDelta:
                    return Color.Orange;

                case FindBitmap.ImageSearchDetailsType.MatchCertainty:
                    return Color.LimeGreen;

                default:
                    return Color.Blue;
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kill the system-wide hotkey on app exit
            WIN32.UnregisterHotKey(Handle, 100);
            KillGameEngine();
        }

        // Set up hotkeys: we need one to be able to quit the loop because 
        // while the bot is running the mouse is hijacked
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            switch (m.Msg)
            {
                case WM_HOTKEY:
                    KillGameEngine();
                    break;
            }

            base.WndProc(ref m);
        }

        // Start the play loop
        private void playButton_Click(object sender, EventArgs e)
        {
            if (gameEngine == null)
                InitGameEngine();

        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            if (gameEngine == null)
                InitGameEngine();
            else
                KillGameEngine();

        }




        private void ckbDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbDebug.Checked)
            {
                if (gameEngine != null)
                    gameEngine.DebugMode = btnSnapshot.Visible = true;

                this.Height = 734;
            }
            else
            {
                if (gameEngine != null)
                    gameEngine.DebugMode = btnSnapshot.Visible = false;
                this.Height = 344;
            }
        }

        private void btnSnapshot_Click(object sender, EventArgs e)
        {

        }

        private Type selectedState;
        private void StartGame()
        {
            if (gameEngine == null)
            {
                InitGameEngine();

                if (selectedState != null)
                    try
                    {
                        gameEngine.StateManager.PushState((BaseGameState)Activator.CreateInstance(selectedState));
                    }
                    catch (ApplicationException)
                    {

                    }
                    finally
                    {
                        selectedState = null;
                    }
            }
            else
                KillGameEngine();

        }

        public Dictionary<string, Type> states = new Dictionary<string, Type>();

        private void GenerateContextOptions()
        {
            states.Clear();
            states.Add("Restart", typeof(States.ConfirmRestartState));
            states.Add( "Game Results", typeof(States.GameOverState));
            states.Add( "Game Menu", typeof(States.MenuState));
            states.Add("Play Now", typeof(States.PlayNowState) );
            states.Add("Rare Gem", typeof(States.RareGemState) );
            states.Add("Star Award", typeof(States.StarState) );
            
            foreach (KeyValuePair<String, Type> state in states)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = state.Key;
                item.Size = new System.Drawing.Size(159, 22);
                item.Name = String.Format("{0}ToolStripMenuItem", state.Value.Name);
                item.Click += new System.EventHandler(GenericToolStripMenuItem_Click);

                this.contextMenuStrip1.Items.Add(item);
                
            }

        }

        private Thread startGameThread;
        private void GenericToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                
                states.TryGetValue(item.Text, out selectedState);

                StartGame();
                //if (startGameThread == null || startGameThread.ThreadState == ThreadState.Stopped)
                //    startGameThread = new Thread(new ThreadStart(StartGame));

                //if (startGameThread.IsAlive)
                //    return;

                //startGameThread.Start();

            }
            catch (Exception)
            { }
        }


    }
}
