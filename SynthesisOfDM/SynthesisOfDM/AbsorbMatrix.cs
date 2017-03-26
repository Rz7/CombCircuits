using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynthesisOfDM
{
    /*
     * Класс поглощения матрицы
     * Конструктор загружает данные исходной матрицы
     * GenMatrix выдает результат
     */

    class AbsorbMatrix
    {
        // Максимальный размер матрицы.
        int MAXSIZE = 1000;

        int j_counter = 0;
        int i_counter = 0;
        byte[][] s_mtx;

        public AbsorbMatrix(byte[][] src_matrix, int j_c, int i_c)
        {
            s_mtx = src_matrix;

            j_counter = j_c;
            i_counter = i_c;
        }

        public byte[][] GenMatrix(out int n_j_c, out int n_i_c)
        {
            byte[][] matrix;
            matrix = new byte[MAXSIZE][];
            for (int j = 0; j < MAXSIZE; ++j)
            {
                matrix[j] = new byte[MAXSIZE];
                for (int i = 0; i < MAXSIZE; ++i)
                    matrix[j][i] = 255;
            }

            for (int j = 0; j < j_counter - 1; ++j)
            {
                if (s_mtx[j][0] == 255)
                    continue;

                for (int jT = j + 1; jT < j_counter; ++jT)
                {
                    if (s_mtx[jT][0] == 255)
                        continue;

                    FAbsorbMatrix(ref s_mtx[j], ref s_mtx[jT], i_counter);
                }
            }

            n_j_c = 0;
            n_i_c = i_counter;

            for (int j = 0; j < j_counter; ++j)
            {
                if (s_mtx[j][0] == 255)
                    continue;

                for (int i = 0; i < i_counter; ++i)
                    matrix[n_j_c][i] = s_mtx[j][i];

                ++n_j_c;
            }

            return matrix;
        }

        void FAbsorbMatrix(ref byte[] array1, ref byte[] array2, int i_c)
        {
            for (int i = 0; i < i_c; ++i)
                if (array1[i] != 2 && array2[i] != 2 && array1[i] != array2[i])
                    return;

            int countH = 0;
            int countHT = 0;

            for (int i = 0; i < i_c; ++i)
            {
                if (array1[i] == 2)
                    ++countH;

                if (array2[i] == 2)
                    ++countHT;
            }

            if (countH == countHT)
                return;

            bool type_diff = true; // true - >; false - <;
            for (int i = 0; i < i_c; ++i)
            {
                if (array1[i] == array2[i])
                    continue;

                if (array1[i] < array2[i])
                    type_diff = false;
                else
                {
                    if (type_diff)
                    {
                        if (array1[i] < array2[i])
                            return;
                    }
                    else
                    {
                        if (array1[i] > array2[i])
                            return;
                    }
                }
            }

            for (int i = 0; i < i_c; ++i)
            {
                if (countH > countHT)
                    array2[i] = 255;
                else array1[i] = 255;
            }
        }
    }
}
