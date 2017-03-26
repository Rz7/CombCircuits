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
    public partial class PreoF : Form
    {
        Form1 form1;
        
        Thread t_pMn;
        PreoManager pMn;

        // Матрица, полученная из формулы.
        string[] final_matrix;
        int counter_fm = 0;

        public PreoF()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void PreoF_Load(object sender, EventArgs e)
        {
        }

        private void PreoF_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Убить поток если есть
            KillThread();

            // Развернуть главное окно
            form1.WindowState = FormWindowState.Normal;
        }

        public void InitForm(Form1 f1)
        {
            form1 = f1;
        }

        public void radioButton1_MouseHover(object sender, EventArgs e)
        {
            label4.Text = "Д.Н.Ф. - дизъюнктивная нормальная форма";
        }

        private void radioButton2_MouseHover(object sender, EventArgs e)
        {
            label4.Text = "К.Н.Ф. - конъюнктивная нормальная форма";
        }

        private void radioButton1_MouseLeave(object sender, EventArgs e)
        {
            label4.Text = "";
        }

        private void radioButton2_MouseLeave(object sender, EventArgs e)
        {
            label4.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text_formula = "";
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(OFD.FileName))
                    {
                        text_formula = sr.ReadToEnd();
                    }
                }
                catch (Exception e_)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e_.Message);
                }
            }

            textBox1.Text = text_formula;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Обнуляем массив
            counter_fm = 0;

            KillThread();

            t_pMn = new Thread(StartThread);
            t_pMn.Start();

            //button3.Visible = true;
            //button4.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            KillThread();
            
            form1.OpenMMatrix();
            form1.GetMMatrix().SetMatrixTB(final_matrix, radioButton1.Checked);
        }

        public void SetBtn4Vis()
        {
            Action action = () => button4.Visible = true;
            if (InvokeRequired) Invoke(action); else action();
        }

        public void AddText(string var)
        {
            textBox1.Text = var;
        }

        public void SetFinalMTX(string f)
        {
            Array.Resize(ref final_matrix, counter_fm + 1);
            final_matrix[counter_fm++] = f;
        }

        private void StartThread()
        {
            pMn = new PreoManager(this, textBox1, button2, button4, radioButton1.Checked);
            pMn.GenMatrix();
        }

        private void KillThread()
        {
            try
            {
                t_pMn.Abort();
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
