namespace BBot
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.playButton = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.debugConsole = new System.Windows.Forms.RichTextBox();
            this.preview = new System.Windows.Forms.PictureBox();
            this.duration = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRestart = new System.Windows.Forms.Button();
            this.gameOverBlueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ckbDebug = new System.Windows.Forms.CheckBox();
            this.btnSnapshot = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.gameOverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starMedalBlueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.findGameScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.preview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.duration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // playButton
            // 
            this.playButton.ContextMenuStrip = this.contextMenuStrip1;
            this.playButton.Location = new System.Drawing.Point(12, 30);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(116, 42);
            this.playButton.TabIndex = 6;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // debugConsole
            // 
            this.debugConsole.Location = new System.Drawing.Point(141, 365);
            this.debugConsole.Name = "debugConsole";
            this.debugConsole.Size = new System.Drawing.Size(514, 347);
            this.debugConsole.TabIndex = 10;
            this.debugConsole.Text = "";
            // 
            // preview
            // 
            this.preview.Location = new System.Drawing.Point(141, 30);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(320, 320);
            this.preview.TabIndex = 14;
            this.preview.TabStop = false;
            // 
            // duration
            // 
            this.duration.Location = new System.Drawing.Point(12, 330);
            this.duration.Name = "duration";
            this.duration.Size = new System.Drawing.Size(116, 20);
            this.duration.TabIndex = 19;
            this.duration.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 314);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Duration";
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(12, 78);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(116, 42);
            this.btnRestart.TabIndex = 21;
            this.btnRestart.Text = "Restart";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // gameOverBlueToolStripMenuItem
            // 
            this.gameOverBlueToolStripMenuItem.Name = "gameOverBlueToolStripMenuItem";
            this.gameOverBlueToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // ckbDebug
            // 
            this.ckbDebug.AutoSize = true;
            this.ckbDebug.Location = new System.Drawing.Point(13, 173);
            this.ckbDebug.Name = "ckbDebug";
            this.ckbDebug.Size = new System.Drawing.Size(58, 17);
            this.ckbDebug.TabIndex = 22;
            this.ckbDebug.Text = "Debug";
            this.ckbDebug.UseVisualStyleBackColor = true;
            this.ckbDebug.CheckedChanged += new System.EventHandler(this.ckbDebug_CheckedChanged);
            // 
            // btnSnapshot
            // 
            this.btnSnapshot.Location = new System.Drawing.Point(13, 196);
            this.btnSnapshot.Name = "btnSnapshot";
            this.btnSnapshot.Size = new System.Drawing.Size(116, 42);
            this.btnSnapshot.TabIndex = 23;
            this.btnSnapshot.Text = "Snapshot";
            this.btnSnapshot.UseVisualStyleBackColor = true;
            this.btnSnapshot.Visible = false;
            this.btnSnapshot.Click += new System.EventHandler(this.btnSnapshot_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(468, 31);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(353, 319);
            this.pictureBox1.TabIndex = 24;
            this.pictureBox1.TabStop = false;
            // 
            // gameOverToolStripMenuItem
            // 
            this.gameOverToolStripMenuItem.Name = "gameOverToolStripMenuItem";
            this.gameOverToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // starMedalBlueToolStripMenuItem
            // 
            this.starMedalBlueToolStripMenuItem.Name = "starMedalBlueToolStripMenuItem";
            this.starMedalBlueToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findGameScreenToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(833, 24);
            this.menuStrip1.TabIndex = 25;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // findGameScreenToolStripMenuItem
            // 
            this.findGameScreenToolStripMenuItem.Name = "findGameScreenToolStripMenuItem";
            this.findGameScreenToolStripMenuItem.Size = new System.Drawing.Size(112, 20);
            this.findGameScreenToolStripMenuItem.Text = "Find game screen";
            this.findGameScreenToolStripMenuItem.Click += new System.EventHandler(this.findGameScreenToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 712);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnSnapshot);
            this.Controls.Add(this.ckbDebug);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.duration);
            this.Controls.Add(this.preview);
            this.Controls.Add(this.debugConsole);
            this.Controls.Add(this.playButton);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "BBot";
            ((System.ComponentModel.ISupportInitialize)(this.preview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.duration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.RichTextBox debugConsole;
        private System.Windows.Forms.PictureBox preview;
        private System.Windows.Forms.NumericUpDown duration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.CheckBox ckbDebug;
        private System.Windows.Forms.Button btnSnapshot;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gameOverBlueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameOverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starMedalBlueToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem findGameScreenToolStripMenuItem;
    }
}

