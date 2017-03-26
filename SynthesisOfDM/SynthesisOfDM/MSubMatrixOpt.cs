using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SynthesisOfDM
{
    /*
     * Некоторые пояснения на счет byte значений:
     * 0 - логический 0
     * 1 - логическая 1
     * 2 - логическая _
     * 255 - полное отсутствие (для некоторых процессов программы, аналог -1)
     */

    //
    // Правила инициализации класса MSubMatrixOpt
    // 1) InitMatrix
    // 2) GenSubMatrix
    //

    class MSubMatrixOpt
    {
        // Максимальный размер массива
        int MAXSIZE = 1000;

        // Матрица, подлежащая делению
        byte[][] matrix = null;
        int j_counter = 0;
        int i_counter = 0;

                               // матрица, кол-во строк, кол-во стобцов
        public void InitMatrix(byte[][] mtx, int j_c, int i_c)
        {
            matrix = mtx;
            j_counter = j_c;
            i_counter = i_c;
        }

        public Submatrix GenSubMatrix(int mid)
        {
            // Функция создания подматриц
            byte[][] mainMatrix = null;              // Остаток от деления
            byte[][] sMtx1 = null, sMtx2 = null;     // Разделившиеся части; sMtx1 - матрица-строка, sMtx2 - остаток

            InitSubMatrix(ref mainMatrix, MAXSIZE, MAXSIZE);
            InitSubMatrix(ref sMtx1, MAXSIZE / 2, MAXSIZE / 2);
            InitSubMatrix(ref sMtx2, MAXSIZE / 2, MAXSIZE / 2);

            int counter_vlMI = 0;               // Счетчик номеров столбцов
            int[] valMI = new int[i_counter];   // Номера столбцов, учавствующих в преобразовании

            // Первоначально, необходимо найти столбец с максимальным числом одинаковых элементов (0 или 1)
            byte n0 = 255; // (аналог -1 для типа byte)
            int i_B = GetIBMatrix(out n0); // MessageBox.Show(i_B.ToString());

            if (i_B == -1)
            {
                MessageBox.Show("Матрица введена некорректно или не имеет смысла.");
                return null;
            }

            int[] vlMI_type = new int[i_counter];   // Массив столбцов, в котором указано, каких элементов в данном столбце больше (0 или 1)

            // Заполнение valMI номерами столбцов
            CounterStb(ref valMI, ref counter_vlMI, i_B, n0, ref vlMI_type);

            // Далее столбцы valMI необходимо сортировать по количеству одинаковых значений (от большего к меньшему)
            SortStb(ref valMI, counter_vlMI, i_B, n0, ref vlMI_type);

            // Далее выделяем из valMI одну строку и оставшуюся часть матрицы
            int[] j_anMtx = null;   // Массив с значениями, указывающими, какие строки главной матрицы остались нетронутыми (для вывода mainMatrix)
            GetsMtx12(valMI, ref sMtx1, ref sMtx2, counter_vlMI, i_B, n0, out j_anMtx, ref vlMI_type);

            // Далее выделяем нетронутые строки в матрицу mainMatrix. Информация о строках находится в массиве j_anMtx
            int mm_j_c = 0;

            string result = "";
            bool wasMainM = false;  // Истина, если остаточная матрица главной матрицы существует
            for (int j = 0; j < j_counter; ++j)
            {
                bool c = false;
                for (int j_anM = 0; j_anM < j_anMtx.Length; ++j_anM)
                    if (j == j_anMtx[j_anM])
                        c = true;

                if (c)  // Данная строка матрицы уже имеется в "оставшейся части матрицы".
                    continue;

                // Остаток матрицы существует
                wasMainM = true;

                for (int i = 0; i < i_counter; ++i)
                {
                    string r = matrix[j][i].ToString();

                    mainMatrix[mm_j_c][i] = matrix[j][i];

                    if (r == "2")
                        r = "_";

                    result += r;
                }

                // Увеличиваем количество строк в mainMatrix
                ++mm_j_c;

                result += Environment.NewLine;
            }

            if (!wasMainM)
            {
                // Такое возможно, это идеальное деление. Остатка нет, матрица поделилась на 2 части.
                result = "отсутствует";
                mainMatrix = null;
            }

            //MessageBox.Show("Результат деления на субматрицы (остаток):" + Environment.NewLine + result);            

            /* О матрице mainMatrix:
             * - название массива - mainMatrix
             * - количество строк - mm_j_c
             * - количество столбцов - mm_i_c
             */

            /////
            // Теперь необходимо произвести анализ всех матриц:
            //

            // 1. sMtx1
            if (sMtx1 != null)
            {
                bool h01 = false;
                for (int i = 0; i < i_counter; ++i)
                    if (sMtx1[0][i] == 0 || sMtx1[0][i] == 1)
                        h01 = true;

                if (!h01)
                {
                    // Результат: строковой матрицы несуществует. Все стобцы пусты (255 или 2).
                    sMtx1 = null;
                }

                //System.IO.File.AppendAllText("CTable_123.txt", SaveMatrix(sMtx1, 1, i_counter));
                //MessageBox.Show(SaveMatrix(sMtx1, 1, i_counter));
            }

            // 2. sMtx2
            int j_counterMMJC = j_counter - mm_j_c; // Количество строк остатка строковой матрицы (общее кол-во строк - кол-во строк остатка от гл. мтр)
            if (sMtx2 != null)
            {
                DelAllEmptLine(sMtx2, ref j_counterMMJC, i_counter);    // Чистим матрицу от пустых строк

                // Если строк нет, матрицы тоже нет.
                if (j_counterMMJC == 0)
                    sMtx2 = null;

                //System.IO.File.AppendAllText("CTable_124.txt", SaveMatrix(sMtx2, j_counterMMJC, i_counter));
                //MessageBox.Show(SaveMatrix(sMtx2, j_counterMMJC, i_counter));
            }

            // 3. mainMatrix
            if (mainMatrix != null)
            {
                DelAllEmptLine(mainMatrix, ref mm_j_c, i_counter);      // Чистим матрицу от пустых строк

                // Если строк нет, матрицы тоже нет.
                if (mm_j_c == 0)
                    mainMatrix = null;
                else
                {
                    //MessageBox.Show(SaveMatrix(mainMatrix, mm_j_c, i_counter));
                    //System.IO.File.AppendAllText("CTable_126.txt", SaveMatrix(mainMatrix, mm_j_c, i_counter));
                }

            }

            //
            // Анализ окончен.
            /////

            // Порядок составления структуры
            Submatrix n_sbmtx = new Submatrix();

            n_sbmtx.id = mid;               // Номер структуры

            n_sbmtx.nSBM0 = -1;             // Номер субматрицы 0 (строковой матрицы)
            n_sbmtx.nSBM1 = -1;             // Номер субматрицы 1 (остаток строковой матрицы)
            n_sbmtx.nSBM2 = -1;             // Номер субматрицы 2 (остаток главной матрицы)

            n_sbmtx.sbM0 = null;            // Матрица-строка
            n_sbmtx.sbM1 = null;            // Остаток от матрицы-строки
            n_sbmtx.sbM2 = null;            // Остаток от главной матрицы

            if (sMtx1 != null)              // Если матрица существует
                n_sbmtx.sbM0 = sMtx1[0];    // Матрица-строка

            n_sbmtx.sbM1 = sMtx2;           // Остаток от матрицы-строки
            n_sbmtx.sbM2 = mainMatrix;      // Остаток от главной матрицы

            // Контрольные значения
            n_sbmtx.i_c0 = i_counter;       // Количество столбцов строковой матрицы

            n_sbmtx.i_c1 = i_counter;       // Количество столбцов остатка строковой матрицы

            n_sbmtx.i_c2 = i_counter;       // Количество столбцов остатка главной матрицы

            n_sbmtx.j_c1 = j_counterMMJC;   // Количество строк остатка строковой матрицы

            n_sbmtx.j_c2 = mm_j_c;          // Количество строк остатка главной матрицы

            // Добавляем данную структуру в лист
            return n_sbmtx;
        }

        public int GetIBMatrix(out byte n0)
        {
            // Функция нахождения столбца матрицы с наибольшим количеством одинаковых знаков 0 или 1

            int i_B = -1;
            n0 = 255;    // Тип значения (0 или 1); 255 - аналог -1 для типа byte

            byte[] type_i01 = new byte[i_counter];      // Значение наибольшего количества значений каждого столбца матрицы
            int[] i_count01 = new int[i_counter];       // Количество общих значений каждого столбца матрицы

            for (int i = 0; i < i_counter; ++i)
            {
                int i0 = 0, i1 = 0;
                for (int j = 0; j < j_counter; ++j)
                {
                    if (matrix[j][i] == 0)
                        ++i0;

                    if (matrix[j][i] == 1)
                        ++i1;
                }

                if (i0 > i1)
                {
                    type_i01[i] = 0;
                    i_count01[i] = i0;
                }
                else
                {
                    type_i01[i] = 1;
                    i_count01[i] = i1;
                }
            }

            // Находим наибольшее число общих элементов среди столбцов
            // Определяем
            int i_Bv = -1;
            for (int i = 0; i < i_counter; ++i)
            {
                if (i_count01[i] > i_Bv)
                {
                    i_Bv = i_count01[i];
                    i_B = i;
                    n0 = type_i01[i];
                }
            }

            int vBest = 0;
            for (int i = 0; i < i_counter; ++i)
                if (i_count01[i] == i_Bv)
                    ++vBest;

            // Если количество столбцов с одинаковым максимальным количеством одинаковых знаков больше одного в матрице
            if (vBest > 1)
            {
                int i0 = 0, i1 = 0;
                for (int i = 0; i < i_counter; ++i)
                {
                    if (i_count01[i] == i_Bv)
                    {
                        switch (type_i01[i])
                        {
                            case 0: ++i0; break;
                            case 1: ++i1; break;
                        }
                    }
                }

                for (int i = 0; i < i_counter; ++i)
                {
                    if (i_count01[i] != i_Bv)
                        continue;

                    if (type_i01[i] == 0 && i0 < i1)
                    {
                        n0 = 0;
                        return i;
                    }

                    if (type_i01[i] == 1 && i1 < i0)
                    {
                        n0 = 1;
                        return i;
                    }
                }
            }

            return i_B;
        }

        public void CounterStb(ref int[] valMI, ref int counter_vlMI, int i_B, byte n0, ref int[] vlMI_type)
        {
            // Функция выборки столбцов, имеющих 2+ одинаковых значения, но противоположных столбцу i_B //

            for (int i = 0; i < i_counter; ++i)
            {
                if (i_B == i)   // Пропускаем столбец i_B
                    continue;

                // Счетчики одинаковых значений
                int _n0 = 0;
                int _n1 = 0;

                int j_ = 0;                                     // Счетчик преобразуемых строк (количества одинаковых значений)
                for (int j = 0; j < j_counter; ++j)             // Счет строки
                    if (matrix[j][i] != n0 && matrix[j][i] < 2)
                        ++j_;

                for (int j = 0; j < j_counter; ++j)
                {
                    if (matrix[j][i] == 0)
                        ++_n0;

                    if (matrix[j][i] == 1)
                        ++_n1;
                }                    

                /*
                 * Если значений в выбранном столбце, противоположных значению главного столбца i_B n0,
                 * 2 или более, то столбец становится избранным и добавляется в массив.
                 */

                if (_n0 > 1 || _n1 > 1)
                {
                    vlMI_type[counter_vlMI] = _n0 > _n1 ? 0 : 1;
                    valMI[counter_vlMI] = i;
                    ++counter_vlMI;
                }
            }
        }

        public void SortStb(ref int[] valMI, int counter_vlMI, int i_B, int n0, ref int[] vlMI_type)
        {
            // Функция сортировки "избранных" стобцов с помощью функции CounterStb

            bool w_end = false;
            while (!w_end)
            {
                w_end = true;

                string result = "";
                for (int j = 0; j < j_counter; ++j)
                {
                    for (int c = 0; c < counter_vlMI; ++c)
                        result += matrix[j][valMI[c]];

                    result += Environment.NewLine;
                }
                //MessageBox.Show(result);

                for (int c = 0; c < counter_vlMI - 1; ++c)
                {
                    for (int c2 = 0; c2 < counter_vlMI - c - 1; ++c2)
                    {
                        int i_c = 0;
                        int i_c2 = 0;
                        for (int j = 0; j < j_counter; ++j)
                        {
                            if (matrix[j][valMI[c2]] == vlMI_type[c2])// && matrix[j][valMI[c2]] < 2)
                                ++i_c;

                            if (matrix[j][valMI[c2 + 1]] == vlMI_type[c2 + 1])// && matrix[j][valMI[c2 + 1]] < 2)
                                ++i_c2;
                        }

                        if (i_c < i_c2)
                        {
                            //MessageBox.Show("Swap: " + valMI[c2] + ";" + valMI[c2 + 1] + Environment.NewLine + "TYPE: " + vlMI_type[c2] + ";" + vlMI_type[c2 + 1]
                            //                + Environment.NewLine + "I: " + i_c + ";" + i_c2);
                            SwapInt(ref valMI[c2], ref valMI[c2 + 1]);
                            SwapInt(ref vlMI_type[c2], ref vlMI_type[c2 + 1]); // Типы тоже меняем местами
                            w_end = false;
                        }
                    }
                }
            }
        }

        public bool GetsMtx12(int[] valMI, ref byte[][] sMtx1, ref byte[][] sMtx2, int counter_vlMI, int i_B, byte n0, out int[] j_anMtx, ref int[] vlMI_type)
        {

            if (j_counter == -1 || i_counter == -1)
            {
                j_counter = 0;
                i_counter = 0;
            }

            int j_ane_c = 0;
            int i_ane_c = 0;

            int[] j_anexcl = new int[j_counter];
            int[] i_anexcl = new int[i_counter];

            for (int j = 0; j < j_counter; ++j)
            {
                if (matrix[j][i_B] == n0)
                    if (matrix[j][valMI[0]] == vlMI_type[0])// && matrix[j][valMI[0]] < 2)
                        j_anexcl[j_ane_c++] = j;
            }

            // Одной строки для деления мало, деление бессмысленно
            if (j_ane_c == 1)
                j_ane_c = 0;

            for (int c = 0; c < counter_vlMI; ++c)
            {
                int i_a_n0 = 0;

                for (int j_an = 0; j_an < j_ane_c; ++j_an)
                    if (matrix[j_anexcl[j_an]][valMI[c]] == vlMI_type[c])// && matrix[j_anexcl[j_an]][valMI[c]] < 2)
                        ++i_a_n0;

                if (i_a_n0 > 1)
                {
                    bool t_add = true;
                    for (int j_an = 0; j_an < j_ane_c; ++j_an)
                        if (matrix[j_anexcl[j_an]][valMI[c]] != vlMI_type[c])// || matrix[j_anexcl[j_an]][valMI[c]] == 2)
                            t_add = false;

                    if (t_add)
                        i_anexcl[i_ane_c++] = valMI[c];
                }
            }

            // Строковая матрица
            sMtx1 = new byte[1][];
            sMtx1[0] = new byte[i_counter];

            // Генерация строковой матрицы
            for (int i = 0; i < i_counter; ++i)
            {
                sMtx1[0][i] = 2;

                if (i == i_B)
                    sMtx1[0][i] = matrix[j_anexcl[0]][i_B];

                for (int i_an = 0; i_an < i_ane_c; ++i_an)
                    if (i == i_anexcl[i_an])
                        sMtx1[0][i] = matrix[j_anexcl[0]][i];
            }

            // Генерация остатка после генерации строковой матрицы
            int iCA = i_counter - (i_ane_c); // Количество столбцов в остатке: общее число столбцов - (количество выделенных столбцов + главный столбец)
            if (iCA > 0)
            {
                // Если остаток вообще есть
                sMtx2 = new byte[j_ane_c][];
                for (int s = 0; s < j_ane_c; ++s)
                    sMtx2[s] = new byte[i_counter];

                for (int j_an = 0; j_an < j_ane_c; ++j_an)
                {
                    for (int i = 0; i < i_counter; ++i)
                    {
                        bool cont = false;
                        for (int i_an = 0; i_an < i_ane_c; ++i_an)
                            if (i == i_anexcl[i_an] || i == i_B)
                                cont = true;

                        if (cont)
                        {
                            sMtx2[j_an][i] = 2;
                            continue;
                        }

                        sMtx2[j_an][i] = matrix[j_anexcl[j_an]][i];
                    }
                }
            }

            j_anMtx = new int[j_ane_c];
            for (int j_an = 0; j_an < j_ane_c; ++j_an)
                j_anMtx[j_an] = j_anexcl[j_an];

            // Вывод информации о первых двух матрицах
            string result = "";

            bool emptlySMTX1 = true;

            int counter_smtx1 = 0;
            for (int i = 0; i < i_counter; ++i)
                if (sMtx1[0][i] != 2)
                {
                    emptlySMTX1 = false;
                    ++counter_smtx1;        // Счетчик количества элементов 0 и 1
                }

            if (counter_smtx1 < 2)
                emptlySMTX1 = true;

            if (!emptlySMTX1)
            {
                for (int i_an = 0; i_an < i_counter; ++i_an)
                {
                    string r = sMtx1[0][i_an].ToString();

                    if (r == "2")
                        r = "_";

                    result += r;
                }
            }
            else
            {
                result = "отсутствует";
                sMtx1 = null;
                //return false;   // Деление прошло неудачно.
            }

            //MessageBox.Show("[TEST]Результат деления на субматрицы (sMtx1):" + Environment.NewLine + result);  
            result = "";

            bool wasSMTX2 = false;
            for (int j_an = 0; j_an < j_ane_c; ++j_an)
            {
                wasSMTX2 = false;
                for (int i = 0; i < i_counter; ++i)
                {
                    string r = sMtx2[j_an][i].ToString();

                    if (r == "2")
                        r = "_";

                    result += r;

                    wasSMTX2 = true;
                }

                if (wasSMTX2)
                    result += Environment.NewLine;
            }

            if (!wasSMTX2)  // TODO: корректна ли эта проверка?
            {
                result = "отсутствует";
                sMtx2 = null;
                //return false;   // Деление прошло неудачно.
            }

            //MessageBox.Show("[TEST]Результат деления на субматрицы (sMtx2):" + Environment.NewLine + result);  
            return true;
        }

        public void InitSubMatrix(ref byte[][] matrix, int size1, int size2)
        {
            matrix = new byte[size1][];
            for (int j = 0; j < size1; ++j)
            {
                matrix[j] = new byte[size2];
                for (int i = 0; i < size2; ++i)
                    matrix[j][i] = 255;
            }
        }

        public void SwapInt(ref int a, ref int b)
        {
            //
            // Функция обмена данными
            //

            int c = a;
            a = b;
            b = c;
        }

        void DelAllEmptLine(byte[][] mtx, ref int j_c, int i_c)
        {
            //
            // Функция удаления пустых строк ("_")
            //

            // Ищем пустые строки и устанавливаем 255 (аналог -1 для byte) у первой колонки.
            for (int j = 0; j < j_c; ++j)
            {
                bool h01 = false;
                for (int i = 0; i < i_c; ++i)
                    if (mtx[j][i] == 0 || mtx[j][i] == 1)
                        h01 = true;

                if (!h01)
                    mtx[j][0] = 255;
            }

            // Удаляем пустые строки
            while (j_c != 0)
            {
                for (int j = 0; j < j_c; ++j)
                {
                    // Если нашли пустую строку
                    if (mtx[j][0] == 255)
                    {
                        // Смещаем нижнюю вверх на 1 пункт
                        for (int j_n = j; j_n < j_c - 1; ++j_n)
                            mtx[j_n] = mtx[j_n + 1];

                        --j_c;  // Уменьшаем размер матрицы на 1.
                    }
                }

                bool end = true;
                for (int j = 0; j < j_c; ++j)
                    if (mtx[j][0] == 255)
                        end = false;

                if (end)
                    break;
            }
        }

        public string SaveMatrix(byte[] r_mtx, int i_c)
        {
            byte[][] mtx = new byte[1][];
            mtx[0] = r_mtx;
            return SaveMatrix(mtx, 1, i_c);
        }

        public string SaveMatrix(byte[][] r_mtx, int j_c, int i_c)
        {
            string result = "";

            for (int j = 0; j < j_c; ++j)
            {
                for (int i = 0; i < i_c; ++i)
                {
                    string v = r_mtx[j][i].ToString();

                    if (v == "2")
                        v = "_";

                    result += v + "";
                }
                result += Environment.NewLine;
            }

            return result;
        }        
    }
}
