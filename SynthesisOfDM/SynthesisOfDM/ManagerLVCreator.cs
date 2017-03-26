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
    public class ManagerLVCreator
    {
        // Форма
        SynthesisLV mForm;

        // Форма преобразования
        bool dnf;

        // Класс для синтеза схемы в LabView
        LVCreator lvCreator;

        // Задержка между действиями
        public int _nsleep = 500;

        // Массив с информацией о точках
        public int[][] el_positions;

        public ManagerLVCreator(SynthesisLV sLV, int _slp)
        {
            // Инициализация формы
            mForm = sLV;

            // Инициализации задержки
            _nsleep = _slp;

            // Инициализация позиций элементов
            InitElements();
        }

        public void CreateMatrix()
        {
            // Функция выделения памяти для матрицы
            matrix = new byte[1000][];
            for (int j = 0; j < 1000; ++j)
                matrix[j] = new byte[1000];
        }

        public bool InitMatrix(string tb)
        {
            // В этой функции происходит инициализация матрицы
            if (tb.Length == 0)
                return false;

            // Инициализация матрицы.
            CreateMatrix();

            // TODO:
            //  Обязательно необходимо доделать проверку "пустой строки"
            //  То есть, если строка матрицы полностью состоит из двоек
            //  То ее необходимо удалить.

            string getMatrix = tb;
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
                    matrix[j_counter][n_i_counter++] = Convert.ToByte(gm);
            }

            if (CanConvertToInt(getMatrix[getMatrix.Length - 1].ToString()))
                ++j_counter;

            // Проверка на полностью пустую строку (состоящую из двоек)
            DelAllEmptLine(matrix, ref j_counter, i_counter);

            // Создаем лист подматриц
            listIM = new ListInfoMatrix();

            Submatrix sbmtx = new Submatrix();
            sbmtx.id = 0;
            sbmtx.nSBM0 = -1;
            sbmtx.nSBM1 = -1;
            sbmtx.nSBM2 = -1;

            sbmtx.i_c0 = -1;
            sbmtx.i_c1 = -1;
            sbmtx.i_c2 = i_counter;

            sbmtx.j_c1 = -1;
            sbmtx.j_c2 = j_counter;

            sbmtx.sbM0 = null;
            sbmtx.sbM1 = null;
            sbmtx.sbM2 = null;

            return true;
        }

        public bool LoadSubMatrixs(string filename_data)
        {
            //
            // Функция загрузки структуры матриц
            //

            // Создаем лист подматриц
            listIM = new ListInfoMatrix();

            // Создание класса работы с документом
            TFFile tff = new TFFile();

            // Считывание данных с файла //
            tff.OpenStream(filename_data, FileMode.Open);    // TODO: Имя файла берется из диалогового окна
            tff.StartBR();

            //
            // Процесс считывания
            //

            // Считать форму преобразования
            SetNF(tff.GetBr().ReadBoolean());

            // Считать количество структур
            int count_sm = tff.GetBr().ReadInt32();

            // Считать count_sm структур
            for (int i = 0; i < count_sm; ++i)
                listIM.AddToList(GetStructFromFile(tff.GetBr()));

            tff.StopBR();
            tff.CloseStream();

            // Процесс удаления лишних матриц
            // То есть тех матриц, которые на самом деле разделились на подматрицы
            foreach (Submatrix sbmtx in listIM.listSBMTX)
            {
                if (sbmtx.nSBM0 != -1)
                {
                    sbmtx.sbM0 = null;
                    sbmtx.i_c0 = 0;
                }

                if (sbmtx.nSBM1 != -1)
                {
                    sbmtx.sbM1 = null;
                    sbmtx.j_c1 = 0;
                }                    

                if (sbmtx.nSBM2 != -1)
                {
                    sbmtx.sbM2 = null;
                    sbmtx.j_c2 = 0;
                }
            }

            if(true == false)
            {
                foreach (Submatrix sbmtx in listIM.listSBMTX)
                {
                    string result = "";

                    result += "ID: " + sbmtx.id + Environment.NewLine;
                    result += "nSbm0: " + sbmtx.nSBM0 + Environment.NewLine;
                    result += "nSbm1: " + sbmtx.nSBM1 + Environment.NewLine;
                    result += "nSbm2: " + sbmtx.nSBM2 + Environment.NewLine;

                    if (sbmtx.sbM0 != null)
                    {
                        result += SaveMatrix(sbmtx.sbM0, sbmtx.i_c0) + Environment.NewLine;
                    }

                    if (sbmtx.sbM1 != null)
                    {
                        result += SaveMatrix(sbmtx.sbM1, sbmtx.j_c1, sbmtx.i_c1) + Environment.NewLine;
                    }

                    if (sbmtx.sbM2 != null)
                    {
                        result += SaveMatrix(sbmtx.sbM2, sbmtx.j_c2, sbmtx.i_c2) + Environment.NewLine;
                    }

                    MessageBox.Show(result);
                }
            }

            return true;
        }

        public bool LoadSubMatrixs(bool _dnf, ListInfoMatrix lim)
        {
            // Форма преобразования
            dnf = _dnf;

            // Создаем лист подматриц
            listIM = lim;
            
            // Сообщение об успешном завершении загрузки
            MessageBox.Show("Данные успешно загружены.");

            return true;
        }

        public void InitElements()
        {
            // В этой функции происходит инициализация позиций элементов
            // Пользователю необходимо выполнять инструкции программы

            /* 
             * element_id:
             * 
             * 0 - позиция scroll_lr - нижнего scroll первого окна
             * 1 - позиция scroll_ud - правого scroll первого окна
             * 2 - позиция scroll_lr - нижнего scroll второго окна
             * 3 - позиция scroll_ud - правого scroll второго окна 
             * 4 - позиция btn - кнопки (для копирования)
             * 5 - позиция not - инверсии
             * 6 - позиция compd_arithmtc_or - блок мультисложения
             * 7 - позиция compd_arithmtc_and - блок мультиумножения
             * 8 - позиция начала схемы
             * 9 - позиция включения/выключения automatic tool
             * 10 - позиция блока сложения
             * 11 - позиция блока умножения
             * 
             */

            el_positions = new int[12][];
            for (int i = 0; i < 12; ++i)
                el_positions[i] = new int[2];

            el_positions[0][0] = 268;
            el_positions[0][1] = 573;

            el_positions[1][0] = 600;
            el_positions[1][1] = 325;

            el_positions[2][0] = 928;
            el_positions[2][1] = 573;

            el_positions[3][0] = 1226;
            el_positions[3][1] = 394;

            el_positions[4][0] = 72;
            el_positions[4][1] = 133;

            el_positions[5][0] = 672;
            el_positions[5][1] = 137;

            el_positions[6][0] = 761;
            el_positions[6][1] = 137;

            el_positions[7][0] = 866;
            el_positions[7][1] = 137;

            el_positions[8][0] = 99;
            el_positions[8][1] = 232;

            el_positions[9][0] = 544;
            el_positions[9][1] = 123;

            el_positions[10][0] = 960;
            el_positions[10][1] = 137;

            el_positions[11][0] = 1041;
            el_positions[11][1] = 137;
        }

        public void InitElements(string text_XY)
        {
            el_positions = new int[12][];
            for (int i = 0; i < 12; ++i)
                el_positions[i] = new int[2];

            string v_c = "";
            int p1 = 0;
            int p2 = 0;
            for(int i = 0; i < text_XY.Length; ++i)
            {
                string v = text_XY[i].ToString();

                if (v == "\r")
                {
                    el_positions[p1][p2] = Convert.ToInt32(v_c);
                    v_c = "";

                    if (p2++ == 1)
                    {
                        p2 = 0;
                        ++p1;
                    }

                    continue;
                }

                if (CanConvertToBIint(v))
                    v_c += v;
            }
        }

        public void SetNF(bool _dnf)
        {
            dnf = _dnf; 
        }

        public void Start()
        {
            // Инициализация синтезатора схем LabView
            lvCreator = new LVCreator(mForm, dnf, _nsleep);

            // Установка позиций
            lvCreator.SetElements(el_positions);

            // Подсчет количества столбцов
            int i_c = listIM.listSBMTX[0].i_c0;

            if (listIM.listSBMTX[0].i_c1 > i_c)
                i_c = listIM.listSBMTX[0].i_c1;

            if (listIM.listSBMTX[0].i_c2 > i_c)
                i_c = listIM.listSBMTX[0].i_c2;

            // Количество кнопок = i_counter (кол-во столбцов)
            // Запуск этапа создания кнопок (входные данные, boolean)
            lvCreator.StepCreateBtn(i_c);

            // Количество инверсий зависит от столбцов ВСЕХ МАТРИЦ
            // Запуск этапа создания инверсий

            GenInversion(i_c);

            int counter_smtx = 0;       // Счетчик матриц
            int full_j_counter = -1;    // Общее количество строк всех матриц
            foreach (Submatrix sbmtx in listIM.listSBMTX)
            {
                if (sbmtx.sbM0 != null) full_j_counter += 1;
                if (sbmtx.sbM1 != null) full_j_counter += sbmtx.j_c1;
                if (sbmtx.sbM2 != null) full_j_counter += sbmtx.j_c2;
            }

            // Синтез первой матрицы
            foreach(Submatrix sbmtx in listIM.listSBMTX)
            {
                //break;

                // Синтез строковой матрицы
                SMatrix(sbmtx.sbM0, sbmtx.i_c0, ref full_j_counter, 1);

                // Синтез остатка строковой матрицы
                SMatrix(sbmtx.sbM1, sbmtx.j_c1, sbmtx.i_c1, ref full_j_counter, 1);

                // Синтез остатка главной матрицы
                SMatrix(sbmtx.sbM2, sbmtx.j_c2, sbmtx.i_c2, ref full_j_counter, 1);           
            }

            // Возврат в стартовую позицию
            lvCreator.SetStartPositionSCBUD();

            // Скопировать элемент (в данном случае - блок мультисложения - 7)
            lvCreator.CopyElement(7);

            foreach (Submatrix sbmtx in listIM._listSBMTX)
            {
                //break;

                // Синтез остатка главной матрицы
                SMatrix(sbmtx.sbM2, sbmtx.j_c2, sbmtx.i_c2, ref counter_smtx, 2);

                // Синтез остатка строковой матрицы
                SMatrix(sbmtx.sbM1, sbmtx.j_c1, sbmtx.i_c1, ref counter_smtx, 2);

                // Синтез строковой матрицы
                SMatrix(sbmtx.sbM0, sbmtx.i_c0, ref counter_smtx, 2);
            }

            // Возврат в стартовую позицию
            lvCreator.SetStartPositionSCBUD();

            // Теперь необходимо подсчитать, какую позицию scroll_ud имеет каждая подматрица.
            // Примечание: Данные берутся с конца матрицы.
            int suv_counter = 0; int sum_suv = 0;
            int[] scroll_ud_value = new int[listIM.GetCount()];
            foreach (Submatrix sbmtx in listIM._listSBMTX)
            {
                if (sbmtx.j_c1 > 0)
                {
                    sum_suv += 1;          // строковая матрица
                    sum_suv += sbmtx.j_c1; // остаток строковой матрицы
                }                    

                if (sbmtx.j_c2 > 0)
                    sum_suv += sbmtx.j_c2; // остаток главной матрицы

                scroll_ud_value[suv_counter++] = sum_suv;
            }

            // Теперь начинается синтез связей между подматрицами
            // Примечание: работа идет с конца списка подматриц
            lvCreator.StepCreateSubMatrixsWires(listIM, scroll_ud_value);

            MessageBox.Show("Синтез завершен.");
        }

        public void SMatrix(byte[] mtx, int i_c, ref int mtxCounter_shift, int param)
        {
            // Синтез строковой матрицы
            byte[][] _mtx = new byte[1][]; _mtx[0] = mtx;
            SMatrix(_mtx, 1, i_c, ref mtxCounter_shift, param);
        }

        public void SMatrix(byte[][] mtx, int j_c, int i_c, ref int mtxCounter_shift, int param)
        {
            // Переменная сдвига матриц
            // mtxCounter_shift - сдвигает целую матрицу вниз на столько, чтобы 
            // другие матрицы (в дальнейшем синтезе) поместились выше.

            if (mtx == null || j_c == 0 || i_c == 0)
                return;

            // Загрузка матрицы в класс
            lvCreator.InitMatrix(mtx, j_c, i_c);

            switch (param)
            {
                case 1:
                    // Запуск этапа создания блоков мультисложения строк
                    lvCreator.StepCreateCompdArithOr(ref mtxCounter_shift);
                break;
                case 2:
                    // Запуск этапа создания блоков мультиумножения блоков мультисложения
                    lvCreator.StepCreateCompdArithAnd(mtxCounter_shift++);
                break;
            }
        }

        void GenInversion(int i_c)
        {
            //
            //  Функция определения необходимости инверсии в каждом столбце всех матриц
            //

            // Массив, определяющий истинность инверсии в данном столбце
            bool[] nInver = new bool[i_c];
            for (int i = 0; i < i_c; ++i)
                nInver[i] = false;

            foreach (Submatrix smtx in listIM.listSBMTX)
            {
                if (smtx.sbM0 != null)
                {
                    for (int i = 0; i < smtx.i_c0; ++i)
                        nInver[i] |= !ConvertToBool(smtx.sbM0[i]);
                }

                if (smtx.sbM1 != null)
                {
                    for (int j = 0; j < smtx.j_c1; ++j)
                        for (int i = 0; i < smtx.i_c1; ++i)
                            nInver[i] |= !ConvertToBool(smtx.sbM1[j][i]);
                }

                if (smtx.sbM2 != null)
                {
                    for (int j = 0; j < smtx.j_c2; ++j)
                        for (int i = 0; i < smtx.i_c2; ++i)
                            nInver[i] |= !ConvertToBool(smtx.sbM2[j][i]);
                }
            }

            // Все инверсии учтены. Отправляем их на синтез
            lvCreator.StepCreateInversion(nInver, i_c);
        }

        bool CanConvertToInt(string v)
        {
            // Функция проверки символа на число
            try
            {
                int r = Convert.ToInt32(v);

                if (r > 2 || r < 0)
                    return false;

                return true;
            }
            catch
            {
                if (v == "_")
                    return true;
                return false;
            }
        }

        bool CanConvertToBIint(string v)
        {
            // Функция проверки символа на число
            try
            {
                int r = Convert.ToInt32(v);
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool ConvertToBool(byte value)
        {
            switch (value)
            {
                case 0: return false;
                case 1: return true;
                case 2: return true;
                case 255: return true;
                default: return true;
            }
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

        public Submatrix GetStructFromFile(BinaryReader br)
        {
            Submatrix result = new Submatrix();

            // Ид структуры
            result.id = br.ReadInt32();

            // Данные о нумерах подматриц данной матрицы
            result.nSBM0 = br.ReadInt32();
            result.nSBM1 = br.ReadInt32();
            result.nSBM2 = br.ReadInt32();

            // Данные о размерах матриц
            result.i_c0 = br.ReadInt32();
            result.j_c1 = br.ReadInt32();
            result.j_c2 = br.ReadInt32();
            result.i_c1 = br.ReadInt32();
            result.i_c2 = br.ReadInt32();

            result.sbM0 = null;
            result.sbM1 = null;
            result.sbM2 = null;

            // I. Строковая матрица
            if (result.i_c0 > 0)
            {
                result.sbM0 = new byte[result.i_c0];
                for (int i = 0; i < result.i_c0; ++i)
                    result.sbM0[i] = br.ReadByte();
            }          

            // II. Остаток строковой матрицы
            if (result.j_c1 > 0 && result.i_c1 > 0)
            {
                result.sbM1 = new byte[result.j_c1][];
                for (int j = 0; j < result.j_c1; ++j)
                {
                    result.sbM1[j] = new byte[result.i_c1];
                    for (int i = 0; i < result.i_c1; ++i)
                        result.sbM1[j][i] = br.ReadByte();
                }
            }

            // III. Остаток строковой матрицы
            if (result.j_c2 > 0 && result.i_c2 > 0)
            {
                result.sbM2 = new byte[result.j_c2][];
                for (int j = 0; j < result.j_c2; ++j)
                {
                    result.sbM2[j] = new byte[result.i_c2];
                    for (int i = 0; i < result.i_c2; ++i)
                        result.sbM2[j][i] = br.ReadByte();
                }
            }

            string mtxs = "";
            if (result.sbM0 != null)
            {
                //mtxs = SaveMatrix(result.sbM0, result.i_c0);
                //MessageBox.Show(mtxs);
            }

            mtxs = "";
            if (result.sbM1 != null)
            {
                //mtxs = SaveMatrix(result.sbM1, result.j_c1, result.i_c1);
                //MessageBox.Show(mtxs);
            }

            mtxs = "";
            if (result.sbM2 != null)
            {
                //mtxs = SaveMatrix(result.sbM2, result.j_c2, result.i_c2);
                //MessageBox.Show(mtxs);
            }

            return result;
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

        // Если синтезируется одна матрица
        int j_counter;  // Строка матрицы
        int i_counter;  // Столбец матрицы
        public byte[][] matrix;

        // Лист с подматрицами и с их связями
        ListInfoMatrix listIM;
    }
}
