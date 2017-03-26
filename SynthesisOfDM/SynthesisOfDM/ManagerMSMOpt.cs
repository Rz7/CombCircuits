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
    class ManagerMSMOpt
    {
        //
        // Класс, контроллирующий работу создания подматриц
        //

        // Класс для работы с матрицами для их деления на подматрицы
        MSubMatrixOpt msmOpt;

        public void CreateMatrix()
        {
            j_counter = 0;
            i_counter = 0;

            // Функция выделения памяти для матрицы
            InitSubMatrix(ref matrix, MAXSIZE, MAXSIZE);
        }

        public void DeleteMatrix()
        {
            InitSubMatrix(ref matrix, 1, 1);
        }

        public void CreateListSubMTX()
        {
            // Создаем лист подматриц
            listIM = new ListInfoMatrix();
        }

        public bool InitMatrix(MMatrix f, string tb)
        {
            // В этой функции происходит инициализация матрицы
            if (tb.Length == 0)
                return false;

            // Инициализация формы
            mmtx = f;

            // Создание матрицы
            CreateMatrix();

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
            for (int j = 0; j < j_counter; ++j)
            {
                bool has_01 = false;
                for (int i = 0; i < i_counter; ++i)
                {
                    if (matrix[j][i] != 2)
                        has_01 = true;
                }

                if (!has_01)
                {
                    MessageBox.Show("Полностью пустые строки недопустимы: строка " + j.ToString());
                    return false;
                }
            }

            return true;
        }

        public int GenSubMatrixs()
        {
            //
            // Функция создания подматриц.
            //

            //
            // Самая главная матрица и ее параметры:
            // matrix       - матрица
            // j_counter    - кол-во строк
            // i_counter    - кол-во столбцов
            //

            int ref_int_1 = -1;

            // Делим главную матрицу на подматрицы.
            int id_start_mtx = GenSubMatrix(matrix, j_counter, i_counter, -1, ref ref_int_1);

            //
            // Теперь необходимо удалить одинаковые матрицы (оставить одну и добавить в новую структуру)
            // 1) Ищем одинаковые матрицы пузырьковым методом
            // 2) Опустошаем матрицы (ставим null)
            // 3) Создаем структуру подматрицы, добавив в "остаток гл. матрицы" данную матрицу
            // 4) Указать ссылки вместо опустошенных подматриц на структуру созданной подматрицы
            //
           
            while(true) 
            {
                bool cont = false; // cont - продолжать проверку "пузырьковой сортировкой" до тех пор, пока не останется совпадений
                for (int i = 0; i < listIM.GetCount() - 1; ++i)
                    for (int p = 0; p < listIM.GetCount() - i - 1; ++p)
                        if (listIM.listSBMTX[i].id >= id_start_mtx && listIM.listSBMTX[i + 1].id >= id_start_mtx)
                            // Сравнение подматриц на наличие одинаковых матриц проводится только по данной функции, другие функции мы не трогаем
                            if (SelectionEqMatrix(listIM.listSBMTX[i], listIM.listSBMTX[i + 1]))
                                cont = true;

                if (!cont)
                    break;

                //MessageBox.Show("123");
            }

            return id_start_mtx;
        }

        public int GetCountMatrix()
        {
            // Подсчитать кол-во подматриц
            int count_matrix = 0;
            foreach (Submatrix sbmtx in listIM.listSBMTX)
            {
                if (sbmtx.sbM0 != null) ++count_matrix;
                if (sbmtx.sbM1 != null) ++count_matrix;
                if (sbmtx.sbM2 != null) ++count_matrix;
            }

            return count_matrix;
        }

        public int GenSubMatrix(byte[][] mtx, int j_c, int i_c, int pid, ref int nSBM)   // pid - parent id struct, nSBM - номер субматрицы данной структуры
                                                                                          //                                (чтобы о нем знал родитель)
        {
            //
            //  Функция деления конкретной матрицы на подматрицы.
            //  Перед делением выполняется поглощение.
            //

            // Если матрица пуста, то делать дальше нечего.
            if (mtx == null)
                return -1;

            // Если размер матрицы менее 3x3
            if (pid > -1 && (j_c < 3 || i_c < 3))
                return -1;

            // Поглощение матрицы
            FAbsorbMatrix(ref mtx, ref j_c, ref i_c);

            // Создали новую структуру
            Submatrix new_sbmtx = new Submatrix();

            // Инициализировали класс деления матрицы
            msmOpt = new MSubMatrixOpt();

            // Загрузили в него матрицу
            msmOpt.InitMatrix(mtx, j_c, i_c);

            // Поделили матрицу и загрузили результат в структуру
            new_sbmtx = msmOpt.GenSubMatrix(listIM.GetCount());

            if (new_sbmtx == null)
                return -1;

            /*
             * 
             * Условие создания подматрицы данной матрицы:
             * 1) (sMtx1 || sMtx2) && mainMtx
             * 2) sMtx1 && sMtx2
             * 
             * Пояснение:
             * 1) Существует sMtx1 или sMtx2 + обязательно остаток
             * 2) Существует sMtx1 и sMtx2
             * 
             * В этих двух случаях sMtx2 и mainMtx (при случае 1) можно делить на подсхемы.
             * 
             */
            
            if (pid > -1 && new_sbmtx.sbM0 == null && new_sbmtx.sbM1 == null && EquMatrix(mtx, new_sbmtx.sbM2, j_c, i_c))
            {
                // Если остаток главной матрицы полностью совпадает с входной,
                // то дальше данную матрицу делить бессмысленно.
                return -1;

                // Данное условие работает некорректно. Матрица делится вечно.
            }

            if ( pid == -1  // Данная структура первая (родителей нет)
                || new_sbmtx.sbM0 != null && new_sbmtx.sbM1 != null)
            {

                // Матрица успешно поделилась и поэтому записывается как структура.
                listIM.AddToList(new_sbmtx);

                // Если pid >= 0, то необходимо указать в pid структуре id этой структуры
                nSBM = new_sbmtx.id;

                //
                // nSBM - ссылка на значение, номер одной из подматрицы родительской матрицы.
                //

                //
                // Если sMtx2 или mainMtx нужно поделить, то рекурсивно выполняется функция
                // GenSubMatrix, с указанием параметров матрицы (сама матрица, кол-во строк и столбцов).
                // 
                // В случае правильного деления (матрица поделилась на 2-3 части), то ее структура (Submatrix)
                // добавляется в список данных структур. Кроме того, в родительской структуре должно быть прописано
                // nSBM1 (остаток строковой матрицы) или nSBM2 (остаток главной матрицы). Это ID другой структуры, на которую
                // поделилась данная матрица.
                //
                //
                //
                //

                // Далее нужно разделить на подсхемы остаток строковой матрицы:
                GenSubMatrix(new_sbmtx.sbM1, new_sbmtx.j_c1, new_sbmtx.i_c1, new_sbmtx.id, ref new_sbmtx.nSBM1);

                // Далее нужно разделить на подсхемы остаток главной матрицы:
                GenSubMatrix(new_sbmtx.sbM2, new_sbmtx.j_c2, new_sbmtx.i_c2, new_sbmtx.id, ref new_sbmtx.nSBM2);

                // Обнуляем матрицы, если те поделились на подматрицы
                if (new_sbmtx.nSBM0 != -1)
                    new_sbmtx.sbM0 = null;

                if (new_sbmtx.nSBM1 != -1)
                    new_sbmtx.sbM1 = null;

                if (new_sbmtx.nSBM2 != -1)
                    new_sbmtx.sbM2 = null;

                if (new_sbmtx.sbM0 == null)
                    new_sbmtx.i_c0 = 0;

                if (new_sbmtx.sbM1 == null)
                {
                    new_sbmtx.i_c1 = 0;
                    new_sbmtx.j_c1 = 0;
                }

                if (new_sbmtx.sbM2 == null)
                {
                    new_sbmtx.i_c2 = 0;
                    new_sbmtx.j_c2 = 0;
                }
            }

            return new_sbmtx.id;
        }

        // Функция, проверяющая совпадают ли данные матрицы
        bool EquMatrix(byte[][] mtx1, byte[][] mtx2, int j_c, int i_c)
        {
            if (mtx1 == null || mtx2 == null)
                return false;

            for (int j = 0; j < j_c; ++j)
                for (int i = 0; i < i_c; ++i)
                    if (mtx1[j][i] != mtx2[j][i])
                        return false;

            return true;
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

        public void FAbsorbMatrix(ref byte[][] mtx, ref int j_c, ref int i_c)
        {
            //
            // Функция поглощения строк матрицы
            //

            (new AbsorbMatrix(mtx, j_c, i_c)).GenMatrix(out j_c, out i_c);
        }

        public bool EqMatrix(byte[][] mtx1, byte[][] mtx2, int j_c1, int i_c1, int j_c2, int i_c2)
        {
            //
            //  Функция определяет, совпадают ли две матрицы, указанные в параметрах
            //

            if (mtx1 == null || mtx2 == null)
                return false;

            if (j_c1 != j_c2 || i_c1 != i_c2)
                return false;

            for (int j = 0; j < j_c1; ++j)
                for (int i = 0; i < i_c1; ++i)
                    if (mtx1[j][i] != mtx2[j][i])
                        return false;

            return true;
        }

        public bool SelectionEqMatrix(Submatrix sm1, Submatrix sm2)
        {
            //
            // Функция определяет, есть ли среди данных двух структур одинаковые матрицы
            //

            bool result = false;

            if (EqMatrix(sm1.sbM1, sm2.sbM1, sm1.j_c1, sm1.i_c1, sm2.j_c1, sm2.i_c1))
            {
                // Остаток строковой с остатком строковой
                result = true;

                // Создаем структуру, записав туда матрицу
                int r = CreateEqMatrix(sm1.sbM1, sm1.j_c1, sm1.i_c1);

                // Указываем данную структуру как ссылку
                sm1.nSBM1 = r;
                sm2.nSBM1 = r;

                // Удаляем обе матрицы из проверяемых структур
                sm1.sbM1 = null;
                sm1.j_c1 = 0;
                sm1.i_c1 = 0;

                sm2.sbM1 = null;
                sm2.j_c1 = 0;
                sm2.i_c1 = 0;
            }

            if (EqMatrix(sm1.sbM1, sm2.sbM2, sm1.j_c1, sm1.i_c1, sm2.j_c2, sm2.i_c2))
            {
                // Остаток строковой с остатком главной
                result = true;

                // Создаем структуру, записав туда матрицу
                int r = CreateEqMatrix(sm1.sbM1, sm1.j_c1, sm1.i_c1);

                // Указываем данную структуру как ссылку
                sm1.nSBM1 = r;
                sm2.nSBM2 = r;

                // Удаляем обе матрицы из проверяемых структур
                sm1.sbM1 = null;
                sm1.j_c1 = 0;
                sm1.i_c1 = 0;

                sm2.sbM2 = null;
                sm2.j_c2 = 0;
                sm2.i_c2 = 0;
            }

            if (EqMatrix(sm1.sbM2, sm2.sbM1, sm1.j_c2, sm1.i_c2, sm2.j_c1, sm2.i_c1))
            {
                // Остаток главной с остатком строковой
                result = true;

                // Создаем структуру, записав туда матрицу
                int r = CreateEqMatrix(sm1.sbM2, sm1.j_c2, sm1.i_c2);

                // Указываем данную структуру как ссылку
                sm1.nSBM2 = r;
                sm2.nSBM1 = r;

                // Удаляем обе матрицы из проверяемых структур
                sm1.sbM2 = null;
                sm1.j_c2 = 0;
                sm1.i_c2 = 0;

                sm2.sbM1 = null;
                sm2.j_c1 = 0;
                sm2.i_c1 = 0;
            }

            if (EqMatrix(sm1.sbM2, sm2.sbM2, sm1.j_c2, sm1.i_c2, sm2.j_c2, sm2.i_c2))
            {
                // Остаток главной с остатком главной
                result = true;

                // Создаем структуру, записав туда матрицу
                int r = CreateEqMatrix(sm1.sbM2, sm1.j_c2, sm1.i_c2);

                // Указываем данную структуру как ссылку
                sm1.nSBM2 = r;
                sm2.nSBM2 = r;

                // Удаляем обе матрицы из проверяемых структур
                sm1.sbM2 = null;
                sm1.j_c2 = 0;
                sm1.i_c2 = 0;

                sm2.sbM2 = null;
                sm2.j_c2 = 0;
                sm2.i_c2 = 0;
            }

            return result;
        }

        public int CreateEqMatrix(byte[][] mtx, int j_c, int i_c)
        {
            //
            // Функция создает новую подматрицу, состоящую исключительно из остатка главной матрицы
            // Остаток главной матрицы - входная матрица данной функции
            // Функция возвращает ID подматрицы, созданной для данной матрицы
            //

            Submatrix n_sbmtx = new Submatrix();

            n_sbmtx.id = listIM.GetCount(); // Номер структуры

            n_sbmtx.nSBM0 = -1;             // Номер субматрицы 0 (строковой матрицы)
            n_sbmtx.nSBM1 = -1;             // Номер субматрицы 1 (остаток строковой матрицы)
            n_sbmtx.nSBM2 = -1;             // Номер субматрицы 2 (остаток главной матрицы)

            n_sbmtx.sbM0 = null;            // Матрица-строка
            n_sbmtx.sbM1 = null;            // Остаток от матрицы-строки
            n_sbmtx.sbM2 = mtx;             // Остаток от главной матрицы

            // Контрольные значения
            n_sbmtx.i_c0 = 0;               // Количество столбцов строковой матрицы
            n_sbmtx.i_c1 = 0;               // Количество столбцов остатка строковой матрицы
            n_sbmtx.j_c1 = 0;               // Количество строк остатка строковой матрицы

            n_sbmtx.j_c2 = j_c;             // Количество строк остатка главной матрицы
            n_sbmtx.i_c2 = i_c;             // Количество столбцов остатка главной матрицы

            listIM.AddToList(n_sbmtx);
            return n_sbmtx.id;
        }

        public ListInfoMatrix GetLim()
        {
            //
            // Функция возвращает лист подматриц для их анализа
            //

            return listIM;
        }

        public void SaveAllSMatrix(string filename, bool dnf)
        {
            //
            // Функция записи всех структур Submatrix в файл.
            //

            // Создание класса работы с документом
            TFFile tff = new TFFile();

            // Запись данных в файл //
            tff.OpenStream(filename, FileMode.Create);
            tff.StartBW();

            //
            // Процесс записи
            //

            // Записать форму преобразования
            tff.GetBw().Write(dnf);

            // Записать количество подматриц
            tff.GetBw().Write(listIM.GetCount());

            // Записать подматрицы
            foreach (Submatrix sm in listIM.listSBMTX)
                SaveStructToFile(sm, tff.GetBw());

            tff.StopBW();
            tff.CloseStream();

            MessageBox.Show("Структура успешно сохранена.");
        }

        public void SaveStructToFile(Submatrix sm, BinaryWriter bw)
        {
            // Ид структуры
            bw.Write(sm.id);

            // Данные о нумерах подматриц данной матрицы
            bw.Write(sm.nSBM0);
            bw.Write(sm.nSBM1);
            bw.Write(sm.nSBM2);

            // Данные о размерах матриц
            bw.Write(sm.i_c0);
            bw.Write(sm.j_c1);
            bw.Write(sm.j_c2);
            bw.Write(sm.i_c1);
            bw.Write(sm.i_c2);

            //  Примечание:
            //  Если подматрицы записывать не нужно, необходимо для
            //  sm.i_c0, sm.j_c1 и sm.j_c2 установить значения 0.

            // I. Строковая матрица
            for (int i = 0; i < sm.i_c0; ++i)
                bw.Write(sm.sbM0[i]);

            // II. Остаток строковой матрицы
            for (int j = 0; j < sm.j_c1; ++j)
                for (int i = 0; i < sm.i_c1; ++i)
                    bw.Write(sm.sbM1[j][i]);

            // III. Остаток строковой матрицы
            for (int j = 0; j < sm.j_c2; ++j)
                for (int i = 0; i < sm.i_c2; ++i)
                    bw.Write(sm.sbM2[j][i]);
        }

        // Максимальный размер массива
        int MAXSIZE = 1000;

        // Параметры главной матрицы
        int j_counter;      // Строка матрицы
        int i_counter;      // Столбец матрицы
        byte[][] matrix;    // Сама матрица

        // Класс окна минимизации матрицы
        MMatrix mmtx;

        // Лист с подматрицами и с их связями
        ListInfoMatrix listIM;
    }

    // Лист структур Submatrix для сохранения в файл
    public class ListInfoMatrix
    {
        public List<Submatrix> listSBMTX = new List<Submatrix>();
        public List<Submatrix> _listSBMTX = new List<Submatrix>();  // Лист "с обратной стороны"

        public void AddToList(Submatrix sb_mtx)
        {
            listSBMTX.Add(sb_mtx);

            _listSBMTX = new List<Submatrix>();
            for (int i = listSBMTX.Count() - 1; i >= 0; --i)
                _listSBMTX.Add(listSBMTX[i]);
        }

        public Submatrix GetElementById(int id)
        {
            foreach (Submatrix sm in listSBMTX)
                if (sm.id == id)
                    return sm;

            return null;
        }

        public int GetCount()
        {
            return listSBMTX.Count();
        }
    }

    // Структура матрицы с возможным делением на подматрицы
    public class Submatrix
    {
        public int id;      // Номер структуры (индентификатор)
        public int nSBM0;   // Номер субматрицы 0 (строковой матрицы)
        public int nSBM1;   // Номер субматрицы 1 (остаток строковой матрицы)
        public int nSBM2;   // Номер субматрицы 2 (остаток главной матрицы)
        // Если "-1", то берется массив int

        // Подматрицы данной матрицы
        public byte[] sbM0;                         // Строковая подматрица
        public byte[][] sbM1;                       // Остаток строковой матрицы
        public byte[][] sbM2;                       // Остаток главной матрицы
        public int i_c0, i_c1, i_c2, j_c1, j_c2;    // Размеры матриц
    }
}
