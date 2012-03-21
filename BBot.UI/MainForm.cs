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
using BBot.GameEngine;

namespace BBot.UI
{
    public partial class MainForm : Form
    {
        //GameEngine gameEngine;

        private System.Windows.Forms.Timer tUpdateDisplay = new System.Windows.Forms.Timer(); // Timer that performs the moves

        public MainForm()
        {
            InitializeComponent();
            GenerateContextOptions();

            // Set up the timer that performs the moves
            tUpdateDisplay.Tick += new EventHandler(UpdateDisplay_tick);
            tUpdateDisplay.Interval = 10; // Perform a move every N milliseconds
            tUpdateDisplay.Enabled = true;
            tUpdateDisplay.Stop();


            /*
            // Shift-Ctrl-Alt Escape will exit the play loop
            WIN32.RegisterHotKey(Handle, 100, WIN32.KeyModifiers.Control | WIN32.KeyModifiers.Shift | WIN32.KeyModifiers.Alt, Keys.Escape);

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            */
            // Put the window at top right
            //this.Location = new Point(Screen.PrimaryScreen.Bounds.Width + 20, 0);
            this.Location = new Point(20, 20);

            // Initially set the preview image
            //this.preview.Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("BBot.Assets.Instruction.bmp"));
            
            InitGameEngine(Screen.PrimaryScreen.Bounds); // Start manually

        }

        private void InitGameEngine(Rectangle screenBounds)
        {
            DebugMessage("Initializing game engine");
            //gameEngine = new GameEngine(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            /*gameEngine = new GameEngine(screenBounds);


            bool debugMode = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugMode"]);
            
#if DEBUG
            debugMode = true;
#endif

            if (gameEngine != null)
                gameEngine.DebugMode = debugMode;

            //System.IO.Directory.CreateDirectory(workingPath);

            gameEngine.DebugEvent += DebugMessage;
            gameEngine.findBitmapWorker.ImageSearchEvent += ImageSearch;

            */

            playButton.Enabled = true;
            playButton.Text = "Start";

            tUpdateDisplay.Start();

        }

        private void KillGameEngine()
        {
            tUpdateDisplay.Stop();

            //if (startGameThread != null)
            //{
            //    if (states.Count > 0)
            //        states.Peek().StopRequested = true;


            //    startGameThread.Join();
            //    startGameThread = null;
            //}
            /*
            if (gameEngine != null)
            {
                gameEngine.Cleanup();

                gameEngine.DebugEvent -= DebugMessage;
                gameEngine.findBitmapWorker.ImageSearchEvent -= ImageSearch;
            }

            gameEngine = null;
            */
            playButton.Enabled = true;
            playButton.Text = "Start";
        }

        private readonly object PlotDetailsLOCK = new Object();
        private DateTime imageSnapshotTimestamp = DateTime.Now;
        private void UpdateDisplay_tick(object sender, EventArgs e)
        {
            /*
            lock (PlotDetailsLOCK)
            {
                foreach (FindBitmapWorker.ImageSearchDetails details in detailsWaiting)
                {
                    plotValue(details);
                }

                detailsWaiting.Clear();


            }*/

            lock (DebugMessagesLOCK)
            {

                foreach (string debugMessage in messagesWaiting)
                {
                    UpdateDebug(debugMessage);

                }
                messagesWaiting.Clear();

            }

            if ((DateTime.Now - imageSnapshotTimestamp).Seconds < 1)
                return; // Only update image every 1 second

            Bitmap newImage = new Bitmap(1, 1);
            /*
            if (Monitor.TryEnter(gameEngine.PreviewScreenLOCK))
            {
                try
                {
                    if (gameEngine.PreviewScreen != null)
                        newImage = gameEngine.PreviewScreen.Clone(new Rectangle(0, 0, gameEngine.PreviewScreen.Width, gameEngine.PreviewScreen.Height), gameEngine.PreviewScreen.PixelFormat);
                }
                finally
                {
                    Monitor.Exit(gameEngine.PreviewScreenLOCK);
                }
            }*/

            if (newImage.Width != 1)
            { // resize and show

                // Prevent using images internal thumbnail
                newImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                newImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

                int newWidth = preview.Width;

                if (newImage.Width <= newWidth)
                {
                    newWidth = newImage.Width;
                }

                int newHeight = newImage.Height * newWidth / newImage.Width;
                if (newHeight > preview.Height)
                {
                    // Resize with height instead
                    newWidth = newImage.Width * preview.Height / newImage.Height;
                    newHeight = preview.Height;
                }

                Image resizedImage = newImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);

                preview.Image = resizedImage;

                imageSnapshotTimestamp = DateTime.Now;
                // Clear handle to original file so that we can overwrite it if necessary
                newImage.Dispose();
            }
        }

        private readonly object DebugMessagesLOCK = new Object();

        private List<string> messagesWaiting = new List<string>();
        private void DebugMessage(string debugMessage)
        {
            lock (DebugMessagesLOCK)
            {
                messagesWaiting.Add(debugMessage);
            }

        }

        private void UpdateDebug(string debugMessage)
        {
            if (debugConsole.InvokeRequired)
            {
                debugConsole.Invoke(new MethodInvoker(() => { UpdateDebug(debugMessage); }));

            }
            else
            {


                debugConsole.AppendText(String.Format("{0} - {1}", DateTime.Now, debugMessage) + Environment.NewLine);
                debugConsole.ScrollToCaret();

            }

        }
        /*
        private List<FindBitmapWorker.ImageSearchDetails> detailsWaiting = new List<FindBitmapWorker.ImageSearchDetails>();
        private void ImageSearch(FindBitmapWorker.ImageSearchDetails details)
        {
            lock (PlotDetailsLOCK)
            {
                detailsWaiting.Add(details);
            }
        }*/
        /*
        private Dictionary<FindBitmapWorker.ImageSearchDetailsType, int> counts = new Dictionary<FindBitmapWorker.ImageSearchDetailsType, int>();
        private void plotValue(FindBitmapWorker.ImageSearchDetails details)
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
                    if (details.type.Equals(FindBitmapWorker.ImageSearchDetailsType.MatchCertainty))
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
        */
        private int GetAdjustedValue(double currentValue)
        {
            return (int)(currentValue * pictureBox1.Height / 100D);
        }
        /*
        private Color GetColor(FindBitmapWorker.ImageSearchDetails details)
        {
            switch (details.type)
            {
                case FindBitmapWorker.ImageSearchDetailsType.CertaintyDelta:
                    return Color.Orange;

                case FindBitmapWorker.ImageSearchDetailsType.MatchCertainty:
                    return Color.LimeGreen;

                default:
                    return Color.Blue;
            }
        }
        */

        /*
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kill the system-wide hotkey on app exit
            WIN32.UnregisterHotKey(Handle, 100);
            KillGameEngine();
        }*/

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
            StartGame();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {/*
            if (gameEngine != null)
                KillGameEngine();
            */
            StartGame();
        }

        private Type selectedState;
        private void StartGame()
        {/*
            if (gameEngine == null)
                InitGameEngine(Screen.PrimaryScreen.Bounds);

            */
            if (selectedState != null)
            {/*
                try
                {
                    gameEngine.StateManager.PushState((BaseGameState)Activator.CreateInstance(selectedState));
                    gameEngine.EventStack.Push(new GameEvent(EngineEventType.FIND_STATE_HINT, null));
                }
                catch (ApplicationException)
                {

                }
                finally
                {
                    selectedState = null;
                }*/
            }

            /*gameEngine.Start();*/

            playButton.Enabled = false;
            playButton.Text = "-Press Escape to stop-";

        }

        public Dictionary<string, Type> states = new Dictionary<string, Type>();

        private void GenerateContextOptions()
        {/*
            states.Clear();
            states.Add("Restart", typeof(States.ConfirmRestartState));
            states.Add("Game Results", typeof(States.GameOverState));
            states.Add("Game Menu", typeof(States.MenuState));
            states.Add("Play Now", typeof(States.PlayNowState));
            states.Add("Rare Gem", typeof(States.RareGemState));
            states.Add("Star Award", typeof(States.StarState));
            states.Add("Medal", typeof(States.MedalState));


            foreach (KeyValuePair<String, Type> state in states)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = state.Key;
                item.Size = new System.Drawing.Size(159, 22);
                item.Name = String.Format("{0}ToolStripMenuItem", state.Value.Name);
                item.Click += new System.EventHandler(GenericToolStripMenuItem_Click);

                this.contextMenuStrip1.Items.Add(item);

            }*/

        }

        private void GenericToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            if (states.TryGetValue(item.Text, out selectedState))
                StartGame();
        }

        List<CaptureForm> captureForms = new List<CaptureForm>();
        private void findGameScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (gameEngine != null)
                KillGameEngine();
            */
            foreach (Screen screen in Screen.AllScreens)
            {
#if DEBUG
                if (screen.Primary) // DEBUG
                    continue;
#endif
                CaptureForm cf = new CaptureForm();
                cf.StartPosition = FormStartPosition.Manual;
                cf.Bounds = screen.Bounds;
                cf.WindowState = FormWindowState.Maximized;
                cf.FormClosed += new FormClosedEventHandler(CaptureForm_Closed);

                cf.Show();

                captureForms.Add(cf);

            }

        }

        private void CaptureForm_Closed(object sender, FormClosedEventArgs e)
        {
            CaptureForm cf = (CaptureForm)sender;

            if (cf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                Rectangle screenBounds = cf.Bounds;


                Rectangle searchArea;

                if (cf.GameBounds.HasValue)
                    searchArea = cf.GameBounds.Value;
                else if (cf.GameCoordinate.HasValue)
                    searchArea = new Rectangle(cf.GameCoordinate.Value, new Size(screenBounds.Width - cf.GameCoordinate.Value.X, screenBounds.Height - cf.GameCoordinate.Value.Y));
                else
                    searchArea = new Rectangle(0, 0, 0, 0); // nothing set


                foreach (CaptureForm form in captureForms)
                {
                    form.DialogResult = System.Windows.Forms.DialogResult.Abort;
                    form.Dispose();
                }

                captureForms.Clear();
                /*
                if (gameEngine != null)
                    return;



                InitGameEngine(screenBounds);
                gameEngine.UpdateSuggestedSearch(searchArea, true);
                gameEngine.CaptureArea();*/

            }
        }


    }
}
