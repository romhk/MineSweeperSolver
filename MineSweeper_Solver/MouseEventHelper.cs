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
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void mouse_event(hardMouseEvent dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);

        public enum CommonMouseEvent
        {
            LEFT_CLICK = 1,
            RIGHT_CLICK = 2,
            MIDDLE_CLICK = 3,
        }

        // ToDo: tune sleep timings
        public static void sendMouseEventUI(CommonMouseEvent MouseEvent, IntPtr hwnd, Point coordinates, bool mouseNewMethod, ref bool flagAbort, int sleep = 15)
        {
            if (mouseNewMethod)
            {
                sendMouseEventArrNew(MouseEvent, hwnd, coordinates, ref flagAbort, sleep);
            }
            else
            {
                sendMouseEventArrOld(MouseEvent, hwnd, coordinates, ref flagAbort, sleep);
            }
        }

        public static void releaseMouseButtons(bool mouseNewMethod)
        {
            if (mouseNewMethod)
            {

            }
            else
            {

            }
        }

        // определяем, движется ли курсор
        public static bool isMouseMove(int ms_delay)
        {
            Point posStart;
            GetCursorPos(out posStart);

            System.Threading.Thread.Sleep(ms_delay);

            Point posEnd;
            GetCursorPos(out posEnd);

            return !(posStart == posEnd);
        }

        // Активно ли окно
        public static bool isActiveWindow(IntPtr hwnd)
        {
            return (hwnd == GetForegroundWindow());
        }

        // ждем, пока окно не будет активно
        public static void waitActiveWindow(IntPtr hwnd, int msSleep)
        {
            while (!isActiveWindow(hwnd))
            {
                System.Threading.Thread.Sleep(msSleep);
            }
        }

        enum smInputMsg
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
        }

        private static int MakeLParam(int LoWord, int HiWord)
        {
            return ((HiWord << 16) | (LoWord & 0xffff));
        }

        public static void sendMouseEventArrNew(CommonMouseEvent MouseEvent, IntPtr hwnd, Point coordinates, ref bool flagAbort, int sleep = 10)
        {

            smInputMsg[] releaseMethods = new smInputMsg[] { smInputMsg.WM_LBUTTONUP, smInputMsg.WM_MBUTTONUP, smInputMsg.WM_RBUTTONUP };  

            smInputMsg[] MouseEventsArr = new smInputMsg[] { };

            switch (MouseEvent)
            {
                case CommonMouseEvent.LEFT_CLICK:
                    MouseEventsArr = new smInputMsg[] { smInputMsg.WM_LBUTTONDOWN, smInputMsg.WM_LBUTTONUP };
                    break;

                case CommonMouseEvent.RIGHT_CLICK:
                    MouseEventsArr = new smInputMsg[] { smInputMsg.WM_RBUTTONDOWN, smInputMsg.WM_RBUTTONUP };
                    break;

                case CommonMouseEvent.MIDDLE_CLICK:
                    MouseEventsArr = new smInputMsg[] { smInputMsg.WM_MBUTTONDOWN, smInputMsg.WM_MBUTTONUP };
                    break;
            }

            // ToDo: убрать проверку IsDialogWindow (жесткая привязка к Program) Возможно заменить на проверку с GetActiveWindow
            foreach (smInputMsg mouseEvent in MouseEventsArr)
            {
                if (!Program.IsDialogWindow() && !isMouseMove(20) && isActiveWindow(hwnd) && !flagAbort)
                {
                    SetCursorPos(coordinates.X, coordinates.Y);
                    if (!isMouseMove(5))
                    {
                        SendMessage(hwnd, (uint)mouseEvent, 0, MakeLParam(coordinates.X, coordinates.Y));

                        System.Threading.Thread.Sleep(sleep);
                    }
                    else
                    {
                        flagAbort = true;
                    }
                }
                else
                {
                    if (releaseMethods.Contains(mouseEvent)) {
                        SendMessage(hwnd, (uint)mouseEvent, 0, MakeLParam(coordinates.X, coordinates.Y));
                    }
                    flagAbort = true;
                    // System.Threading.Thread.Sleep(100);
                }
            }
        }

        enum hardMouseEvent
        {
            LEFT_DOWN = 0x02,
            LEFT_UP = 0x04,
            RIGHT_DOWN = 0x08,
            RIGHT_UP = 0x10,
            MIDDLE_DOWN = 0x20,
            MIDDLE_UP = 0x40,
        }

        public static void sendMouseEventArrOld(CommonMouseEvent MouseEvent, IntPtr hwnd, Point coordinates, ref bool flagAbort, int sleep = 2)
        {
            hardMouseEvent[] MouseEventArr = new hardMouseEvent[] { };

            hardMouseEvent[] releaseMethods = new hardMouseEvent[] { hardMouseEvent.LEFT_UP, hardMouseEvent.MIDDLE_UP, hardMouseEvent.RIGHT_UP };

            switch (MouseEvent)
            {
                case CommonMouseEvent.LEFT_CLICK:
                    MouseEventArr = new hardMouseEvent[] { hardMouseEvent.LEFT_DOWN, hardMouseEvent.LEFT_UP };
                    break;

                case CommonMouseEvent.RIGHT_CLICK:
                    MouseEventArr = new hardMouseEvent[] { hardMouseEvent.RIGHT_DOWN, hardMouseEvent.RIGHT_UP };
                    break;

                case CommonMouseEvent.MIDDLE_CLICK:
                    MouseEventArr = new hardMouseEvent[] { hardMouseEvent.MIDDLE_DOWN, hardMouseEvent.MIDDLE_UP };
                    break;
            }

            // ToDo: убрать проверку IsDialogWindow (жесткая привязка к Program) Возможно заменить на проверку с GetActiveWindow
            foreach (hardMouseEvent _me in MouseEventArr)
            {
                if (!Program.IsDialogWindow() && !isMouseMove(20) && isActiveWindow(hwnd) && !flagAbort)
                {
                    waitActiveWindow(hwnd, 2);
                    SetCursorPos(coordinates.X, coordinates.Y);
                    if (!isMouseMove(5))
                    {
                        mouse_event(_me, coordinates.X, coordinates.Y, 0, 0);

                        System.Threading.Thread.Sleep(sleep);
                    }
                }
                else
                {
                    if (releaseMethods.Contains(_me))
                    {
                        mouse_event(_me, coordinates.X, coordinates.Y, 0, 0);
                    }
                    flagAbort = true;
                }
            }
        }
    }
}
