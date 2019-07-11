using System;
using System.Collections.Generic;
using UIAutomationClient;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace MineSweeper_Solver
{
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        static CUIAutomation _automation = new CUIAutomation();

        static string WindowCaption = "Сапер";
        static IntPtr MineSweeperHWND;
        static IUIAutomationElement appElement;
        static int GameField_X = 2;
        static int GameField_Y = 2;

        // PatternBank
        static string[][] stringsPatternBank = new string[][] {
            new string[] {
                "   ",
                "121",
                "M#M",
            },

            new string[] {
                "    ",
                "1221",
                "#MM#",
            },

            new string[] {
                "   ",
                " 1 ",
                "  M",
            },

            new string[] {
                "   ",
                " 2 ",
                " MM",
            },

            new string[] {
                "   ",
                " 3 ",
                "MMM",
            },

            new string[] {
                "   ",
                " 3М",
                " MM",
            },

            new string[] {
                "    ",
                " 12 ",
                " ##M",
            },

            new string[] {
                "   ",
                " 4M",
                "MMM",
            },

            new string[] {
                "  M",
                " 5M",
                "MMM",
            },

            new string[] {
                " MM",
                " 6M",
                "MMM",
            },

            new string[] {
                "MMM",
                " 7M",
                "MMM",
            },

            new string[] {
                "     ",
                " 112 ",
                " #M#M",
            },

            new string[] {
                "    ",
                " 12 ",
                " ##M",
            },

        };
        public static List<matrix> cachePattern = new List<matrix>();
        public static List<matrix> cachePatternMask = new List<matrix>();

        public static bool madePatternCache = false;



        public struct matrix
        {
            public int cols, rows;
            public int[,] cells;
        }

        public static class MatrixHelper
        {
            // инициализируем матрицу с значениями
            public static void initMatrixWithValues(ref matrix m, int v)
            {
                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        m.cells[j, i] = v;
                    }
                }
            }

            // инициализируем матрицу с нолями
            public static void initMatrixWithZeroes(ref matrix m)
            {
                initMatrixWithValues(ref m, 0);
            }

            // поворот матрицы
            public static matrix rotateMatrix(matrix m)
            {
                matrix tmp_m = new matrix
                {
                    cols = m.rows,
                    rows = m.cols,
                    cells = new int[m.cols, m.rows],
                };

                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        tmp_m.cells[i, j] = m.cells[m.rows - j - 1, i];
                    }
                }

                return tmp_m;
            }

            public static matrix mirrorMatrixHorizontal(matrix m)
            {
                matrix tmp_m = getNoInitMatrixFrom(m);

                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        tmp_m.cells[j, i] = m.cells[j, m.cols - i - 1];
                    }
                }

                return tmp_m;
            }

            public static matrix mirrorMatrixVertical(matrix m)
            {
                matrix tmp_m = getNoInitMatrixFrom(m);

                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        tmp_m.cells[j, i] = m.cells[m.rows - j - 1, i];
                    }
                }

                return tmp_m;
            }

            // получаем такую же матрицу по размерам, но ячейки в cells не проинициализированы
            public static matrix getNoInitMatrixFrom(matrix m)
            {
                return new matrix
                {
                    cols = m.cols,
                    rows = m.rows,
                    cells = new int[m.rows, m.cols],
                };
            }

            // получаем срез (часть) матрицы
            public static matrix getMatrixSlice(matrix m, int x, int y, int w, int h)
            {
                matrix tmp_m = new matrix
                {
                    cols = w,
                    rows = h,
                    cells = new int[h, w],
                };

                for (int j = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++)
                    {
                        int c_x = x + i;
                        int c_y = y + j;
                        if (c_x < 0 || c_x >= m.cols) { tmp_m.cells[j, i] = 0; continue; }
                        if (c_y < 0 || c_y >= m.rows) { tmp_m.cells[j, i] = 0; continue; }
                        tmp_m.cells[j, i] = m.cells[c_y, c_x];
                    }
                }

                return tmp_m;
            }

            // получаем маску матрицы
            public static matrix getPatternMask(matrix p)
            {
                matrix tmp_m = getNoInitMatrixFrom(p);

                for (int j = 0; j < p.rows; j++)
                {
                    for (int i = 0; i < p.cols; i++)
                    {
                        if (p.cells[j, i] == -2) { tmp_m.cells[j, i] = -1; continue; }
                        tmp_m.cells[j, i] = p.cells[j, i];
                    }
                }

                return tmp_m;
            }

            // проверка на равенство матриц
            public static bool isEqualMatrix(matrix m1, matrix m2)
            {
                if (isEqualMatrixSizes(m1, m2))
                {
                    for (int j = 0; j < m1.rows; j++)
                    {
                        for (int i = 0; i < m1.cols; i++)
                        {
                            if (m1.cells[j, i] != m2.cells[j, i]) { return false; }
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // проверка на соответствие размеров матриц
            public static bool isEqualMatrixSizes(matrix m1, matrix m2)
            {
                if (m1.rows != m2.rows) { return false; }
                if (m1.cols != m2.cols) { return false; }

                return true;
            }

            public static bool isZeroMatrix(matrix m)
            {
                matrix zero = getNoInitMatrixFrom(m);
                initMatrixWithZeroes(ref zero);

                return isEqualMatrix(m, zero);
            }

        }

        public static class ConsoleHelper
        {
            // красивости
            public static void offsetWrite(string s, Point position, System.ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black)
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
                Console.SetCursorPosition(position.X, position.Y);
                Console.Write(s);
                Console.ResetColor();
            }

            // рисуем простую рамку
            public static void drawBorderRECT(Rectangle rect, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black)
            {
                int
                    x = rect.X,
                    y = rect.Y,
                    w = rect.Width,
                    h = rect.Height;

                string s;

                // head
                s = '|' + new string('-', w - 2) + '|';
                offsetWrite(s, getPointFromInt(x, y), fg, bg);

                // body
                for (int j = 1; j < h - 1; j++)
                {
                    s = '|' + new string(' ', w - 2) + '|';
                    offsetWrite(s, getPointFromInt(x, y + j), fg, bg);

                }

                //footer
                s = '|' + new string('-', w - 2) + '|';
                offsetWrite(s, getPointFromInt(x, y + h - 1), fg, bg);
            }

            public static void drawBorder(int x, int y, int w, int h, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black)
            {
                drawBorderRECT(new Rectangle { X = x, Y = y, Width = w, Height = h }, fg, bg);
            }

            // отрисовываем матрицу на экране - это тест функция
            public static void drawMatrix(matrix m, Point position)
            {
                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        Console.SetCursorPosition(position.X + i, position.Y + j);

                        int state = m.cells[j, i];
                        if (state == 0) { continue; }
                        if (state == -1) { Console.Write("#"); continue; }
                        if (state == -2) { Console.Write("M"); continue; }
                        if (state == -3) { Console.Write("@"); continue; }
                        Console.Write(state.ToString());
                    }
                }
            }
        }

        public static class MouseEventHelper
        {
            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out Point lpPoint);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            static extern bool SetCursorPos(int x, int y);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern void mouse_event(MouseEvent dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


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

            public static void sendMouseEventArr(MouseEvent[] MouseEventArr, Point coordinates, int sleep = 2)
            {
                foreach (MouseEvent _me in MouseEventArr)
                {
                    if (!IsDialogWindow())
                    {
                        SetActiveWindow(MineSweeperHWND);
                        SetForegroundWindow(MineSweeperHWND);

                        SetCursorPos(coordinates.X, coordinates.Y);
                        mouse_event(_me, coordinates.X, coordinates.Y, 0, 0);

                        // DEBUG
                        //Point p;
                        //GetCursorPos(out p);
                        //if (x != p.X || y != p.Y) { Console.WriteLine($"CURSOR SET: {x}x{y}    GET: {p.X}x{p.Y}"); }

                        System.Threading.Thread.Sleep(sleep);
                    }
                }
            }
        };

        // возвращает оффсет - для наглядности
        public static Point getPointFromInt(int x, int y)
        {
            return new Point() { X = x, Y = y };
        }

        public static void sendMouseEventsFromMSGF(MSGF_Data data)
        {
            for (int i = 0; i < data.ColsCount; i++)
            {
                for (int j = 0; j < data.RowsCount; j++)
                {
                    if (data.cells[i, j].sendMouseEvent)
                    {
                        tagRECT r = data.cells[i, j].elem.CurrentBoundingRectangle;
                        Point pCoordinates = new Point() { X = (int)((r.left + r.right) / 2), Y = (int)((r.top + r.bottom) / 2) };

                        MouseEventHelper.sendMouseEventArr(data.cells[i, j].mouseEvent, pCoordinates);

                    }
                }
            }
        }

        public struct MSGF_Cell
        {
            public int state;
            public IUIAutomationElement elem;
            public bool sendMouseEvent;
            public MouseEventHelper.MouseEvent[] mouseEvent;
        }

        public struct MSGF_Data
        {
            public MSGF_Cell[,] cells;
            public int RowsCount;
            public int ColsCount;
            public int TimeSecs;
            public int RemainingMines;
            public int Xcells;
        }

        public static bool isEqual_MSGF_Cell(MSGF_Cell c1, MSGF_Cell c2)
        {
            if (c1.state != c2.state) { return false; }
            if (c1.sendMouseEvent != c2.sendMouseEvent) { return false; }

            return true;
        }

        public static bool isEqual_MSGF_Data(MSGF_Data d1, MSGF_Data d2)
        {
            if (d1.ColsCount != d2.ColsCount) { return false; }
            if (d1.RowsCount != d2.RowsCount) { return false; }

            for (int i = 0; i < d1.ColsCount; i++)
            {
                for (int j = 0; j < d1.RowsCount; j++)
                {
                    if (!isEqual_MSGF_Cell(d1.cells[i, j], d2.cells[i, j])) { return false; }
                }
            }

            return true;
        }

        public static MSGF_Cell copyMSGF_Cell(MSGF_Cell c)
        {
            MSGF_Cell tmp_c = new MSGF_Cell
            {
                state = c.state,
                elem = c.elem,
                mouseEvent = c.mouseEvent,
                sendMouseEvent = c.sendMouseEvent
            };

            return tmp_c;

        }

        public static MSGF_Data copy_MSGF_Data(MSGF_Data d)
        {
            MSGF_Data tmp_d = new MSGF_Data
            {
                ColsCount = d.ColsCount,
                RowsCount = d.RowsCount,

                cells = new MSGF_Cell[d.ColsCount, d.RowsCount],
            };

            for (int i = 0; i < d.ColsCount; i++)
            {
                for (int j = 0; j < d.RowsCount; j++)
                {
                    tmp_d.cells[i, j] = copyMSGF_Cell(d.cells[i, j]);
                }
            }

            return tmp_d;
        }

        public struct cell_coordinate
        {
            public int x;
            public int y;
        }

        public struct analyze_matrix
        {
            public cell_coordinate[] coord_closed;
            public cell_coordinate[] coord_mine_checked;
            public cell_coordinate[] coord_mine_to_check;
            public int count_closed;
            public int count_mine_checked;
            public int count_mine_to_check;
        }

        static public MSGF_Data ParseMineSweeperGameField()
        {
            MSGF_Data result = new MSGF_Data();
            result.Xcells = 0;

            appElement = _automation.ElementFromHandle(MineSweeperHWND);

            var array_rows = appElement.FindAll(TreeScope.TreeScope_Children, _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_CustomControlTypeId));
            if (array_rows.Length > 1)
            {
                result.RowsCount = array_rows.Length - 1;

                /*var elem_texts = array_rows.GetElement(0).FindAll(TreeScope.TreeScope_Children, _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_TextControlTypeId));
                if (elem_texts.Length == 2)
                {
                    result.TimeSecs = Convert.ToInt32(elem_texts.GetElement(0).GetCurrentPropertyValue(UIA_PropertyIds.UIA_ValueValuePropertyId));
                    result.RemainingMines = Convert.ToInt32(elem_texts.GetElement(1).GetCurrentPropertyValue(UIA_PropertyIds.UIA_ValueValuePropertyId));
                }*/

                // Ищем ячейки
                for (int j = 1; j < array_rows.Length; j++)
                {
                    var elem_row = array_rows.GetElement(j);
                    var array_cells = elem_row.FindAll(TreeScope.TreeScope_Children, _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_ButtonControlTypeId));

                    if (j == 1)
                    {
                        result.ColsCount = array_cells.Length;
                        result.cells = new MSGF_Cell[result.ColsCount, result.RowsCount];
                    }

                    for (int i = 0; i < array_cells.Length; i++)
                    {

                        {
                            string nm = array_cells.GetElement(i).CurrentName;
                            result.cells[i, j - 1].elem = array_cells.GetElement(i); // Runtime Error Out of range


                            if (nm.Contains("помечена")) { result.cells[i, j - 1].state = -2; continue; }
                            if (nm.Contains("неоткрыта")) { result.cells[i, j - 1].state = -1; result.Xcells++; continue; }
                            if (nm.Contains("мин нет")) { result.cells[i, j - 1].state = 0; continue; }
                            if (nm.Contains("ячейки: 1")) { result.cells[i, j - 1].state = 1; continue; }
                            if (nm.Contains("ячейки: 2")) { result.cells[i, j - 1].state = 2; continue; }
                            if (nm.Contains("ячейки: 3")) { result.cells[i, j - 1].state = 3; continue; }
                            if (nm.Contains("ячейки: 4")) { result.cells[i, j - 1].state = 4; continue; }
                            if (nm.Contains("ячейки: 5")) { result.cells[i, j - 1].state = 5; continue; }
                            if (nm.Contains("ячейки: 6")) { result.cells[i, j - 1].state = 6; continue; }
                            if (nm.Contains("ячейки: 7")) { result.cells[i, j - 1].state = 7; continue; }
                            if (nm.Contains("ячейки: 8")) { result.cells[i, j - 1].state = 8; continue; }
                        }
                    }
                }
            }

            return result;
        }

        public static void ResultConsoleOutput(MSGF_Data data)
        {
            // ToDo: использовать фрейм буфер для перерисовки, чтобы не было мерцания
            // рисуем границы
            Console.ResetColor();
            Console.Clear();
            Console.WriteLine($"  Time: {data.TimeSecs}    Mines: {data.RemainingMines}  Xcells: {data.Xcells}");
            Console.SetCursorPosition(GameField_X, GameField_Y);
            Console.Write($"|{new string('-', data.ColsCount)}|");

            Console.SetCursorPosition(GameField_X, GameField_Y + data.RowsCount + 1);
            Console.Write($"|{new string('-', data.ColsCount)}|");

            for (int j = 0; j <= data.RowsCount - 1; j++)
            {
                Console.SetCursorPosition(GameField_X, GameField_Y + j + 1);
                Console.Write("|");

                Console.SetCursorPosition(GameField_X + data.ColsCount + 1, GameField_Y + j + 1);
                Console.Write("|");

                for (int i = 0; i <= data.ColsCount - 1; i++)
                {
                    if (data.cells[i, j].state == 0)
                    {
                        continue;
                    }
                    else if (data.cells[i, j].state == -1)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.SetCursorPosition(GameField_X + 1 + i, GameField_Y + 1 + j);
                        Console.Write("#");
                        Console.ResetColor();
                    }
                    else if (data.cells[i, j].state == -2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.SetCursorPosition(GameField_X + 1 + i, GameField_Y + 1 + j);
                        Console.Write("M");
                        Console.ResetColor();
                    }
                    else if (data.cells[i, j].state == -3)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.SetCursorPosition(GameField_X + 1 + i, GameField_Y + 1 + j);
                        Console.Write("@");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ResetColor();
                        if (data.cells[i, j].mouseEvent == MouseEventHelper.MIDDLE_CLICK) { Console.ForegroundColor = ConsoleColor.Magenta; }
                        Console.SetCursorPosition(GameField_X + 1 + i, GameField_Y + 1 + j);
                        Console.Write(data.cells[i, j].state.ToString());
                        Console.ResetColor();
                    }
                }
            }
        }

        public static bool IsGameStarted(MSGF_Data data)
        {
            bool result = false;

            for (int i = 0; i < data.ColsCount; i++)
            {
                for (int j = 0; j < data.RowsCount; j++)
                {
                    if (data.cells[i, j].state != -1)
                    {
                        result = true;
                        break;
                    }
                }
                if (result) break;
            }

            return result;
        }

        public static bool IsDialogWindow()
        {
            //Stopwatch diagTimer = new Stopwatch();
            //diagTimer.Start();

            bool result = false;

            var array_windows = appElement.FindAll(TreeScope.TreeScope_Children, _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_WindowControlTypeId));

            for (int i = 0; i < array_windows.Length; i++)
            {
                string s_name = array_windows.GetElement(i).CurrentName;

                if (s_name.Contains("Игра проиграна") || s_name.Contains("Игра выиграна") || s_name.Contains("Новая игра")) { result = true; }
            }

            //diagTimer.Stop();
            //Console.WriteLine("IGS {0}",diagTimeToString(diagTimer.Elapsed));
            return result;
        }

        public static string diagTimeToString(TimeSpan t)
        {
            return String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);
        }

        public static void Basic_AnalyzeMSGF(ref MSGF_Data data)
        {
            for (int i = 0; i < data.ColsCount; i++)
            {
                for (int j = 0; j < data.RowsCount; j++)
                {
                    // Инициализация матрицы
                    if (data.cells[i, j].state > 0)
                    {
                        analyze_matrix mtx;
                        mtx.coord_closed = new cell_coordinate[8];
                        mtx.coord_mine_checked = new cell_coordinate[8];
                        mtx.coord_mine_to_check = new cell_coordinate[8];
                        mtx.count_closed = 0;
                        mtx.count_mine_checked = 0;
                        mtx.count_mine_to_check = 0;

                        int mines_count = data.cells[i, j].state;

                        for (int c = -1; c <= 1; c++)
                        {
                            // пропускаем столбцы вне поля
                            if (((c + i) < 0) || ((c + i) == data.ColsCount)) { continue; }

                            for (int r = -1; r <= 1; r++)
                            {
                                // пропускаем строки вне поля
                                if (((r + j) < 0) || ((r + j) == data.RowsCount)) { continue; }

                                // пропускаем центральную ячейку
                                if (c == 0 && r == 0) { continue; }

                                // статус текущей ячейки (количество мин вокруг)
                                int _state = data.cells[i + c, j + r].state;

                                // если ячейка закрыта и не помечена
                                if (_state == -1)
                                {
                                    mtx.coord_closed[mtx.count_closed].x = c + i;
                                    mtx.coord_closed[mtx.count_closed].y = r + j;
                                    mtx.count_closed++;
                                }

                                // если ячейка помечена
                                if (_state == -2)
                                {
                                    mtx.coord_mine_checked[mtx.count_closed].x = c + i;
                                    mtx.coord_mine_checked[mtx.count_closed].y = r + j;
                                    mtx.count_mine_checked++;
                                }

                                // если ячейка помечена
                                if (_state == -3)
                                {
                                    mtx.coord_mine_to_check[mtx.count_closed].x = c + i;
                                    mtx.coord_mine_to_check[mtx.count_closed].y = r + j;
                                    mtx.count_mine_to_check++;
                                }

                            }
                        }


                        if (mines_count == mtx.count_closed + mtx.count_mine_checked + mtx.count_mine_to_check)
                        {
                            for (int n = 0; n < mtx.count_closed; n++)
                            {
                                data.cells[mtx.coord_closed[n].x, mtx.coord_closed[n].y].state = -3;
                                data.cells[mtx.coord_closed[n].x, mtx.coord_closed[n].y].sendMouseEvent = true;
                                data.cells[mtx.coord_closed[n].x, mtx.coord_closed[n].y].mouseEvent = MouseEventHelper.RIGHT_CLICK;
                            }
                        }
                        else
                        {
                            if (mines_count == mtx.count_mine_checked)
                            {
                                data.cells[i, j].sendMouseEvent = true;
                                data.cells[i, j].mouseEvent = MouseEventHelper.MIDDLE_CLICK;
                            }
                        }
                    }
                }
            }
        }

        public static matrix getMatrixFromMSGF(MSGF_Data msgf)
        {
            matrix tmp_m = new matrix
            {
                cols = msgf.ColsCount,
                rows = msgf.RowsCount,
                cells = new int[msgf.RowsCount, msgf.ColsCount],
            };

            for (int j = 0; j < msgf.RowsCount; j++)
            {
                for (int i = 0; i < msgf.ColsCount; i++)
                {
                    tmp_m.cells[j, i] = msgf.cells[i, j].state;
                }
            }

            return tmp_m;
        }

        public static class PatternaAalyzer
        {
            // сложение матрицы и паттерна
            static matrix sumMatrixAndPattern(matrix m, matrix p)
            {
                if (!MatrixHelper.isEqualMatrixSizes(m, p)) { throw new Exception("MatrixSizesEqualError"); }
                matrix tmp_m = MatrixHelper.getNoInitMatrixFrom(m);

                MatrixHelper.initMatrixWithValues(ref tmp_m, 9);

                var pm = MatrixHelper.getPatternMask(p);

                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        int v_m = m.cells[j, i];
                        int v_pm = pm.cells[j, i];
                        int v_p = p.cells[j, i];

                        if (v_m == v_pm && v_m == 0) { tmp_m.cells[j, i] = 0; continue; }
                        if (v_m == v_pm && (v_m > 0 && v_m < 9)) { tmp_m.cells[j, i] = v_m; continue; }
                        if (v_pm == 0 && (v_m > 0 && v_m < 9)) { tmp_m.cells[j, i] = 0; continue; }
                        if (v_pm == -1 && (v_m == -1 || v_m == -2 || v_m == -3)) { tmp_m.cells[j, i] = -1; continue; }

                        if (v_pm == -1 && (v_m > 0 && v_m < 9) && (v_p != -2 || v_m == -3)) { tmp_m.cells[j, i] = -1; continue; }

                        if (v_pm == 0 && (v_m == -1 || v_m == -2 || v_m == -3)) { tmp_m.cells[j, i] = v_m; continue; };


                    }
                }

                return tmp_m;
            }

            // получаем матрицу с минами или отметками их
            static matrix getMines(matrix m)
            {
                matrix tmp_m = MatrixHelper.getNoInitMatrixFrom(m);

                for (int j = 0; j < m.rows; j++)
                {
                    for (int i = 0; i < m.cols; i++)
                    {
                        int v_m = m.cells[j, i];

                        if (v_m == -2 || v_m == -3)
                        {
                            tmp_m.cells[j, i] = v_m;
                        }
                        else
                        {
                            tmp_m.cells[j, i] = 0;
                        }
                    }
                }

                return tmp_m;
            }

            // провепка на совпадение петтерна и части игрового поля
            // эту функцию нужно заменить на функцию  с возвращением матрицы: с нолями если ошибка применения паттерна и 
            static bool cmpMatrixAndPatternMask(matrix m, matrix p)
            {
                return MatrixHelper.isEqualMatrix(sumMatrixAndPattern(m, p), MatrixHelper.getPatternMask(p));
            }

            // проверка совпадения матрицы и петтерна. В случае успеха в result лежит матрица с указанием отметок мин
            static bool cmpMatrixAndPattern(matrix m, matrix p, ref matrix result)
            {
                result = MatrixHelper.getNoInitMatrixFrom(m);
                MatrixHelper.initMatrixWithZeroes(ref result);

                if (!MatrixHelper.isEqualMatrix(sumMatrixAndPattern(m, p), MatrixHelper.getPatternMask(p)))
                {
                    return false;
                }
                else
                {
                    matrix mines_m = getMines(m);
                    matrix mines_p = getMines(p);

                    for (int j = 0; j < mines_m.rows; j++)
                    {
                        for (int i = 0; i < mines_m.cols; i++)
                        {
                            int
                                v_mm = mines_m.cells[j, i],
                                v_mp = mines_p.cells[j, i];

                            if (v_mm == 0 && v_mp == 0) { continue; }
                            if ((v_mm == -2 || v_mm == -3) && v_mp == -2) { continue; }

                            if ((v_mm == -2 || v_mm == -3) && v_mp == 0) { return false; }

                            if (v_mm == 0 && v_mp == -2) { result.cells[j, i] = -3; continue; }
                        }
                    }

                    // если результатом является нулевая матрица, то вернем ложь, чтобы не обновлять игровое поле
                    return !MatrixHelper.isZeroMatrix(result);
                }
            }

            // применить результаты на матрицу
            static void applyResult(ref matrix m, int x, int y, matrix result)
            {
                for (int j = 0; j < result.rows; j++)
                {
                    for (int i = 0; i < result.cols; i++)
                    {
                        if (result.cells[j, i] == 0) { continue; }
                        if (x + i < 0 || x + i >= m.cols) { continue; }
                        if (y + j < 0 || y + j >= m.rows) { continue; }

                        m.cells[y + j, x + i] = result.cells[j, i];
                    }
                }
            }

            // прогоняем по игровому полю паттерн, при совпадение применяем изменения
            static void findAndApplyPattern(ref matrix m, matrix p)
            {
                for (int j = -1; j <= m.rows - p.rows + 1; j++)
                {
                    for (int i = -1; i <= m.cols - p.cols + 1; i++)
                    {
                        matrix sub_matrix = MatrixHelper.getMatrixSlice(m, i, j, p.cols, p.rows);

                        matrix buffer = new matrix();

                        if (cmpMatrixAndPattern(sub_matrix, p, ref buffer))
                        {
                            applyResult(ref m, i, j, buffer);
                        }


                    }

                }
            }

            // добавляем все возможные варианты паттерна (повороты и зеркалирование) в кеш (библиотеку)
            static void addToPatternCache(matrix p, ref List<matrix> list_pattern)
            {
                matrix m_a = p;
                matrix m_b = MatrixHelper.mirrorMatrixHorizontal(p);
                matrix m_c = MatrixHelper.mirrorMatrixVertical(p);

                for (int n = 0; n < 4; n++)
                {
                    list_pattern.Add(m_a);
                    list_pattern.Add(m_b);
                    list_pattern.Add(m_c);

                    p = MatrixHelper.rotateMatrix(m_a);
                    p = MatrixHelper.rotateMatrix(m_b);
                    p = MatrixHelper.rotateMatrix(m_c);
                }
            }

            //  проверям соответсвие массива строк формату паттерна
            static bool checkPatternStringArr(string[] patternStringArr)
            {
                if (patternStringArr.Length == 0) { return false; }
                if (patternStringArr[0].Length == 0) { return false; }

                int
                    p_rows = patternStringArr.Length,
                    p_cols = patternStringArr[0].Length;

                for (int n = 0; n < p_rows; n++)
                {
                    if (patternStringArr[n].Length != p_cols) { return false; }
                }

                return true;
            }

            // преобразуем массив строк в матрицу
            static matrix loadPatternFromStringArr(string[] patternStringArr)
            {
                matrix tmp_m;

                if (!checkPatternStringArr(patternStringArr)) { throw new Exception("patternStringArr_Wrong_format"); }

                int
                    _rows = patternStringArr.Length,
                    _cols = patternStringArr[0].Length;

                tmp_m = new matrix
                {
                    cols = _cols,
                    rows = _rows,
                };

                tmp_m.cells = new int[_rows, _cols];
                MatrixHelper.initMatrixWithZeroes(ref tmp_m);

                for (int j = 0; j < _rows; j++)
                {
                    for (int i = 0; i < _cols; i++)
                    {
                        string s = patternStringArr[j];

                        switch (patternStringArr[j][i])
                        {
                            case '1': tmp_m.cells[j, i] = 1; break;
                            case '2': tmp_m.cells[j, i] = 2; break;
                            case '3': tmp_m.cells[j, i] = 3; break;
                            case '4': tmp_m.cells[j, i] = 4; break;
                            case '5': tmp_m.cells[j, i] = 5; break;
                            case '6': tmp_m.cells[j, i] = 6; break;
                            case '7': tmp_m.cells[j, i] = 7; break;
                            case '8': tmp_m.cells[j, i] = 8; break;
                            case '#': tmp_m.cells[j, i] = -1; break;
                            case 'M': tmp_m.cells[j, i] = -2; break;
                            case '@': tmp_m.cells[j, i] = -3; break;
                        }
                    }
                }

                return tmp_m;
            }

            // загружаем паттерны в кэш (библиотеку) из банка 
            public static void loadPatternsFromBank(string[][] patternBank, ref List<matrix> list_pattern)
            {
                for (int n = 0; n < patternBank.Length; n++)
                {
                    addToPatternCache(loadPatternFromStringArr(patternBank[n]), ref list_pattern);
                }
            }

            public static void optimizePatternCache(ref List<matrix> list_pattern)
            {
                for (int n = list_pattern.Count - 1; n >= 0; n--)
                {
                    for (int m = n - 1; m >= 0; m--)
                    {
                        if (MatrixHelper.isEqualMatrix(list_pattern[n], list_pattern[m]))
                        {
                            list_pattern.RemoveAt(n);
                            break;
                        }
                    }
                }
            }

            static void exportMatrixToMSGF(matrix m, ref MSGF_Data msgf)
            {

                for (int j = 0; j < msgf.RowsCount; j++)
                {
                    for (int i = 0; i < msgf.ColsCount; i++)
                    {
                        int v_m = m.cells[j, i];
                        msgf.cells[i, j].state = v_m;
                        if (v_m == -3)
                        {
                            msgf.cells[i, j].sendMouseEvent = true;
                            msgf.cells[i, j].mouseEvent = MouseEventHelper.RIGHT_CLICK;
                        }
                    }
                }
            }

            public static void analyzeMSGF(ref MSGF_Data data)
            {
                matrix GameField = getMatrixFromMSGF(data);

                foreach (matrix p in cachePattern)
                {
                    findAndApplyPattern(ref GameField, p);
                }

                exportMatrixToMSGF(GameField, ref data);
            }
        }



        // **************************************
        // Entring point Main
        // **************************************
        static void Main(string[] args)
        {
            // ToDo: Поиск названеия окна на разных языках
            MineSweeperHWND = FindWindowByCaption(IntPtr.Zero, WindowCaption);

            // если окно игры не найдено, то выход из приложения
            if (MineSweeperHWND == IntPtr.Zero)
            {
                Console.WriteLine("Окно игры не найдено!");
                System.Threading.Thread.Sleep(3000);
                System.Environment.Exit(-1);
            }

            // загружаем паттерны в банк
            PatternaAalyzer.loadPatternsFromBank(stringsPatternBank, ref cachePattern);
            // оптимизация паттернов в банке (удаляет дубли)
            PatternaAalyzer.optimizePatternCache(ref cachePattern);

            Console.WriteLine($"Patterns: {cachePattern.Count}");

            while (true)
            {
                // Парсим поле
                var ms_data = ParseMineSweeperGameField();

                // проверка на наличие открытых ячеек
                if (IsGameStarted(ms_data))
                {
                    if (!IsDialogWindow())
                    {
                        // поиск и применение паттернов на поле
                        PatternaAalyzer.analyzeMSGF(ref ms_data);

                        // элементарный анализ
                        Basic_AnalyzeMSGF(ref ms_data);

                        // клики по ячейкам
                        sendMouseEventsFromMSGF(ms_data);
                    }
                    else
                    {
                        // если открыто диалоговое окно (начало игры, вы выиграли или проиграли)
                        Console.ResetColor();
                        Console.WriteLine("Открыто диалоговое окно");
                        System.Threading.Thread.Sleep(100);
                    }
                }
                else
                {
                    // если все ячейки закрыты
                    Console.ResetColor();
                    Console.WriteLine("Игра не начата");
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
    }
}
