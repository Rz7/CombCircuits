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
    public partial class SynthesisLV : Form
    {
        // Главная форма
        Form1 form1;

        // Менеджер работы синтеза схем в LabView
        public ManagerLVCreator mLvCreator;

        // Структура матриц
        string filename_struct = "";

        public SynthesisLV()
        {
            InitializeComponent();
        }

        private void SynthesisLV_Load(object sender, EventArgs e)
        {
            // Инициализация менеджера синтеза схем в LabView
            mLvCreator = new ManagerLVCreator(this, 500);
        }

        private void SynthesisLV_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Развернуть главное окно
            form1.WindowState = FormWindowState.Normal;
        }

        public void InitForm(Form1 f1)
        {
            form1 = f1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //
            // Функция загрузки структуры подматриц.
            //

            // Узнаем имя файла
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Файлы AutoDNF | *.ADNF";
            if (OFD.ShowDialog() == DialogResult.OK)
                filename_struct = OFD.FileName;

            // Здесь находится файл со структурами
            // filename_struct;

            // Если существует файл структур - синтезируем структуры
            if (!mLvCreator.LoadSubMatrixs(filename_struct))
                return;

            MessageBox.Show("Данные успешно загружены.");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string text_dotsXY = "";
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(OFD.FileName))
                    {
                        text_dotsXY = sr.ReadToEnd();
                    }
                }
                catch (Exception e_)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e_.Message);
                }
            }

            // Здесь лежат нерасшифрованные координаты точек
            if(mLvCreator != null)
            {
                mLvCreator.InitElements(text_dotsXY);
            }
            else
            {
                MessageBox.Show("Возникла непредвиденая ошибка.");
            }
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            label4.Text = "Структура создается после минимизации матрицы.";
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            label4.Text = "";
        }

        private void button4_MouseHover(object sender, EventArgs e)
        {
            label4.Text = "Файл точек содержит информацию о координатах экрана (X, Y)," + Environment.NewLine
                          + "где расположены те или иные элементы системы LabView." + Environment.NewLine
                          + "Они нужны для синтеза схемы.";
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            label4.Text = "";
        }

        private void label3_MouseHover(object sender, EventArgs e)
        {
            label4.Text = "Каждое действие - передвижение мыши, клик, нажатие клавиш клавиатуры." + Environment.NewLine
                            + "Так как система выполняет это все программно, необходимо вручную" + Environment.NewLine
                            + "указать задержку. Она зависит от отклика окон Windows.";
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label4.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Старт синтеза
            //try
            //{
                // Определение задержки
                int sleep = 500; try { sleep = Convert.ToInt32(textBox1.Text); }
                catch { }

                // Инициализация менеджера синтеза схем в LabView
                mLvCreator._nsleep = sleep;

                // Запуск процесса
                mLvCreator.Start();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Ошибка: сбой в синтезе." + Environment.NewLine + ex.Message);
            //}            
        }
    }
}
