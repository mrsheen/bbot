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
            this.gameOverBlueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.gameOverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starMedalBlueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.findGameScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameState = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.preview)).BeginInit();
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
            // gameOverBlueToolStripMenuItem
            // 
            this.gameOverBlueToolStripMenuItem.Name = "gameOverBlueToolStripMenuItem";
            this.gameOverBlueToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(468, 31);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(353, 120);
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
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlDark;
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
            this.findGameScreenToolStripMenuItem.BackColor = System.Drawing.SystemColors.ControlLight;
            this.findGameScreenToolStripMenuItem.Name = "findGameScreenToolStripMenuItem";
            this.findGameScreenToolStripMenuItem.Size = new System.Drawing.Size(112, 20);
            this.findGameScreenToolStripMenuItem.Text = "Find game screen";
            this.findGameScreenToolStripMenuItem.Click += new System.EventHandler(this.findGameScreenToolStripMenuItem_Click);
            // 
            // gameState
            // 
            this.gameState.Location = new System.Drawing.Point(468, 157);
            this.gameState.Name = "gameState";
            this.gameState.Size = new System.Drawing.Size(353, 193);
            this.gameState.TabIndex = 26;
            this.gameState.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 782);
            this.Controls.Add(this.gameState);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.preview);
            this.Controls.Add(this.debugConsole);
            this.Controls.Add(this.playButton);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "BBot";
            ((System.ComponentModel.ISupportInitialize)(this.preview)).EndInit();
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
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gameOverBlueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameOverToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starMedalBlueToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem findGameScreenToolStripMenuItem;
        private System.Windows.Forms.RichTextBox gameState;
    }
}

