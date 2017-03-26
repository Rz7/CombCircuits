using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SynthesisOfDM
{
    public partial class MMatrix : Form
    {
        // Главная форма
        Form1 form1;

        // Класс для работы с матрицами
        MMatrixOpt mmOpt;

        // Менеджер работы с подматрицами
        ManagerMSMOpt managerMSMO;

        // Форма анализа структур подматриц
        AnalyseSubMTXF asMtxF;

        // Отображение матриц
        int[] listElementsId;
        string[] arrayMtxText;
        int counterAMT = 0;

        public MMatrix()
        {
            InitializeComponent();
        }

        private void MMatrix_Load(object sender, EventArgs e)
        {
            
        }

        private void MMatrix_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Развернуть главное окно
            form1.WindowState = FormWindowState.Normal;
        }

        public void InitForm(Form1 f1)
        {
            form1 = f1;
        }

        public void SetMatrixTB(string[] tb_t, bool dnf)
        {
            radioButton1.Checked = dnf;
            radioButton2.Checked = !dnf;

            Array.Resize(ref listElementsId, tb_t.Length);
            arrayMtxText = tb_t;
            counterAMT = tb_t.Length - 1;            

            if(counterAMT > 0)
            {
                label3.Visible = true;
                button6.Visible = true;
                button7.Visible = true;
            }

            counterAMT = 0;
            UpdateShowMtx(counterAMT);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text_matrix = "";
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(OFD.FileName))
                    {
                        text_matrix = sr.ReadToEnd();
                    }
                }
                catch (Exception e_)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e_.Message);
                }
            }

            textBox1.Text = text_matrix;
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "Файлы AutoDNF | *.ADNF";

            if (SFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Сохраняем лист
                    if (mmOpt != null)
                        managerMSMO.SaveAllSMatrix(SFD.FileName, mmOpt.dnf); // TODO: сделать норм
                }
                catch { }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            form1.OpenLabViewCreator();

            try
            {
                form1.GetSLVF().mLvCreator.LoadSubMatrixs(mmOpt.dnf, managerMSMO.GetLim());
            }
            catch { }
        }

        private void button5_Click(object sender, EventArgs e)
        {           
            Thread th_mmtx = new Thread(() => { ThreadMinMtx(); });
            th_mmtx.Start();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (--counterAMT <= 0)
                counterAMT = 0;

            UpdateShowMtx(counterAMT);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (++counterAMT + 1 >= arrayMtxText.Length)
                counterAMT = arrayMtxText.Length - 1;

            UpdateShowMtx(counterAMT);
        }

        public void UpdateShowMtx(int index)
        {
            SetBtnEnb(button6, (index > 0));
            SetBtnEnb(button7, (index + 1 < arrayMtxText.Length));

            SetTextBoxText(textBox1, arrayMtxText[index]);
            SetLabelText(label3, (index + 1).ToString() + "/" + arrayMtxText.Length);
        }

        public void ThreadMinMtx()
        {
            //
            // Процесс упрощения матрицы
            //
            mmOpt = new MMatrixOpt();

            // Форма преобразования (по умолчанию - dnf)
            mmOpt.dnf = true;

            mmOpt.bres1 = checkBox1.Checked;
            mmOpt.bres2 = checkBox1.Checked;
            mmOpt.bres3 = checkBox1.Checked;
            mmOpt.bres4 = checkBox1.Checked;

            //
            // Процесс деления на схемы
            //
            managerMSMO = new ManagerMSMOpt();

            // Создаем лист подматриц
            managerMSMO.CreateListSubMTX();

            // Далее необходимо:
            // 1) вычислить форму преобразования
            // 2) расшифровать все матрицы (разделены двойным пробелом)

            // Определяем 
            mmOpt.dnf = radioButton1.Checked;
            
            if(arrayMtxText == null || arrayMtxText.Length == 0)
            {
                string tbT = textBox1.Text;
                bool f_was = false;
                int counter_MTX = 0;                // Количество матриц
                string[] arrayMTX = new string[1];  // Массив с матрицами

                for (int i = 0; i < tbT.Length; ++i)
                {
                    if (tbT[i] == ' ')
                        continue;

                    if (tbT[i] == ' ')
                        continue;

                    if (i > 1)
                    {
                        if (tbT[i - 1].ToString() == "N" && tbT[i].ToString() == "F")
                        {
                            if (tbT[i - 2].ToString() == "D")
                                mmOpt.dnf = true;

                            if (tbT[i - 2].ToString() == "K")
                                mmOpt.dnf = false;
                        }
                    }

                    if (i > 1 && tbT[i - 2].ToString() == "\r" && tbT[i - 1].ToString() == "\n" && tbT[i].ToString() == "\r")
                    {
                        if (f_was)
                        {
                            Array.Resize(ref arrayMTX, 1 + ++counter_MTX);
                            f_was = false;
                        }

                        continue;
                    }
                    else
                    {
                        if (i + 1 == tbT.Length)
                        {
                            arrayMTX[counter_MTX++] += tbT[i].ToString();
                            continue;
                        }
                    }

                    if (tbT[i].ToString() == "\n" || tbT[i].ToString() == "\r")
                        continue;

                    // Записываем формулу в массив
                    arrayMTX[counter_MTX] += tbT[i].ToString();

                    if (i + 1 < tbT.Length && tbT[i + 1].ToString() == "\r")
                        arrayMTX[counter_MTX] += Environment.NewLine;

                    // Предполагается, что была записана формула
                    f_was = true;
                }

                arrayMtxText = arrayMTX;
                listElementsId = new int[arrayMtxText.Length];

                if(counter_MTX > 0)
                {
                    SetLabelVis(label3, true);
                    SetBtnVis(button6, true);
                    SetBtnVis(button7, true);
                }
            }

            for (int i = 0; i < arrayMtxText.Length; ++i)
            {
                SetTextBoxText(textBox1, mmOpt.StartProcess(arrayMtxText[i]));

                counterAMT = i;
                arrayMtxText[counterAMT] = textBox1.Text;
                UpdateShowMtx(counterAMT);

                // Проверка на правильность инициализации матрицы
                bool astop = false;

                try
                {
                    if (mmOpt.result4 != "")
                        astop = managerMSMO.InitMatrix(this, mmOpt.result4);
                    else
                        astop = managerMSMO.InitMatrix(this, arrayMtxText[i]);
                }
                catch
                {
                    astop = managerMSMO.InitMatrix(this, arrayMtxText[i]);
                }

                if (!astop)
                {
                    MessageBox.Show("Возникла ошибка в загрузке матрицы.");
                    return;
                }

                // Генерируем подматрицы
                listElementsId[counterAMT] = managerMSMO.GenSubMatrixs();
            }

            MessageBox.Show("Процесс деления окончен." + Environment.NewLine
                               + "Количество матриц: " + managerMSMO.GetCountMatrix().ToString());

            // Отобразить кнопки для дальнейших действий
            SetBtnVis(button2, true);
            SetBtnVis(button3, true);
            SetBtnVis(button4, true);

            SetBtnSize(button5, (new Size(98, 40)));

            arrayMtxText = null;
        }

        public void SetBtnVis(Button btn, bool vis)
        {
            Action action = () => btn.Visible = vis;
            if (InvokeRequired) Invoke(action); else action();
        }

        public void SetBtnEnb(Button btn, bool vis)
        {
            Action action = () => btn.Enabled = vis;
            if (InvokeRequired) Invoke(action); else action();
        }

        public void SetBtnSize(Button btn, Size sz)
        {
            Action action = () => btn.Size = sz;
            if (InvokeRequired) Invoke(action); else action();
        }

        public void SetTextBoxText(TextBox tb, string txt)
        {
            Action action = () => tb.Text = txt;
            if (InvokeRequired) Invoke(action); else action();
        }

        public void SetLabelText(Label lb, string txt)
        {
            Action action = () => lb.Text = txt;
            if (InvokeRequired) Invoke(action); else action();
        }

        public void SetLabelVis(Label lb, bool vis)
        {
            Action action = () => lb.Visible = vis;
            if (InvokeRequired) Invoke(action); else action();
        }

        private void button3_MouseHover(object sender, EventArgs e)
        {
            label2.Text = "Сохранить структуру упрощенной матрицы.";
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            label2.Text = "";
        }

        private void button4_MouseHover(object sender, EventArgs e)
        {
            label2.Text = "Синтезировать структуру упрощенной матрицы.";
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            label2.Text = "";
        }

        private void button5_MouseHover(object sender, EventArgs e)
        {
            label2.Text = "Минимизация схемы.";
        }

        private void button5_MouseLeave(object sender, EventArgs e)
        {
            label2.Text = "";
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            label2.Text = "Анализировать структуру подматриц.";
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            label2.Text = "";
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (managerMSMO.GetLim() == null)
                return;

            if (managerMSMO.GetLim().GetElementById(listElementsId[counterAMT]) == null)
                return;

            asMtxF = new AnalyseSubMTXF();
            asMtxF.SetParentForm();
            asMtxF.SetLim(managerMSMO.GetLim());
            asMtxF.SetSMTX(managerMSMO.GetLim().GetElementById(listElementsId[counterAMT]));
            asMtxF.Show();
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
    }
}
