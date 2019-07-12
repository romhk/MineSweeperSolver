using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Solver
{
    public class ConsoleHelper
    {
        // возвращает оффсет - для наглядности
        public static Point getPointFromInt(int x, int y)
        {
            return new Point() { X = x, Y = y };
        }

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
            s = '+' + new string('-', w - 2) + '+';
            offsetWrite(s, getPointFromInt(x, y), fg, bg);

            // body
            for (int j = 1; j < h - 1; j++)
            {
                s = '|' + new string(' ', w - 2) + '|';
                offsetWrite(s, getPointFromInt(x, y + j), fg, bg);

            }

            //footer
            s = '+' + new string('-', w - 2) + '+';
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
}
