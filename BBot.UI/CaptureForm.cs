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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BBot
{
    public partial class CaptureForm : Form
    {
        public Point? GameCoordinate {get;set;}
        public Rectangle? GameBounds {get; set;}

        public CaptureForm()
        {
            InitializeComponent();
        }

        private void CaptureForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (GameBounds.HasValue && Math.Abs(GameBounds.Value.X - e.X) > 50)
                return;
            this.GameCoordinate = e.Location;
            this.GameBounds = null;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CaptureForm_MouseDown(object sender, MouseEventArgs e)
        {
            this.GameBounds = new Rectangle(e.Location.X,e.Location.Y,0,0);
            //this.RaisePaintEvent(this, new PaintEventArgs(this.CreateGraphics(), this.RectangleToClient(new Rectangle())));
        }

        DateTime moveTimestamp = DateTime.Now;
        private void CaptureForm_MouseMove(object sender, MouseEventArgs e)
        {
            if ((DateTime.Now-moveTimestamp).Milliseconds < 25)
                return;

            if (!this.GameBounds.HasValue)
                return;

            Rectangle bounds = GameBounds.Value;

            bounds.Width = Math.Abs(bounds.X - e.X);
            bounds.Height = Math.Abs(bounds.Y - e.Y);

            // Swap x,y if top-left
            bounds.X = Math.Min(bounds.X, e.X);
            bounds.Y = Math.Min(bounds.Y, e.Y);

            this.GameBounds = bounds;

            bounds.X -= 100;
            bounds.Y -= 100;
            bounds.Width += 200;
            bounds.Height += 200;

            this.Invalidate(bounds);

        }

        private void CaptureForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (!GameBounds.HasValue)
                return;

            Rectangle bounds = GameBounds.Value;
            
            bounds.Width = Math.Abs(bounds.X - e.X);
            bounds.Height = Math.Abs(bounds.Y - e.Y);

            // Swap x,y if top-left
            bounds.X = Math.Min(bounds.X, e.X);
            bounds.Y = Math.Min(bounds.Y, e.Y);

            this.GameBounds = bounds;

            DialogResult = DialogResult.OK;
            Close();

        }

        private void CaptureForm_Paint(object sender, PaintEventArgs e)
        {
            if (!this.GameBounds.HasValue)
                return;

            using (Brush brush = new SolidBrush(Color.Red))
            {
                e.Graphics.FillRectangle(brush, this.GameBounds.Value);
            }
            moveTimestamp = DateTime.Now;
        }


    }
}
