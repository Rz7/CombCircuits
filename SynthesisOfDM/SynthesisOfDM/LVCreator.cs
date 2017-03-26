using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace SynthesisOfDM
{
    class LVCreator
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, long dwFlags, long dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetAsyncKeyState(int vkey);

        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public const int KEYEVENTF_KEYUP = 0x0002;

        public const int VK_SPACE = 0x20;

        public int[] VK_N = new int[10];

        public const int VK_F1 = 0x70;
        public const int VK_F2 = 0x71;
        public const int VK_F3 = 0x72;
        public const int VK_F4 = 0x73;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;

        public const int VK_LCTRL = 0x11;
        public const int VK_C = 0x43;
        public const int VK_V = 0x56;

        int j_counter;  // Строка матрицы
        int i_counter;  // Столбец матрицы
        public byte[][] matrix;

        int scblr = 0;  // Сдвиг scroll_lr (нижний scroll)
        int scbud = 0;  // Сдвиг scroll_ud (правый scroll)

        bool dnf;           // Форма преобразования
        
        int _nsleep = 100;  // Задержка

        class ElementPosition
        {
            // Структура позиции некоторого элемента
            // id_element - номер элемента
            // x и y - позиция мыши, при которой она будет находиться в середине элемента
            // scroll_lr - сдвиг нижнего scroll
            // scroll_ud - сдвиг правого scroll

            public int id_element;

            public int x;
            public int y;

            public int scroll_lr;
            public int scroll_ud;

            public void SetXY(int _x, int _y)
            {
                x = _x;
                y = _y;
            }
        }

        static long IntToLong(int value)
        {
            return ((long)value) & 0xFFFFFFFFL;
        }

        public void InitVK()
        {
            // Функция инициализации некоторых VirtualKeys

            VK_N[0] = 0x30;
            VK_N[1] = 0x31;
            VK_N[2] = 0x32;
            VK_N[3] = 0x33;
            VK_N[4] = 0x34;
            VK_N[5] = 0x35;
            VK_N[6] = 0x36;
            VK_N[7] = 0x37;
            VK_N[8] = 0x38;
            VK_N[9] = 0x39;
        }

        public LVCreator(SynthesisLV f1, bool _dnf, int sleep)
        {
            // Инициализация формы
            mForm = f1;

            // Инициализация формы преобразования
            dnf = _dnf;

            // Инициализация задержки
            _nsleep = sleep;

            // Инициализация клавиш
            InitVK();
        }

        public void InitMatrix(byte[][] mtx, int j_c, int i_c)
        {
            //
            // Второй способ инициализации матрицы.
            //

            matrix = mtx;
            j_counter = j_c;
            i_counter = i_c;

            DelAllEmptLine(matrix, ref j_counter, i_counter);
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

        public void SetElements(int[][] positions)
        {
            // Опустошаем лист
            EPList = new List<ElementPosition>();

            for (int i = 0; i < 12; ++i)
            {
                ElementPosition ep = new ElementPosition();
                ep.id_element = i;
                ep.x = positions[i][0];
                ep.y = positions[i][1];

                ep.scroll_lr = scblr;
                ep.scroll_ud = scbud;

                EPList.Add(ep);
            }
        }

        public void StepCreateBtn(int i_c)
        {
            // Функция создания кнопок
            // Создается кнопок, равное количеству столбцов матрицы, то есть i_counter

            // Скопировать элемент (в данном случае - кнопка - 4)
            CopyElement(4);

            for (int i = 0; i < i_c; ++i)
            {
                // Фокус мыши на позиции начала создания схемы
                //SetMousePos(EPList[8].x, EPList[8].y);
                //FocusMouseByPos(EPList[8].x, EPList[8].y);

                // Вставка элемента в указанную позицию.
                PasteElement(EPList[8].x, EPList[8].y);

                // Вынужденная задержка
                nSleep(_nsleep);

                // Промотать страницу вниз
                MoveScroll(false, true, -360, true);

                // Вынужденная задержка
                nSleep(_nsleep);
            }
        }

        public void StepCreateInversion(bool[] nInv, int i_c)
        {
            // Функция создания инверсии
            // правее от каждой кнопке (boolean) создается инверсия только в том случае, если у этой кнопке
            // в матрице хотя бы в одной из строк имеется значение 0

            // Перед копированием элемента вернемся в стартовую позицию scroll
            MoveScroll(false, true, -scbud, false);

            // Вынужденная задержка
            nSleep(_nsleep);

            // Скопировать элемент (в данном случае - кнопка - 5)
            CopyElement(5);

            // Вынужденная задержка
            nSleep(_nsleep);

            // Стартовая позиция
            int x = EPList[8].x + 80 + (EPList[3].x - EPList[1].x);
            int y = EPList[8].y;

            // Инверсий в идеальном случае должно создаться столько же, сколько и кнопок
            // То есть i_counter инверсий
            for (int i = 0; i < i_c; ++i)
            {
                // Фокус мыши на позиции начала создания схемы
                //SetMousePos(x, y);
                //FocusMouseByPos(x, y);

                if (nInv[i])
                {
                    // Вставка элемента в указанную позицию.
                    PasteElement(x, y);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    SetMousePos(x + 20, y);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    // Дальше необходимо провести связь с кнопкой
                    SetMousePos(x - 8, y);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    SetMousePos(x - 70, y + 1);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);
                }

                // Спуститься на ступень вниз для создания следующей инверсии
                MoveScroll(false, true, -360, false);

                // Вынужденная задержка
                nSleep(_nsleep);
            }
        }

        public void StepCreateCompdArithOr(ref int mtxCounter_shift)
        {
            // Функция создания блоков мультисложения строк
            // Создается блоков, равное количеству столбцов матрицы, то есть i_counter

            // Перед копированием элемента вернемся в стартовую позицию scroll
            MoveScroll(false, true, -scbud, false);

            // Вынужденная задержка
            nSleep(_nsleep);

            // Скопировать элемент (в данном случае - кнопка - 6)
            CopyElement(6);

            // Вынужденная задержка
            nSleep(_nsleep);

            // Обновляем стартовую позицию, прибавив к x 50, к y 100
            // Так же к позиции прибавляется разница между scroll ud1 и ud2, т.к. работа будет вестись в Block Diagram
            // Стартовая позиция является позицией, левее которой расположена первая кнопка

            int x = EPList[8].x + 150 + (EPList[3].x - EPList[1].x);
            int y = EPList[8].y;

            for (int j = 0; j < j_counter; ++j)
            {
                // Фокус мыши на позиции начала создания схемы
                //SetMousePos(x, y);
                //FocusMouseByPos(x, y);

                Thread.Sleep(500);

                // Вставка элемента в указанную позицию.
                PasteElement(x, y);

                // Вынужденная задержка
                nSleep(_nsleep);

                // Теперь необходимо расширить этот элемент.
                // Количество гнезд для входа каждого j-го элемента должно совпадать с количеством
                // столбцов, вычтя количество двоек в строке.

                int sdv_GnC = -1;
                for (int i = 0; i < i_counter; ++i)
                    if (matrix[j][i] < 2)
                        ++sdv_GnC;

                // TODO:    вынужденная временная мера для избавления от бага
                //          потом подумать, как исправить
                if (sdv_GnC == 0)
                    ++sdv_GnC;

                if (sdv_GnC > 0)
                {
                    // Установка размера
                    SetMousePos(x, y + 4);

                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y + 4, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    SetMousePos(x, y + 4 + sdv_GnC * 8);

                    mouse_event(MOUSEEVENTF_LEFTUP, x, y + 4 + sdv_GnC * 8, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);
                }

                // Необходимо вычесть столбцы, с которыми связи проведены не будут и
                // которые последние.
                int v_ic = 0;
                for (int i = 0; i < i_counter; ++i)
                {
                    if (matrix[j][i] < 2)
                        v_ic = 0;
                    else ++v_ic;
                }

                // Теперь необходимо провести связи между блоком мультисложения и кнопкой
                int s_i_counter = i_counter - v_ic;
                int c_sdvY = 0; // На сколько пикселей происходит сдвиг блока для прокладки связи
                for (int i = 0; i < i_counter; ++i)
                {
                    // Протянуть связь между блоком и кнопкой. Если кнопка = 2, то связь не делается.
                    if (matrix[j][i] < 2)
                    {
                        // Сброс выделения блока
                        SetMousePos(x + 100, y);
                        FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                        // Установка позиции на связь
                        SetMousePos(x - 8, y + 2 + c_sdvY);
                        FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                        int sdv = 160;

                        if (matrix[j][i] == 0)
                            sdv = 70; // 70, здесь будет стоять инверсия (в будущем)

                        SetMousePos(x - sdv, y + 1);
                        FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                        c_sdvY += 8;
                    }

                    int p10 = 5;// i > 2 ? 10 : 0;

                    // Установить позицию на кнопке и завершить связь
                    SetMousePos(x, y + p10);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    if (i + 1 == s_i_counter)
                        break;

                    // Завершить проведение связи; отпустить ЛКМ
                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y + p10, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    // Переместить блок на 90px вниз
                    int px90 = 90;

                    SetMousePos(x, y + px90 + p10);

                    mouse_event(MOUSEEVENTF_LEFTUP, x, y + px90 + p10, 0, 0);

                    // Прокрутить правый скролл на 300px вниз
                    MoveScroll(false, true, -360, false);

                    // Вынужденная задержка
                    nSleep(_nsleep);
                }

                // Вернуть блок в начальное положение
                for (int i = 1; i < s_i_counter; ++i)
                {
                    SetMousePos(x, y + 5);
                    FocusMouseByPos(x, y + 5);

                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y + 5, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    SetMousePos(x, y - 90 + 5);

                    mouse_event(MOUSEEVENTF_LEFTUP, x, y - 90 + 5, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    MoveScroll(false, true, 360, false);

                    // Вынужденная задержка
                    nSleep(_nsleep);
                }

                // Сдвинуть блок немного вправо, чтобы дать место новому
                SetMousePos(x, y);

                {
                    // Вынужденная задержка
                    nSleep(_nsleep);

                    int mvl6065_1 = 60;
                    SetMousePos(x, y + 5);

                    // Обязательная задержка
                    Thread.Sleep(200);

                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y + 5, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    SetMousePos(x + mvl6065_1, y + 5);

                    mouse_event(MOUSEEVENTF_LEFTUP, x + mvl6065_1, y + 5, 0, 0);
                }

                // Вынужденная задержка
                nSleep(_nsleep);
            }

            // Сдвигаем блоки сложения вниз
            int mvl6065 = 60;
            int mvl6065y = 240;

            for (int j = 0; j < j_counter; ++j)
            {
                int mSdv = mtxCounter_shift--;  // mtxCounter_shift - количество оставшихся строк всех матриц
                for (int j2 = 0; j2 < mSdv; ++j2)
                {
                    // Устанавливаем позицию
                    SetMousePos(x + mvl6065, y + 5);

                    // Обязательная задержка
                    Thread.Sleep(300);

                    mouse_event(MOUSEEVENTF_LEFTDOWN, x + mvl6065, y + 5, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    SetMousePos(x + mvl6065, y + 5 + mvl6065y);

                    mouse_event(MOUSEEVENTF_LEFTUP, x + mvl6065, y + 5 + mvl6065y, 0, 0);

                    // Вынужденная задержка
                    nSleep(_nsleep);

                    MoveScroll(false, true, -mvl6065y * 4, false);
                }

                if(j_counter == 1)
                    ShiftElementToC(x + mvl6065, y + 5, x + mvl6065 + mvl6065, EPList[8].y + 5);

                MoveScroll(false, true, mvl6065y * 4 * mSdv, false);
            }
        }

        public void StepCreateCompdArithAnd(int counter_smtx)
        {
            // Функция создания блока мультиумножения блоков сложения
            // Создается один блок с количеством связей j_counter

            // Вынужденная задержка
            nSleep(_nsleep);

            // Установка "начальных" координат
            int x = EPList[8].x + 150 + (EPList[3].x - EPList[1].x);
            int y = EPList[8].y;

            x += 60;
            y += 5;

            int mvl6065 = 60;
            int mvl6065y = 240;

            if (j_counter == 1)
            {
                // Если идет работа со строковой матрицы, то мы просто сдвигаем блок сложения вперед
                // Создавать блок мультиумножения бессмысленно.
                // Сдвиг блока мультиумножения немного вверх

                /*nSleep(100);

                // Сброс фокуса
                SetMousePos(x + 100, y - 5);
                FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                //ShiftElementToC(x, y, x + mvl6065, EPList[8].y + 5);

                // Спуск на позицию
                SetMousePos(x, y);

                Thread.Sleep(5000);

                // Вынужденная задержка
                nSleep(_nsleep);

                mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);

                // Вынужденная задержка
                nSleep(_nsleep);

                SetMousePos(x + mvl6065, EPList[8].y + 5);

                Thread.Sleep(5000);

                mouse_event(MOUSEEVENTF_LEFTUP, x + mvl6065, EPList[8].y + 5, 0, 0);

                // Вынужденная задержка
                nSleep(_nsleep);*/
            }
            else
            {
                // Вставка элемента в указанную позицию.
                PasteElement(x + mvl6065, y);

                // Конкретно здесь проблема с Tool Palette:
                // Она становится неавтоматической, хотя должна быть автоматичной

                // Вынужденная задержка
                nSleep(_nsleep);

                // Установка размера //
                // Мультибоксу дается столько ячеек для входа, сколько существует блоков мультисложения
                // то есть кол_во_ячеек = j_counter
                ShiftElementToC(x + mvl6065, y + 4, x + mvl6065, y - 4 + j_counter * 8);

                for (int j = 0; j < j_counter; ++j)
                {
                    //-- Дальше идет проведение связи --//
                    // Проведение связи
                    // Сброс выделения блока
                    SetMousePos(x + mvl6065 + 100, y);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    // Установка позиции на вход блока мультиумножения
                    SetMousePos(x + mvl6065 - 8, y + 2 + j * 8);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    // Установка позиции на выход блока мультисложения
                    SetMousePos(x, y - 5);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    // Сдвиг скролла (без последнего сдвига)
                    if (j + 1 < j_counter)
                        SCCAA_sdv(x, y, mvl6065, mvl6065y);

                    Thread.Sleep(100);
                }

                // Сдвиг блока мультиумножения немного вверх
                ShiftElementToC(x + mvl6065, y + 5, x + mvl6065, EPList[8].y + 15);
            }

            // Дополнительный сдвиг для синтеза следующей матрицы
            MoveScroll(false, true, -mvl6065y * 4, false);
        }

        public void SCCAA_sdv(int x, int y, int mvl6065, int mvl6065y)
        {
            //
            // Функция сдвига поля и блока вниз для синтеза блоков мультисложения
            //

            // Спуск на позицию
            SetMousePos(x + mvl6065, y + 5);

            // Обязательная задержка
            Thread.Sleep(200);

            mouse_event(MOUSEEVENTF_LEFTDOWN, x + mvl6065, y + 5, 0, 0);

            // Вынужденная задержка
            nSleep(_nsleep);

            SetMousePos(x + mvl6065, y + 5 + mvl6065y);

            mouse_event(MOUSEEVENTF_LEFTUP, x + mvl6065, y + 5 + mvl6065y, 0, 0);

            // Вынужденная задержка
            nSleep(_nsleep);

            MoveScroll(false, true, -mvl6065y * 4, false);
        }

        public void StepCreateSubMatrixsWires(ListInfoMatrix lim, int[] suv)
        {
            //
            // Функция создания связей между подматрицами
            //

            // На выход функции поступает список подматриц и координаты всех подматриц
            // Примечание: список идет с конца.

            // Установка координат блока мультиумножения
            int x = EPList[8].x + 150 + (EPList[3].x - EPList[1].x) + 120;
            int y = EPList[8].y + 5;

            // Установка координат блока сложения
            int x_a = EPList[8].x + (EPList[3].x - EPList[1].x) + 150 + 120 + 60;
            int y_a = EPList[8].y + 5;

            // Установка координат блока умножения
            int x_m = x_a + 60;
            int y_m = y_a;

            // Установка координат для блока сложения/умножения на время его синтеза (проведения связей)
            int x_f = x_m + 60;
            int y_f = y_m;

            foreach (Submatrix sbmtx in lim._listSBMTX)
            {
                // Для начала необходимо определить, есть ли связь "блока сложения"
                // Для этого должны существовать матрицы: строковая и остаток строковой
                bool m0_has = false;
                if (sbmtx.sbM0 != null && (sbmtx.sbM1 != null || sbmtx.nSBM1 != -1))
                {
                    // Необходимо провести связь между строковой и остатка строковой матриц
                    m0_has = true;

                    // Синтез блока сложения

                    // Копируем блок сложения
                    SMCopyBSBU(10);

                    // Устанавливаем позицию scrollud в зависимости от sbM1 (или nSBM1)
                    // и проводим связь с этой матрицей.

                    // Возвращаемся в старт. позицию
                    SetStartPositionSCBUD();

                    // Проматываем скролл к необходимой матрице
                    if(sbmtx.sbM1 != null)
                    {
                        // Остаток строковой матрицы должен находиться на пункт выше
                        MoveScroll(false, true, -4 * 240 * (suv[lim.GetCount() - sbmtx.id - 1] - 2), false);

                        // Вставка элемента
                        PasteElement(x_f, y_f);

                        // Сброс фокуса после вставки
                        SetMousePos(x_f + 20, y_f);
                        FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                        // Мы находимся на месте остатка строковой матрицы. Теперь необходимо провести связь
                        ShiftElementToC(x_f - 8, y_f - 8, x, y);

                        // Теперь необходимо сдвинуть данный блок сложения к месту строковой матрицы
                        ShiftElementToC(x_f, y_f, x_f, y_f + 240);
                        MoveScroll(false, true, -240 * 4, false);
                    }
                    else
                    {
                        // В данном случае остаток строковой матрицы находится по данным координатам
                        MoveScroll(false, true, -4 * 240 * (suv[lim.GetCount() - sbmtx.nSBM1 - 1] - 1), false);

                        // Вставка элемента
                        PasteElement(x_f, y_f);

                        // Сброс фокуса после вставки
                        SetMousePos(x_f + 20, y_f);
                        FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                        // Мы находимся на месте остатка строковой матрицы. Теперь необходимо провести связь
                        // Стоит учесть, что данная матрица может имеет свой блок сложения/умножения.
                        ShiftElementToC(x_f - 8, y_f - 8, x_m, y_m);

                        // Теперь необходимо сдвинуть данный блок сложения к месту строковой матрицы
                        int f_sdv = suv[lim.GetCount() - sbmtx.id - 1] - suv[lim.GetCount() - sbmtx.nSBM1 - 1];

                        if (sbmtx.j_c1 == 0)
                            ++f_sdv;

                        for (int f = 0; f < f_sdv; ++f)
                        {
                            ShiftElementToC(x_f, y_f, x_f, y_f + 240);
                            MoveScroll(false, true, -240 * 4, false);
                        }
                    }

                    // Сброс фокуса
                    SetMousePos(x_f + 20, y_f);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    // Проводим вторую связь между блоком сложения и блоком мультиумножения строковой матрицы
                    ShiftElementToC(x_f - 8, y_f + 6, x, y);

                    // Сброс фокуса
                    SetMousePos(x_f + 20, y_f);
                    FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                    // Если отсутствует остаток главной матрицы, то блок сложения следует сдвинуть вправо
                    if (sbmtx.nSBM2 == -1 && sbmtx.sbM2 == null)
                    {
                        ShiftElementToC(x_f, y_f, x_m, y_m);
                        continue;
                    }                                            
                    else
                        // Перемещаем блок сложения на его место
                        ShiftElementToC(x_f, y_f, x_a, y_a);
                }

                // Теперь необходимо определить, существует ли остаток главной матрицы
                // И существует ли блок сложения, чтобы провести блок умножения
                // Если же блока умножения не существует, то блок сложения (если есть) сдвинуть на позицию блока умножения
                // (это чуть правее).
                if (m0_has)
                {
                    if (sbmtx.nSBM2 != -1 || sbmtx.sbM2 != null)
                    {
                        // Синтез блока умножения

                        // Копируем блок умножения
                        SMCopyBSBU(11);

                        // Возвращаемся в старт. позицию
                        SetStartPositionSCBUD();

                        if(sbmtx.sbM2 != null)
                        {
                            // Необходимо сделать расчет положения остатка главной матрицы и сдвинуть к ней скролл
                            // [позиция_данной_матрицы - 1 пункт - кол_во_строк_второй_мтц]
                            int sdv_mS = suv[lim.GetCount() - sbmtx.id - 1] - 2 - sbmtx.j_c1;
                            
                            if(sbmtx.j_c1 == 0)
                                ++sdv_mS;

                            MoveScroll(false, true, -4 * 240 * sdv_mS, false);

                            // Вставка элемента
                            PasteElement(x_f, y_f);

                            // Сброс фокуса после вставки
                            SetMousePos(x_f + 20, y_f);
                            FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                            ShiftElementToC(x_f - 8, y_f - 8, x, y);

                            // Теперь необходимо сдвинуть данный блок умножения к месту блока сложения
                            int f_sdv = sbmtx.j_c1 + 1;
                            for (int f = 0; f < f_sdv; ++f)
                            {
                                ShiftElementToC(x_f, y_f, x_f, y_f + 240);
                                MoveScroll(false, true, -240 * 4, false);
                            }
                        }
                        else
                        {
                            // Необходимо сделать расчет положения остатка главной матрицы и сдвинуть к ней скролл
                            int valueNSBM21PJC1C = suv[lim.GetCount() - sbmtx.nSBM2 - 1] - 1;
                            MoveScroll(false, true, -4 * 240 * valueNSBM21PJC1C, false);

                            // Вставка элемента
                            PasteElement(x_f, y_f);

                            // Сброс фокуса после вставки
                            SetMousePos(x_f + 20, y_f);
                            FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                            ShiftElementToC(x_f - 8, y_f - 8, x_m, y_m);

                            // Теперь необходимо сдвинуть данный блок умножения к месту блока сложения
                            int aValue = 0;

                            if (sbmtx.j_c1 == 0)
                                ++aValue;

                            int f_sdv = suv[lim.GetCount() - sbmtx.id - 1] - suv[lim.GetCount() - sbmtx.nSBM2 - 1] + aValue;
                            //MessageBox.Show(f_sdv.ToString());
                            for (int f = 0; f < f_sdv; ++f)
                            {
                                ShiftElementToC(x_f, y_f, x_f, y_f + 240);
                                MoveScroll(false, true, -240 * 4, false);
                            }
                        }

                        // Сброс фокуса
                        SetMousePos(x_f + 20, y_f);
                        FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

                        // Проводим вторую связь между блоком сложения и блоком мультиумножения строковой матрицы
                        ShiftElementToC(x_f - 8, y_f + 6, x_a, y_a);

                        // Перемещаем блок сложения на его место
                        ShiftElementToC(x_f, y_f, x_m, y_m);
                    }
                }
                else
                {
                    // Сдвиг блока мультиумножения на позицию блока умножения
                    MoveScroll(false, true, -4 * 240 * (suv[lim.GetCount() - sbmtx.id - 1] - 1), false);
                    
                    ShiftElementToC(x, y, x_m, y_m);

                    // Возвращаемся в старт. позицию
                    SetStartPositionSCBUD();
                }
            }
        }

        public void SMCopyBSBU(int block_id)
        {
            //
            // Функция правильного копирования блока сложения/умножения
            //

            // Последовательность копирования элемента:
            // 1) запомним позицию scrollud
            // 2) вернемся в стартовую позицию scroll
            // 3) скопируем нужный элемент
            // 4) вернемся в изначальный scrollud
            
            int n_scbud = scbud;

            SetStartPositionSCBUD();

            // Вынужденная задержка
            nSleep(_nsleep);

            // Скопировать элемент (10 - блок сложения, 11 - блок умножения)
            CopyElement(block_id);

            // Вынужденная задержка
            nSleep(_nsleep);

            MoveScroll(false, true, n_scbud, false);
        }

        public void SetStartPositionSCBUD()
        {
            // Функция возврата поля в стартовую позицию
            MoveScroll(false, true, -scbud, false);
        }

        public void ShiftElementToC(int old_x, int old_y, int new_x, int new_y)
        {
            // Спуск на позицию
            SetMousePos(old_x, old_y);

            // Вынужденная задержка
            nSleep(_nsleep);

            mouse_event(MOUSEEVENTF_LEFTDOWN, old_x, old_y, 0, 0);

            // Вынужденная задержка
            nSleep(_nsleep);

            SetMousePos(new_x, new_y);

            mouse_event(MOUSEEVENTF_LEFTUP, new_x, new_y, 0, 0);

            // Вынужденная задержка
            nSleep(_nsleep);
        }

        public void CopyElement(int element_id)
        {
            // * 4 - позиция btn - кнопки (для копирования)
            // * 5 - позиция not - инверсии
            // * 6 - позиция compd_arithmtc_or - блок мультисложения
            // * 7 - позиция compd_arithmtc_and - блок мультиумножения
            // * 10 - позиция блока сложения
            // * 11 - позиция блока умножения

            // Если форма преобразования KNF, то необходимо подменять element_id
            if (!dnf)
            {
                switch(element_id)
                {
                    case 6:   element_id = 7; break;
                    case 7:   element_id = 6; break;
                    case 10: element_id = 11; break;
                    case 11: element_id = 10; break;
                }
            }

            // Установить позицию для подготовки выделения элемента
            // Вычитается 20 пикселей для выделения

            SetMousePos(EPList[element_id].x, EPList[element_id].y + 20 * (element_id == 4 ? 1 : 0));

            //if (element_id == 4 || element_id == 5 || element_id == 6 || element_id == 7)
            //{
                // Зажимается кнопка мыши, переносится и отпускается в центре кнопки; элемент выделен
                SetMousePos(EPList[element_id].x, EPList[element_id].y + 20);
                mouse_event(MOUSEEVENTF_LEFTDOWN, EPList[element_id].x, EPList[element_id].y + 20, 0, 0);


                // Вынужденная задержка
                nSleep(_nsleep);

                //mouse_event(MOUSEEVENTF_MOVE, 5, me_value, 0, 0);
                SetMousePos(EPList[element_id].x + 1, EPList[element_id].y - 20);

                // Вынужденная задержка
                nSleep(_nsleep);

                mouse_event(MOUSEEVENTF_LEFTUP, EPList[element_id].x, EPList[element_id].y - 20, 0, 0);
            //}
            //else
            //    FocusMouseByPos(EPList[element_id].x, EPList[element_id].y);


            // Вынужденная задержка
            nSleep(_nsleep);


            // Нажатие CTRL+C для добавления элемента в буфер
            FCopyEl();


            // Вынужденная задержка
            nSleep(_nsleep);
        }

        public void CreateElement()
        {
            // В этой функции создается элемент, который ранее был скопирован
            // Элемент создается в той позиции, где на данный момент находится мышь
            // Перед создаем делается фокус (клик ЛКМ)
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;

            // Фокус мыши в позиции, где она находится
            FocusMouseByPos(x, y);

            // Вынужденная задержка
            nSleep(_nsleep);

            // Вставка элемента
            keybd_event(VK_LCTRL, 0, 0, 0);
            keybd_event(VK_V, 0, 0, 0);

            // Вынужденная задержка
            nSleep(_nsleep);
        }

        public void MoveScroll(bool lr, bool ud, int lenght, bool frontpanel)
        {
            // Функция передвигает позицию scroll на заданное расстояние.
            // lr - передвигается нижний scroll (влево-вправо) панели Front Panel и панели Block Diagram
            // ud - передвигается правый scroll (вверх-вниз) панели Front Panel и панели Block Diagram
            // lenght - расстояние сдвига

            // * 0 - позиция scroll_lr - нижнего scroll первого окна
            // * 1 - позиция scroll_ud - правого scroll первого окна
            // * 2 - позиция scroll_lr - нижнего scroll второго окна
            // * 3 - позиция scroll_ud - правого scroll второго окна

            int[] lulu = new int[2];
            lulu[0] = 0;
            lulu[1] = 0;

            if (lr)
            {
                lulu[0] = lenght;
                scblr += lenght;
            }

            if (ud)
            {
                lulu[1] = lenght;
                scbud += lenght;
            }

            for (int i = 0; i < 4; ++i)
            {
                if (lulu[i % 2] == 0)
                    continue;

                if (!frontpanel && i < 2)
                    continue;

                int x = EPList[i].x - (i % 2) * 40;
                int y = EPList[i].y - ((i + 1) % 2) * 40;

                // Установить позицию мыши
                SetMousePos(x, y);

                // Сделать фокус
                FocusMouseByPos(x, y);

                // Передвинуть мышь обратно на позицию scroll
                //mouse_event(MOUSEEVENTF_MOVE, (i % 2 == 0 ? 0 : 25), (i % 2 == 1 ? 0 : 25), 0, 0);
                SetMousePos(EPList[i].x, EPList[i].y);

                // Передвинуть колесико
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, lulu[i % 2], 0);

                // Вынужденная задержка
                nSleep(_nsleep);
            }
        }

        void SetMousePos(int x, int y)
        {
            // Установка мыши по заданным координатам x и y
            mForm.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(x, y);

            // Вынужденная задержка
            nSleep(_nsleep);
        }

        void FocusMouseByPos(int x, int y)
        {
            // Фокус мыши по заданным координатам x и y
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            // Вынужденная задержка
            nSleep(_nsleep);
        }

        void FCopyEl()
        {
            // Функция копирования
            keybd_event(VK_LCTRL, 0, 0, 0);
            keybd_event(VK_C, 0, 0, 0);

            keybd_event(VK_LCTRL, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, 0);
        }

                         // Позиции в момент вставки 
        void PasteElement(int x_now, int y_now)
        {
            nSleep(50);

            // Сбрасываем фокус
            SetMousePos(x_now + 30, y_now);
            FocusMouseByPos(Cursor.Position.X, Cursor.Position.Y);

            nSleep(50);

            // Выключаем automatic tool
            SetMousePos(EPList[9].x, EPList[9].y);
            FocusMouseByPos(EPList[9].x, EPList[9].y);

            nSleep(50);

            // Фокусимся обратно в исходную позицию
            SetMousePos(x_now, y_now);
            FocusMouseByPos(x_now, y_now);

            // Функция вставки
            {
                keybd_event(VK_LCTRL, 0, 0, 0);
                keybd_event(VK_V, 0, 0, 0);

                keybd_event(VK_LCTRL, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
            }

            nSleep(50);

            // Включаем automatic tool
            SetMousePos(EPList[9].x, EPList[9].y);
            FocusMouseByPos(EPList[9].x, EPList[9].y);

            nSleep(50);
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

        void nSleep(int t_sleep)
        {
            try
            {
                if (GetAsyncKeyState(VK_F3) != 0)
                {
                    MessageBox.Show("Аварийный выход из программы.");
                    Application.Exit();
                }
            }
            catch
            {

            }


            Thread.Sleep(_nsleep);
        }

        // Форма
        SynthesisLV mForm;

        //string[] msg_l;
        List<ElementPosition> EPList = new List<ElementPosition>();
    }
}
