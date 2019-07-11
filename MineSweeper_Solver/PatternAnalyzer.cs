using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Solver
{

    public class PatternAnalyzer
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
            matrix GameField = Program.getMatrixFromMSGF(data);

            foreach (matrix p in Program.cachePattern)
            {
                findAndApplyPattern(ref GameField, p);
            }

            exportMatrixToMSGF(GameField, ref data);
        }

    }
}
