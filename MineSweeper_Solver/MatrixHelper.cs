using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Solver
{
    public class MatrixHelper
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
                    if (p.cells[j, i] == -2 || p.cells[j, i] == -4 || p.cells[j, i] == -5) { tmp_m.cells[j, i] = -1; continue; }
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
}
