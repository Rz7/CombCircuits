using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace SynthesisOfDM
{
    class PreoManager
    {
        //
        // Менеджер преобразований формул в матрицы
        //

        // Класс работы с формулой
        PreoFTMTX pFTMTX;

        // Окно программы
        PreoF pForm;

        // Главное текстовое окно
        TextBox tb1;

        // Кнопка "Преобразовать в матрицу"
        Button btn2;

        // Кнопка "Минимизация"
        Button btn4;

        // Тип формы преобразования (true - dnf, false - knf)
        bool dnf;

        public PreoManager(PreoF _pForm, TextBox _tb1, Button _btn2, Button _btn4, bool _dnf)
        {
            pForm = _pForm;

            tb1 = _tb1;

            btn2 = _btn2;

            btn4 = _btn4;

            dnf = _dnf;
        }

        public void GenMatrix()
        {
            //
            // Функция генерации матриц
            //

            // Все матрицы, созданные на основе формул (списком)
            string listMatrix = "";

            if (dnf)
                listMatrix += "DNF";
            else
                listMatrix += "KNF";
            listMatrix += Environment.NewLine + Environment.NewLine;

            // Инициализируем класс работы с формулами
            pFTMTX = new PreoFTMTX(pForm, dnf);

            // Дальше необходимо считать все формулы, введенные в окне программы
            bool f_was = false;                     // Формула была
            int counter_formulas = 0;               // Количество формул
            string[] arrayFormulas = new string[1]; // Массив с формулами
            for (int i = 0; i < tb1.Text.Length; ++i)
            {
                if(tb1.Text[i] == ' ')
                    continue;

                if(tb1.Text[i] == ' ')
                    continue;

                if (tb1.Text[i].ToString() == "\r")
                    continue;

                if (tb1.Text[i].ToString() == Environment.NewLine)
                    continue;

                if (tb1.Text[i] == ';')
                {
                    if(f_was)
                    {
                        Array.Resize(ref arrayFormulas, 1 + ++counter_formulas);
                        f_was = false;
                    }
                    
                    continue;
                }
                else
                {
                    if (i + 1 == tb1.Text.Length)
                    {
                        arrayFormulas[counter_formulas++] += tb1.Text[i].ToString();
                        continue;
                    }
                        
                }

                // Записываем формулу в массив
                arrayFormulas[counter_formulas] += tb1.Text[i].ToString();

                // Предполагается, что была записана формула
                f_was = true;
            }

            // Опустошаем все
            pFTMTX.ResetAll();

            // Количество переменных во всех формулах
            int ppv = 0;

            // Вычислим максимальное кол-во переменных
            foreach (string formula in arrayFormulas)
            {
                if (formula == null)
                    continue;

                pFTMTX.AddTVar(formula, ref ppv);

                // Опустошаем все
                pFTMTX.ResetAllWOT();
            }            

            // Анализируем каждую формулу и превращаем ее в матрицу
            foreach(string formula in arrayFormulas)
            {
                if (formula == null)
                    continue;

                // Загружаем формулу
                pFTMTX.AddVar(formula, ppv);

                if(!pFTMTX.CheckFormlTr())
                {
                    MessageBox.Show("Формула введена с ошибками. Расчет остановлен.", "Ошибка заполнения формулы",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!pFTMTX.Preo_obr())
                    return;

                // Получаем матрицу
                string result = pFTMTX.GenMat();

                if (result != "")
                {
                    listMatrix += result + Environment.NewLine;

                    // Добавляем лист матриц в память формы
                    pForm.SetFinalMTX(result);
                }

                // Сбрасываем результаты для синтеза следующей матрицы
                // Опустошаем все, за исключением t_var (переменные)
                pFTMTX.ResetAllWOT();
            }

            // Добавляем лист матриц в текстовое окно
            pForm.AddText("Список матриц:" + Environment.NewLine + Environment.NewLine + listMatrix);

            // Сделать видимой кнопку "минимизация"
            pForm.SetBtn4Vis();            

            MessageBox.Show("Все матрицы успешно сгенерированы.");
        }
    }
}
