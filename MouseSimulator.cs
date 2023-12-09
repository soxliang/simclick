#define LEFT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace simclick
{
    public class MouseOperations
    {
        [Flags]
#if !LEFT
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            AbsoluteMoveLeftDown = 0x00008003,
            LeftUp = 0x00000004,
            AbsoluteMoveLeftUp = 0x00008005,
            Left = 0x00000006,
            AbsoluteMoveLeft = 0x00008007,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            AbsoluteMove = 0x00008001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            AbsoluteMoveRightDown = 0x00008009,
            RightUp = 0x00000010,
            AbsoluteMoveRightUp = 0x00008011,
            Right = 0x00000018,
            AbsoluteMoveRight = 0x00008019,
            Wheel = 0x00000800
        }
#else
        public enum MouseEventFlags
        {
            RightDown = 0x00000002,
            AbsoluteMoveRightDown = 0x00008003,
            RightUp = 0x00000004,
            AbsoluteMoveRightUp = 0x00008005,
            Right = 0x00000006,
            AbsoluteMoveRight = 0x00008007,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            AbsoluteMove = 0x00008001,
            Absolute = 0x00008000,
            LeftDown = 0x00000008,
            AbsoluteMoveLeftDown = 0x00008009,
            LeftUp = 0x00000010,
            AbsoluteMoveLeftUp = 0x00008011,
            Left = 0x00000018,
            AbsoluteMoveLeft = 0x00008019,
            Wheel = 0x00000800
        }
#endif
        //[DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        //public static void SetCursorPosition(int x, int y)
        //{
        //    SetCursorPos(x, y);
        //}

        //public static void SetCursorPosition(MousePoint point)
        //{
        //    SetCursorPos(point.X, point.Y);
        //}

        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        public static void MouseEvent(MouseEventFlags value, int dx = 0, int dy = 0, int w = 0)
        {
            //MousePoint position = GetCursorPosition();
            //if (dx >= 0)
            //{
            //position.X = dx;
            //}
            //if (dy >= 0)
            //{
            //position.Y = dy;
            //}

            mouse_event
                ((int)value,
                 dx,
                 dy,
                 w,
                 0)
                ;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }


}
