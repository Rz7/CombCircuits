using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SynthesisOfDM
{
    class MMatrixOpt
    {
        static int maxMASSVALUE = 1000;

        public bool dnf;        // форма преобразования

        bool sWT = false;       // была ли сортировка с транспонированием
        int j_counter = 0;      // количество строк
        int i_counter = 0;      // количество столбцов

        string out_smtx = "";   // Исходная матрица в текстовом виде
        int[][] out_matrix = new int[maxMASSVALUE][];
        int[][] result_mtx = new int[maxMASSVALUE][];

        public string result1; // Результат после суммы строк матрицы
        public string result2; // Результат после поглощения матрицы
        public string result3; // Результат после сортировки матрицы
        public string result4; // Результат после Т + сортировки матрицы + Т

        // Отобразить ли промежуточный результат?
        public bool bres1 = false;
        public bool bres2 = false;
        public bool bres3 = false;
        public bool bres4 = false;

        // Смена строк матрицы местами
        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public string StartProcess(string o_mtx)
        {
            out_smtx = o_mtx;

            if (out_smtx == "")
                return "";

            string full_result = "";

            GetOutMatrix();     // Преобразовать записанную в textBox1 матрицу в out_matrix[][]

            GenSummMatrix();    // Произвести сложение
            if (bres1)
                full_result += "После сложения:" + Environment.NewLine + GetStringMatrix(out_matrix, j_counter, i_counter);

            GenAbsorbMatrix();  // Произвести поглощение
            if (bres2)
            {
                full_result += Environment.NewLine + Environment.NewLine;
                full_result += "После поглощения:" + Environment.NewLine + GetStringMatrix(out_matrix, j_counter, i_counter);
            }

            GenSortMatrix();    // Произвести сортировку
            if (bres3)
            {
                full_result += Environment.NewLine + Environment.NewLine;
                full_result += "После сортировки:" + Environment.NewLine + GetStringMatrix(out_matrix, j_counter, i_counter);
            }

            /*
            
            Функция (Транспонирования + сортировки + Транспонирования) временно выключена.
            Ее тестирование отложено на неопределенный срок времени.
            
            GenSortWTMatrix();  // Произвести сортировку вместе с транспонированием
            if (bres4)
            {
                full_result += Environment.NewLine + Environment.NewLine;
                full_result += "После Т + сортировки + Т:" + Environment.NewLine + GetStringMatrix(out_matrix, j_counter, i_counter);
            }
            else
            {
                full_result = "Конечное преобразование:" + Environment.NewLine + result4;
            }
             
            */

            //MessageBox.Show(GetStringMatrix(out_matrix, j_counter, i_counter));

            // Удаление повторных строк
            DelEXJ(ref out_matrix, j_counter, i_counter);

            result4 = GetStringMatrix(out_matrix, j_counter, i_counter);
            full_result = result4;

            //return full_result;
            return o_mtx;
        }

        void DelEXJ(ref int[][] matrix, int j_c, int i_c)
        {
            //
            // Функция, удаляющая эквивалентные строки матрицы
            //

            for(int j1 = 0; j1 < j_c; ++j1)
            {
                if (matrix[j1][0] == -1)
                    continue;

                for(int j2 = 0; j2 < j_c - j1 - 1; ++j2)
                {
                    if (matrix[j1 + 1][0] == -1)
                        continue;

                    bool ex = true; // эквивалентна ли строка

                    for(int i = 0; i < i_c; ++i)
                        if(matrix[j1][i] != matrix[j1 + 1][i])
                        {
                            ex = false;
                            break;
                        }

                    if(ex)
                        matrix[j1][0] = -1;
                }
            }

            // Удалить все пустые строки
            DelAllEmptLine();
        }

        void Refresh()
        {
            out_matrix = new int[maxMASSVALUE][];
            result_mtx = new int[maxMASSVALUE][];

            for (int i = 0; i < maxMASSVALUE; ++i)
            {
                out_matrix[i] = new int[maxMASSVALUE];
                result_mtx[i] = new int[maxMASSVALUE];
            }

            j_counter = 0;
            i_counter = 0;
        }

        void GetOutMatrix()
        {
            Refresh();

            string getMatrix = out_smtx;
            int n_i_counter = 0;
            for (int i = 0; i < getMatrix.Length; ++i)
            {
                string gm = getMatrix[i].ToString();
                if (i > 0 && CanConvertToInt(getMatrix[i - 1].ToString()) && gm == "\r")
                {
                    if (n_i_counter > i_counter)
                        i_counter = n_i_counter;

                    n_i_counter = 0;
                    ++j_counter;
                    continue;
                }

                if (gm == "_")
                    gm = "2";

                if (CanConvertToInt(gm))
                    out_matrix[j_counter][n_i_counter++] = Convert.ToInt32(gm);
            }

            if (CanConvertToInt(getMatrix[getMatrix.Length - 1].ToString()))
                ++j_counter;
        }

        void GenSummMatrix()
        {
            int rm_j_counter = 0;
            int rm_i_counter = 0;

            for (int j = 0; j < j_counter - 1; ++j)
            {
                for (int jT = j + 1; jT < j_counter; ++jT)
                {
                    int counter_ord = 0;
                    for (int i = 0; i < i_counter; ++i)
                        if (out_matrix[j][i] != 2 && out_matrix[jT][i] != 2 && out_matrix[j][i] != out_matrix[jT][i])
                            ++counter_ord;

                    if (counter_ord != 1)
                        continue;

                    for (int i = 0; i < i_counter; ++i)
                    {
                        if (out_matrix[j][i] == 2)
                        {
                            result_mtx[rm_j_counter][rm_i_counter++] = out_matrix[jT][i];
                            continue;
                        }

                        if (out_matrix[jT][i] == 2)
                        {
                            result_mtx[rm_j_counter][rm_i_counter++] = out_matrix[j][i];
                            continue;
                        }

                        if (out_matrix[j][i] != out_matrix[jT][i])
                        {
                            result_mtx[rm_j_counter][rm_i_counter++] = 2;
                            continue;
                        }

                        result_mtx[rm_j_counter][rm_i_counter++] = out_matrix[j][i];
                    }

                    ++rm_j_counter;
                    rm_i_counter = 0;
                }
            }

            int oJc = j_counter; int counterRMTX = 0;
            j_counter += rm_j_counter;

            for (int j = oJc; j < j_counter; ++j, ++counterRMTX)
                for (int i = 0; i < i_counter; ++i)
                    out_matrix[j][i] = result_mtx[counterRMTX][i];

            DelAllEmptLine();
            result1 = GetStringMatrix(out_matrix, j_counter, i_counter) + Environment.NewLine;
        }

        void GenAbsorbMatrix()
        {
            for (int j = 0; j < j_counter - 1; ++j)
            {
                if (out_matrix[j][0] == -1)
                    continue;

                for (int jT = j + 1; jT < j_counter; ++jT)
                {
                    if (out_matrix[jT][0] == -1)
                        continue;

                    AbsorbMatrix(ref out_matrix[j], ref out_matrix[jT]);
                }
            }

            DelAllEmptLine();
            result2 = GetStringMatrix(out_matrix, j_counter, i_counter) + Environment.NewLine;
        }

        void GenSortMatrix()
        {
            // Сортировка матрицы //

            while (true)
            {
                bool updateMTX = false;

                for (int j = 0; j < j_counter; ++j)
                {
                    for (int jT = 0; jT < j_counter - j - 1; ++jT)
                    {
                        if (GetBiggerValue(out_matrix[jT], out_matrix[jT + 1], false))
                        {
                            updateMTX = true;
                            Swap(ref out_matrix[jT], ref out_matrix[jT + 1]);
                        }
                    }
                }

                if (!updateMTX)
                    break;
            }

            result3 = GetStringMatrix(out_matrix, j_counter, i_counter) + Environment.NewLine;
        }

        void GenSortWTMatrix()
        {
            for (int i = 0; i < i_counter; ++i)
                out_matrix[j_counter][i] = i;

            // Транспонирование, сортировка и опять транспонирование //
            TransposMatrix(out_matrix, j_counter + 1, i_counter);

            while (true)
            {
                bool updateMTX = false;

                for (int i = 0; i < i_counter; ++i)
                {
                    for (int iT = 0; iT < i_counter - i - 1; ++iT)
                    {
                        if (GetBiggerValue(out_matrix[iT], out_matrix[iT + 1], true))
                        {
                            updateMTX = true;
                            Swap(ref out_matrix[iT], ref out_matrix[iT + 1]);
                        }
                    }
                }

                if (!updateMTX)
                    break;
            }

            // Обратное транспонирование //
            TransposMatrix(out_matrix, i_counter, j_counter + 1);

            // В следующем GetStringMatrix() установим sWT = true (для отображения нумеров столбцов)
            // sWT = true; TODO: продумать реализацию сохранения нумеров столбцов (напр. отдельный массив)

            // Сохранение результата
            result4 = GetStringMatrix(out_matrix, j_counter, i_counter) + Environment.NewLine;
        }

        void AbsorbMatrix(ref int[] mtx1, ref int[] mtx2)
        {
            for (int i = 0; i < i_counter; ++i) if (mtx1[i] != 2 && mtx2[i] != 2 && mtx1[i] != mtx2[i])
                    return;

            int countHT = 0;
            int countH = 0;

            for (int i = 0; i < i_counter; ++i)
            {
                if (mtx1[i] == 2)
                    ++countH;

                if (mtx2[i] == 2)
                    ++countHT;
            }

            if (countH == countHT)
                return;

            bool type_diff = true; // true - >; false - <;
            for (int i = 0; i < i_counter; ++i)
            {
                if (mtx1[i] == mtx2[i])
                    continue;

                if (mtx1[i] < mtx2[i])
                    type_diff = false;
                else
                {
                    if (type_diff && mtx1[i] < mtx2[i])
                        return;

                    if (!type_diff && mtx1[i] > mtx2[i])
                        return;
                }
            }

            for (int i = 0; i < i_counter; ++i)
            {
                if (countH > countHT)
                    mtx2[i] = -1;
                else mtx1[i] = -1;
            }
        }

        void TransposMatrix(int[][] out_mtx, int j, int i)
        {
            // j - строки
            // i - столбцы

            int[][] rMtx = new int[i][];
            for (int iT = 0; iT < i; ++iT)
                rMtx[iT] = new int[j];

            for (int jT = 0; jT < j; ++jT)
                for (int iT = 0; iT < i; ++iT)
                    rMtx[iT][jT] = out_mtx[jT][iT];

            out_matrix = rMtx;
        }

        public string GetStringMatrix()
        {
            // Возвращает матрицу, записанную в out_matrix
            return GetStringMatrix(out_matrix, j_counter, i_counter);
        }

        string GetStringMatrix(int[][] matrix, int j_c, int i_c)
        {
            string sendMatrix = "";
            for (int j = 0; j < j_c; ++j)
            {
                for (int i = 0; i < i_c; ++i)
                    sendMatrix += matrix[j][i] == 2 ? "_" : Convert.ToString(matrix[j][i]);

                sendMatrix += Environment.NewLine;
            }

            if (sWT)
            {
                // Выводит дополнительную строку с нумерами столбцов
                for (int i = 0; i < i_c; ++i)
                    sendMatrix += Convert.ToString(matrix[j_c][i]);

                sendMatrix += Environment.NewLine;
            }
            return sendMatrix;
        }

        bool GetBiggerValue(int[] v1, int[] v2, bool T)
        {
            for (int i = 0; !T && i < i_counter || T && i <= j_counter; ++i)
            {
                if (v2[i] == v1[i])
                    continue;

                return (v2[i] < v1[i]);
            }
            return false;
        }

        void DelAllEmptLine()
        {
            while (j_counter != 0)
            {
                for (int j = 0; j < j_counter; ++j)
                    if (out_matrix[j][0] == -1)
                        out_matrix[j] = out_matrix[--j_counter];

                bool end = true;
                for (int j = 0; j < j_counter; ++j)
                    if (out_matrix[j][0] == -1)
                        end = false;

                if (end)
                    break;
            }
        }

        public void SaveMatrixToFile(int[][] matrix, string filename)
        {
            string sMatrix = GetStringMatrix(matrix, j_counter, i_counter);
            System.IO.File.AppendAllText(filename, sMatrix);
        }

        bool CanConvertToInt(string v)
        {
            try
            {
                int r = Convert.ToInt32(v);
                return true;
            }
            catch
            {
                if (v == "_")
                    return true;
                return false;
            }
        }

        public byte[][] GetMatrixInByte(int[][] matrix)
        {
            byte[][] mtx_b = new byte[maxMASSVALUE][];

            for(int j = 0; j < maxMASSVALUE; ++j)
            {
                mtx_b[j] = new byte[maxMASSVALUE];

                for (int i = 0; i < maxMASSVALUE; ++i)
                    mtx_b[j][i] = Convert.ToByte(matrix[j][i]);
            }

            return mtx_b;
        }
    }
}
