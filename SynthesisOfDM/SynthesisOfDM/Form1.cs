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
    public partial class Form1 : Form
    {
        PreoF preoF;
        MMatrix mmtxF;
        SynthesisLV slvF;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        
        public PreoF GetPreoF() { return preoF; }
        public MMatrix GetMMatrix() { return mmtxF; }
        public SynthesisLV GetSLVF() { return slvF; }

        private void button2_Click(object sender, EventArgs e) { OpenPreoF(); this.WindowState = FormWindowState.Minimized; }
        private void button3_Click(object sender, EventArgs e) { OpenMMatrix(); this.WindowState = FormWindowState.Minimized; }
        private void button4_Click(object sender, EventArgs e) { OpenLabViewCreator(); this.WindowState = FormWindowState.Minimized; }

        public void OpenPreoF()
        {
            try
            {
                preoF.Hide();
                preoF.Show();
            }
            catch
            {
                preoF = new PreoF();
                preoF.Show();
            }

            preoF.InitForm(this);
        }

        public void OpenMMatrix()
        {
            try
            {
                mmtxF.Hide();
                mmtxF.Show();
            }
            catch
            {
                mmtxF = new MMatrix();
                mmtxF.Show();
            }

            mmtxF.InitForm(this);
        }

        public void OpenLabViewCreator()
        {
            try
            {
                slvF.Hide();
                slvF.Show();
            }
            catch
            {
                slvF = new SynthesisLV();
                slvF.Show();
            }

            slvF.InitForm(this);
        }
    }
}
