using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace SomethingDinosaurRelated
{
    class MouseControl
    {
        public static void MouseLeftDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }
        public static void MouseLeftUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        public static void DoMouseClick()
        {
            mouse_event(MouseEventFlag.LeftUp | MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void MouseScrollUpDown(int value)
        {
            value *= 30;
            mouse_event(MouseEventFlag.Wheel, 0, 0, (uint)value, UIntPtr.Zero);
        }

        public static void MouseScrollLeftRight(int value)
        {
            value *= 30;
            mouse_event(MouseEventFlag.HWheel, 0, 0, (uint)value, UIntPtr.Zero);
        }

        public static void DoDoubleClick()
        {
            DoMouseClick();
            Thread.Sleep(150);
            DoMouseClick();
        }

        public static void MouseRightDown()
        {
            mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void MouseRightUp()
        {
            mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
        }

        public static void DoRightClick()
        {
            mouse_event(MouseEventFlag.RightUp | MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void CtrlDown()
        {
            keybd_event(0x11, 0, 0, 0);
        }

        public static void CtrlUp()
        {
            keybd_event(0x11, 0, 0x0002, 0);
        }
        public static void minimise()
        {
            keybd_event(0x12, 0, 0, 0);// Alt
            keybd_event(0x20, 0, 0, 0);// Space
            Thread.Sleep(100);
            keybd_event(0x4E, 0, 0, 0);// N



            keybd_event(0x4E, 0, 0x0002, 0);
            keybd_event(0x20, 0, 0x0002, 0);
            keybd_event(0x12, 0, 0x0002, 0);

        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            HWheel = 0x1000,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);

            return lpPoint;
        }

    }
}
