using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MineSweeper_Solver
{
    public class MouseEventHelper
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(MouseEvent dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        
        public enum MouseEvent
        {
            LEFT_DOWN = 0x02,
            LEFT_UP = 0x04,
            RIGHT_DOWN = 0x08,
            RIGHT_UP = 0x10,
            MIDDLE_DOWN = 0x20,
            MIDDLE_UP = 0x40,
        }

        public static MouseEvent[] LEFT_CLICK = new MouseEvent[] { MouseEvent.LEFT_DOWN, MouseEvent.LEFT_UP };
        public static MouseEvent[] LEFT_DBLCLICK = new MouseEvent[] { MouseEvent.LEFT_DOWN, MouseEvent.LEFT_UP, MouseEvent.LEFT_DOWN, MouseEvent.LEFT_UP };
        public static MouseEvent[] RIGHT_CLICK = new MouseEvent[] { MouseEvent.RIGHT_DOWN, MouseEvent.RIGHT_UP };
        public static MouseEvent[] MIDDLE_CLICK = new MouseEvent[] { MouseEvent.MIDDLE_DOWN, MouseEvent.MIDDLE_UP };

        public static void sendMouseEventArr(MouseEvent[] MouseEventArr, IntPtr hwnd,  Point coordinates, int sleep = 2)
        {
            // ToDo: убрать проверку IsDialogWindow (жесткая привязка к Program) Возможно заменить на проверку с GetActiveWindow
            foreach (MouseEvent _me in MouseEventArr)
            {
                if (!Program.IsDialogWindow())
                {
                    SetActiveWindow(hwnd);
                    SetForegroundWindow(hwnd);

                    SetCursorPos(coordinates.X, coordinates.Y);
                    mouse_event(_me, coordinates.X, coordinates.Y, 0, 0);

                    System.Threading.Thread.Sleep(sleep);
                }
            }
        }
    }
}
