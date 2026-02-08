using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Threading;



namespace Piano_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<BTN> btns = new List<BTN>();
        private void Form1_Load(object sender, EventArgs e)
        {
            int tonestart = 2;
            int xst = 100, yst = 100, dist =  0;
            string notes = "CDEFGAB";
            int[] statuses = { 1, 0, -1, 1, 0, 0, -1 };//главное начинать с до инициализацию
            for (int i = 0; i < 15; i++)
            {
                btns.Add(new BTN(xst, yst, tonestart + i, statuses[i % 7], notes[i % 7]));
                xst += btns[i].Width;
                xst += dist;
            }
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < btns.Count; i++)
            {
                btns[i].Draw(e.Graphics);
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

                for (int i = 0; i < btns.Count; i++)
                {
                    if (btns[i].isInside(e.X, e.Y))
                    {
                        btns[i].change_pitchband(e.X);
                        btns[i].Pressured = true;
                        label1.Text = Convert.ToString(i) + btns[i].Note + Convert.ToString(btns[i].Pitchband);
                    }
                }
           
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < btns.Count; i++)
            {
                if (btns[i].isInside(e.X, e.Y) && e.Button == MouseButtons.Left)
                {
                    btns[i].change_pitchband(e.X);
                    label1.Text = Convert.ToString(i) + btns[i].Note + Convert.ToString(btns[i].Pitchband);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            for(int i = 0; i < btns.Count; i++)
            {
                btns[i].Pressured = false;
            }
        }



       

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            for(int i = 0;i< btns.Count; i++)
            {
                if (btns[i].Pressured) btns[i].Pressured = false;
            }
        }
    }
}
