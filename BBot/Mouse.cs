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
using System.Text;
using System.Runtime.InteropServices;

namespace BBot
{
    // As we don't have the Java.awt.Robot class like those lucky Java people,
    // here's a utility class to simplify performing mouse actions
    public class Mouse
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);
        
        // Flags to represent mouse actions
        private const uint LEFTDOWN = 0x00000002;
        private const uint LEFTUP = 0x00000004;
        private const uint MIDDLEDOWN = 0x00000020;
        private const uint MIDDLEUP = 0x00000040;
        private const uint MOVE = 0x00000001;
        private const uint ABSOLUTE = 0x00008000;
        private const uint RIGHTDOWN = 0x00000008;
        private const uint RIGHTUP = 0x00000010;

        public static void MoveTo(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public static void Press()
        {
            mouse_event(LEFTDOWN, 0, 0, 0, 0);
        }

        public static void Release()
        {
            mouse_event(LEFTUP, 0, 0, 0, 0);
        }

        public static void Click()
        {
            mouse_event(LEFTDOWN, 0, 0, 0, 0);
            mouse_event(LEFTUP, 0, 0, 0, 0);
        }
    }


    #region MouseInput

    public class SendInputClass
    {
        //C# signature for "SendInput()"
        [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
        static extern uint SendInput(
            uint nInputs,
            INPUT[] pInputs,
            int cbSize);
        //C# signature for "GetMessageExtraInfo()"
        [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
        static extern IntPtr GetMessageExtraInfo();
        private enum InputType
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }
        [Flags()]
        private enum MOUSEEVENTF
        {
            MOVE = 0x0001,  // mouse move 
            LEFTDOWN = 0x0002,  // left button down
            LEFTUP = 0x0004,  // left button up
            RIGHTDOWN = 0x0008,  // right button down
            RIGHTUP = 0x0010,  // right button up
            MIDDLEDOWN = 0x0020,  // middle button down
            MIDDLEUP = 0x0040,  // middle button up
            XDOWN = 0x0080,  // x button down 
            XUP = 0x0100,  // x button down
            WHEEL = 0x0800,  // wheel button rolled
            VIRTUALDESK = 0x4000,  // map to entire virtual desktop
            ABSOLUTE = 0x8000,  // absolute move
        }
        [Flags()]
        private enum KEYEVENTF
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008,
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }
        // This function simulates a simple mouseclick at the current cursor position.
        public static uint Click(int x, int y)
        {
            int vscreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int vscreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int vscreenLeft = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X;
            int vscreenTop = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y;
            // Absolute input requires that input is in 'normalized' coords - with the entire
            // desktop being (0,0)...(65535,65536). Need to convert input x,y coords to this
            // first.
            //
            // In this normalized world, any pixel on the screen corresponds to a block of values
            // of normalized coords - eg. on a 1024x768 screen,
            // y pixel 0 corresponds to range 0 to 85.333,
            // y pixel 1 corresponds to range 85.333 to 170.666,
            // y pixel 2 correpsonds to range 170.666 to 256 - and so on.
            // Doing basic scaling math - (x-top)*65536/Width - gets us the start of the range.
            // However, because int math is used, this can end up being rounded into the wrong
            // pixel. For example, if we wanted pixel 1, we'd get 85.333, but that comes out as
            // 85 as an int, which falls into pixel 0's range - and that's where the pointer goes.
            // To avoid this, we add on half-a-"screen pixel"'s worth of normalized coords - to
            // push us into the middle of any given pixel's range - that's the 65536/(Width*2)
            // part of the formula. So now pixel 1 maps to 85+42 = 127 - which is comfortably
            // in the middle of that pixel's block.
            // The key ting here is that unlike points in coordinate geometry, pixels take up
            // space, so are often better treated like rectangles - and if you want to target
            // a particular pixel, target its rectangle's midpoint, not its edge.
            x = ((x - vscreenLeft) * 65536) / vscreenWidth + 65536 / (vscreenWidth * 2);
            y = ((y - vscreenTop) * 65536) / vscreenHeight + 65536 / (vscreenHeight * 2);
            INPUT input_set = new INPUT();
            input_set.mi.dx = x;
            input_set.mi.dy = y;
            input_set.mi.mouseData = 0;
            input_set.mi.dwFlags = (int)(MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE);
            INPUT input_down = new INPUT();
            input_down.mi.dx = 0;
            input_down.mi.dy = 0;
            input_down.mi.mouseData = 0;
            input_down.mi.dwFlags = (int)MOUSEEVENTF.LEFTDOWN;
            INPUT input_up = input_down;
            input_up.mi.dwFlags = (int)MOUSEEVENTF.LEFTUP;
            INPUT[] input = { input_set, input_down, input_up };
            return SendInput(3, input, Marshal.SizeOf(input_set));
        }

        // This function simulates a simple mouseclick at the current cursor position.
        public static uint Move(int x, int y)
        {
            int vscreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int vscreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int vscreenLeft = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X;
            int vscreenTop = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y;
            // Absolute input requires that input is in 'normalized' coords - with the entire
            // desktop being (0,0)...(65535,65536). Need to convert input x,y coords to this
            // first.
            //
            // In this normalized world, any pixel on the screen corresponds to a block of values
            // of normalized coords - eg. on a 1024x768 screen,
            // y pixel 0 corresponds to range 0 to 85.333,
            // y pixel 1 corresponds to range 85.333 to 170.666,
            // y pixel 2 correpsonds to range 170.666 to 256 - and so on.
            // Doing basic scaling math - (x-top)*65536/Width - gets us the start of the range.
            // However, because int math is used, this can end up being rounded into the wrong
            // pixel. For example, if we wanted pixel 1, we'd get 85.333, but that comes out as
            // 85 as an int, which falls into pixel 0's range - and that's where the pointer goes.
            // To avoid this, we add on half-a-"screen pixel"'s worth of normalized coords - to
            // push us into the middle of any given pixel's range - that's the 65536/(Width*2)
            // part of the formula. So now pixel 1 maps to 85+42 = 127 - which is comfortably
            // in the middle of that pixel's block.
            // The key ting here is that unlike points in coordinate geometry, pixels take up
            // space, so are often better treated like rectangles - and if you want to target
            // a particular pixel, target its rectangle's midpoint, not its edge.
            x = ((x - vscreenLeft) * 65536) / vscreenWidth + 65536 / (vscreenWidth * 2);
            y = ((y - vscreenTop) * 65536) / vscreenHeight + 65536 / (vscreenHeight * 2);
            INPUT input_set = new INPUT();
            input_set.mi.dx = x;
            input_set.mi.dy = y;
            input_set.mi.mouseData = 0;
            input_set.mi.dwFlags = (int)(MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE);
            
            INPUT[] input = { input_set};
            return SendInput(1, input, Marshal.SizeOf(input_set));
        }
    }

    #endregion
}
