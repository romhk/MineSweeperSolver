using System;
using System.Collections.Generic;
using UIAutomationClient;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace MineSweeper_Solver
{

    public struct matrix
    {
        public int cols, rows;
        public int[,] cells;
    }

    public struct MSGF_Cell
    {
        public int state;
        public Point clickArea;
        public IUIAutomationElement elem;
        public bool sendMouseEvent;
        public MouseEventHelper.CommonMouseEvent MouseEvent;
    }

    public struct MSGF_Data
    {
        public MSGF_Cell[,] cells;
        public int RowsCount;
        public int ColsCount;
        public int TimeSecs;
        public int RemainingMines;
    }



    public class ProgramFlags
    {
        public bool flagMainWindowFound = false;
        public bool flagFullDataParsed = false;
        public bool flagGameStarted = false;
        public bool flagOpenedDialogWindow = false;
        public bool flagUseNewMouseEvents = true;
        public bool flagAbortSendMouseEvent = false;
    }


    public class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        static CUIAutomation _automation = new CUIAutomation();

        static string WindowCaption = "Сапер";
        public static IntPtr MineSweeperHWND;
        static IUIAutomationElement appElement;
        static int GameField_X = 2;
        static int GameField_Y = 2;
        public static ProgramFlags pFlags = new ProgramFlags();

        // на время тестирования остается возможность переключиться на старый метод отправки кликов
        static bool useMouseEventNew = true;

        public static void SendMouseEventsFromMSGF(MSGF_Data data)
        {
            for (int i = 0; i < data.ColsCount; i++)
            {
                for (int j = 0; j < data.RowsCount; j++)
                {
                    if (data.cells[i, j].sendMouseEvent)
                    {
                        if (!pFlags.flagAbortSendMouseEvent)
                        {
                            MSGF_Cell cell = data.cells[i, j];

                            MouseEventHelper.sendMouseEventUI(cell.MouseEvent, MineSweeperHWND, cell.clickArea, pFlags.flagUseNewMouseEvents, ref pFlags.flagAbortSendMouseEvent);
                        }
                        else
                        {
                            return;
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
                MouseEvent = c.MouseEvent,
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

        public static bool ParseMineSweeperGameField(IntPtr window, ref MSGF_Data data, ref bool statusFlag)
        {
            int getCellStatusByName(string name)
            {
                if (name.Contains("вопросительным знаком")) { return -1; }
                if (name.Contains("помечена")) { return -2; }
                if (name.Contains("неоткрыта")) { return -1; }
                if (name.Contains("мин нет")) { return 0; }
                if (name.Contains("ячейки: 1")) { return 1; }
                if (name.Contains("ячейки: 2")) { return 2; }
                if (name.Contains("ячейки: 3")) { return 3; }
                if (name.Contains("ячейки: 4")) { return 4; }
                if (name.Contains("ячейки: 5")) { return 5; }
                if (name.Contains("ячейки: 6")) { return 6; }
                if (name.Contains("ячейки: 7")) { return 7; }
                if (name.Contains("ячейки: 8")) { return 8; }

                Console.WriteLine("ERR getCellStatusByName");
                return -100;
            }

            try
            {
                statusFlag = false;

                if (!statusFlag)
                {
                    data = new MSGF_Data();

                    appElement = _automation.ElementFromHandle(window);

                    var array_rows = appElement.FindAll(TreeScope.TreeScope_Children, _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_CustomControlTypeId));

                    if (array_rows.Length > 1)
                    {
                        data.RowsCount = array_rows.Length - 1;

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
                                data.ColsCount = array_cells.Length;
                                data.cells = new MSGF_Cell[data.ColsCount, data.RowsCount];
                            }

                            for (int i = 0; i < array_cells.Length; i++)
                            {

                                {
                                    IUIAutomationElement cell = array_cells.GetElement(i);
                                    string nm = cell.CurrentName;
                                    tagRECT R = cell.CurrentBoundingRectangle;

                                    data.cells[i, j - 1].elem = array_cells.GetElement(i); // Runtime Error Out of range
                                    data.cells[i, j - 1].clickArea = new Point((R.left + R.right) / 2, (R.top + R.bottom) / 2);

                                    data.cells[i, j - 1].state = getCellStatusByName(nm);
                                }
                            }
                        }
                    }

                    Console.WriteLine("Full Parsing");
                    statusFlag = true;
                    return true;
                }
                else
                {
                    for (int j = 0; j < data.RowsCount; j++)
                    {
                        for (int i = 0; i < data.ColsCount; i++)
                        {
                            if (data.cells[i, j].state == -1)
                            {
                            }
                        }
                    }
                    Console.Write("Minimal parsing");
                    return true;
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                Console.WriteLine("Parse COMException");
                statusFlag = false;
                return false;
            }
            return false;
        }

        public static void ResultConsoleOutput(MSGF_Data data)
        {
            // ToDo: использовать фрейм буфер для перерисовки, чтобы не было мерцания
            // рисуем границы
            Console.ResetColor();
            Console.Clear();
            Console.WriteLine($"  Time: {data.TimeSecs}    Mines: {data.RemainingMines}");
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
                        if (data.cells[i, j].MouseEvent == MouseEventHelper.CommonMouseEvent.MIDDLE_CLICK) { Console.ForegroundColor = ConsoleColor.Magenta; }
                        Console.SetCursorPosition(GameField_X + 1 + i, GameField_Y + 1 + j);
                        Console.Write(data.cells[i, j].state.ToString());
                        Console.ResetColor();
                    }
                }
            }
        }

        public static bool IsGameStarted(MSGF_Data data)
        {
            for (int i = 0; i < data.ColsCount; i++)
            {
                for (int j = 0; j < data.RowsCount; j++)
                {
                    if (data.cells[i, j].state != -1)
                    {
                        return true;
                    }
                }
            }

            Console.WriteLine($"IsGameStarted FALSE Data: {data.ColsCount}x{data.RowsCount}");
            return false;
        }

        public static bool IsDialogWindow()
        {
            try
            {
                var array_windows = appElement.FindAll(TreeScope.TreeScope_Children, _automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_WindowControlTypeId));

                for (int i = 0; i < array_windows.Length; i++)
                {
                    string s_name = array_windows.GetElement(i).CurrentName;

                    if (s_name.Contains("Игра проиграна") || s_name.Contains("Игра выиграна") || s_name.Contains("Новая игра")) { return true; }
                }
            }
            catch (Exception e)
            {
                return true;
            }

            return false;
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
                                data.cells[mtx.coord_closed[n].x, mtx.coord_closed[n].y].MouseEvent = MouseEventHelper.CommonMouseEvent.RIGHT_CLICK;
                            }
                        }
                        else
                        {
                            if (mines_count == mtx.count_mine_checked)
                            {
                                data.cells[i, j].sendMouseEvent = true;
                                data.cells[i, j].MouseEvent = MouseEventHelper.CommonMouseEvent.MIDDLE_CLICK;
                            }
                        }
                    }
                }
            }
        }

        public static bool findWindow(string caption, ref IntPtr wHwnd)
        {
            IntPtr tmpPtr = FindWindowByCaption(IntPtr.Zero, caption);
            if (tmpPtr != IntPtr.Zero)
            {
                wHwnd = tmpPtr;
                return true;
            }
            return false;
        }

        static void Main(string[] args)
        {
            // ToDo: Поиск названеия окна на разных языках

            // Генерирование кеша паттернов для анализатора
            PatternAnalyzer.initPatternAnalizer(quiet: false);

            MSGF_Data ms_data = new MSGF_Data();

            while (true)
            {
                if (findWindow(WindowCaption, ref MineSweeperHWND))
                {
                    if (ParseMineSweeperGameField(MineSweeperHWND, ref ms_data, ref pFlags.flagFullDataParsed))
                    {
                        // проверка на наличие открытых ячеек
                        if (IsGameStarted(ms_data))
                        {
                            if (!IsDialogWindow())
                            {
                                // поиск и применение паттернов на поле
                                PatternAnalyzer.analyzeMSGF(ref ms_data);

                                // элементарный анализ
                                Basic_AnalyzeMSGF(ref ms_data);

                                // клики по ячейкам
                                pFlags.flagAbortSendMouseEvent = false;
                                SendMouseEventsFromMSGF(ms_data);
                            }
                            else
                            {
                                // если открыто диалоговое окно (начало игры, вы выиграли или проиграли)
                                pFlags.flagFullDataParsed = false;
                                Console.ResetColor();
                                Console.WriteLine("Открыто диалоговое окно");
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            // если все ячейки закрыты
                            pFlags.flagFullDataParsed = false;
                            Console.ResetColor();
                            Console.WriteLine("Игра не начата");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                else
                {
                    pFlags.flagFullDataParsed = false;
                    Console.WriteLine("Не найдено окно игры");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}
