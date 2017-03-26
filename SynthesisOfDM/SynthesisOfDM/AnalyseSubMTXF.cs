using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SynthesisOfDM
{
    public partial class AnalyseSubMTXF : Form
    {
        // Структура подматрицы, с которой данная форма работает
        Submatrix sbmtx;

        // Лист структур
        ListInfoMatrix lim;

        // Родительская форма
        AnalyseSubMTXF asMtxF_parent;

        // Форма остатка строковой матрицы
        AnalyseSubMTXF asMtxF_1;

        // Форма остатка главной матрицы
        AnalyseSubMTXF asMtxF_2;

        public AnalyseSubMTXF()
        {
            InitializeComponent();
        }

        private void AnalyseSubMTXF_Load(object sender, EventArgs e)
        {

        }

        public void SetLim(ListInfoMatrix _lim)
        {
            lim = _lim;
        }

        public void SetParentForm()
        {
            asMtxF_parent = null;
        }

        public void SetParentForm(AnalyseSubMTXF asMtxf)
        {
            // Установка родительской формы
            asMtxF_parent = asMtxf;
            button1.Visible = true;
        }

        public void SetSMTX(Submatrix _sbmtx)
        {
            //
            // В данной функции из структуры берутся данные и записываются в интерфейс программы
            //

            // Получаем нашу структуру
            sbmtx = _sbmtx;

            if (sbmtx == null)
            {
                MessageBox.Show("Возникла ошибка в инициализации структуры.");
                return;
            }                

            if (sbmtx.i_c0 > 0 && sbmtx.sbM0 != null)
            {
                for (int i = 0; i < sbmtx.i_c0; ++i)
                {
                    string v = sbmtx.sbM0[i].ToString();

                    if (v == "2")
                        v = "_";

                    textBox1.Text += v;
                }

                button4.Visible = true;
            }
            else
            {
                // Отсутствие матрицы
                textBox1.Text = "Матрица отсутствует.";
            }

            if (sbmtx.j_c1 > 0 && sbmtx.i_c1 > 0 && sbmtx.sbM1 != null)
            {
                for (int j = 0; j < sbmtx.j_c1; ++j)
                {
                    for (int i = 0; i < sbmtx.i_c1; ++i)
                    {
                        string v = sbmtx.sbM1[j][i].ToString();

                        if (v == "2")
                            v = "_";

                        textBox2.Text += v;
                    }

                    textBox2.Text += Environment.NewLine;
                }

                button5.Visible = true;
            }
            else
            {
                if(sbmtx.nSBM1 != -1)
                {
                    // Отсутствие матрицы
                    textBox2.Text = "Матрица разделена на подматрицы.";
                    button2.Visible = true;
                }
                else
                {
                    // Отсутствие матрицы
                    textBox2.Text = "Матрица отсутствует.";
                }
            }

            if (sbmtx.j_c2 > 0 && sbmtx.i_c2 > 0 && sbmtx.sbM2 != null)
            {
                for (int j = 0; j < sbmtx.j_c2; ++j)
                {
                    for (int i = 0; i < sbmtx.i_c2; ++i)
                    {
                        string v = sbmtx.sbM2[j][i].ToString();

                        if (v == "2")
                            v = "_";

                        textBox3.Text += v;
                    }

                    textBox3.Text += Environment.NewLine;
                }

                button6.Visible = true;
            }
            else
            {
                if (sbmtx.nSBM2 != -1)
                {
                    // Отсутствие матрицы
                    textBox3.Text = "Матрица разделена на подматрицы.";
                    button3.Visible = true;
                }
                else
                {
                    // Отсутствие матрицы
                    textBox3.Text = "Матрица отсутствует.";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(asMtxF_parent != null)
            {
                this.Hide();
                asMtxF_parent.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lim == null)
                return;

            if (sbmtx.nSBM1 == -1)
                return;

            if(asMtxF_1 == null)
            {
                asMtxF_1 = new AnalyseSubMTXF();
                asMtxF_1.SetParentForm(this);
                asMtxF_1.SetLim(lim);
                asMtxF_1.SetSMTX(lim.GetElementById(sbmtx.nSBM1));
            }

            if (asMtxF_1 != null)
                asMtxF_1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (lim == null)
                return;

            if (sbmtx.nSBM2 == -1)
                return;

            if (asMtxF_2 == null)
            {
                asMtxF_2 = new AnalyseSubMTXF();
                asMtxF_2.SetParentForm(this);
                asMtxF_2.SetLim(lim);
                asMtxF_2.SetSMTX(lim.GetElementById(sbmtx.nSBM2));
            }

            asMtxF_2.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!CheckMatrixToICounter(textBox1.Text))
                return;

            for(int i = 0; i < textBox1.Text.Length; ++i)
            {
                if(i > sbmtx.i_c0)
                    return;

                string v = textBox1.Text[i].ToString();

                if (v == Environment.NewLine)
                    return;

                if (v == "_")
                    v = "2";

                sbmtx.sbM0[i] = Convert.ToByte(v);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!CheckMatrixToICounter(textBox2.Text))
                return;

            int j_c = 0;
            int i_c = 0;
            for (int i = 0; i < textBox2.Text.Length; ++i)
            {
                string v = textBox2.Text[i].ToString();

                if (i > 0 && CanConvertToInt(textBox2.Text[i - 1].ToString()) && v == "\r")
                {
                    i_c = 0;
                    ++j_c;
                    continue;
                }

                if (v == "_")
                    v = "2";

                if (CanConvertToInt(v)) 
                    sbmtx.sbM1[j_c][i_c++] = Convert.ToByte(v);
            }

            if (CanConvertToInt(textBox3.Text[textBox3.Text.Length - 1].ToString()))
                ++j_c;

            sbmtx.j_c1 = j_c;
            MessageBox.Show("Матрица обновлена.");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!CheckMatrixToICounter(textBox3.Text))
                return;

            int j_c = 0;
            int i_c = 0;
            for (int i = 0; i < textBox3.Text.Length; ++i)
            {
                string v = textBox3.Text[i].ToString();

                if (i > 0 && CanConvertToInt(textBox3.Text[i - 1].ToString()) && v == "\r")
                {
                    i_c = 0;
                    ++j_c;
                    continue;
                }

                if (v == "_")
                    v = "2";

                if (CanConvertToInt(v))
                    sbmtx.sbM2[j_c][i_c++] = Convert.ToByte(v);
            }

            if (CanConvertToInt(textBox3.Text[textBox3.Text.Length - 1].ToString()))
                ++j_c;

            sbmtx.j_c2 = j_c;
            MessageBox.Show("Матрица обновлена.");
        }

        private bool CheckMatrixToICounter(string sb)
        {
            int i_c = 0;
            for(int i = 0; i < sb.Length; ++i)
            {
                if (sb[i].ToString() == "\r")
                    i_c = 0;
                else
                {
                    if (i_c > sbmtx.i_c0 && sbmtx.sbM0 != null || i_c > sbmtx.i_c1 && sbmtx.sbM1 != null || i_c > sbmtx.i_c2 && sbmtx.sbM2 != null)
                        return false;
                    ++i_c;
                }
            }

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
    }
}
