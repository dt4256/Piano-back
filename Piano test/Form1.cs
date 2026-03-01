using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;



namespace Piano_test
{

    public partial class Form1 : Form
    {
        private BTN? activeBtn = null;
        //private BTN? activeBtn_2 = null;
        public int chn = 0;
        public int getch()
        {
            int curr = chn;
            chn = (chn + 1) % 16;
            if (chn == 9) ++chn;
            return curr;
        }
        private OutputDevice? _midiDevice;
        //private OutputDevice? _midiDevice_second;
        public Form1()
        {
            InitializeComponent();
            try
            {
                _midiDevice = OutputDevice.GetByName("DawPort");
                _midiDevice.PrepareForEventsSending();/*
                _midiDevice_second = OutputDevice.GetByName("DawPort_2");
                _midiDevice_second.PrepareForEventsSending();
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к MIDI: {ex.Message}");
            }
        }

        List<BTN> btns = new List<BTN>();
        private void Form1_Load(object sender, EventArgs e)
        {
            int tonestart = 2;
            int xst = 100, yst = 100, dist = 0;
            string notes = "CDEFGAB";
            int[] statuses = { 1, 0, -1, 1, 0, 0, -1 };//главное начинать с до инициализацию
            for (int i = 0; i < 15; i++)
            {
                btns.Add(new BTN(xst, yst, tonestart + i, statuses[i % 7], notes[(i+3) % 7]));
                xst += btns[i].Width;
                xst += dist;
            }
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Pen big = new Pen(Color.Black, 10);
            Pen oct_3 = new Pen(Color.ForestGreen, 5);
            Pen oct_4 = new Pen(Color.Orange, 5);
            Pen oct_5 = new Pen(Color.Blue, 5);
            for (int i = 0; i < btns.Count; i++)
            {
                btns[i].Draw(e.Graphics);
                if (i == 1 || i == 2 || i == 3 || i == 5 || i == 6 || i == 8 || i == 9|| i == 10 || i == 12 || i == 13|| i == 15)
                    e.Graphics.DrawLine(big, btns[i].X, btns[i].Y, btns[i].X, btns[i].Y + 200);
                if (i < 4)
                {
                    e.Graphics.DrawLine(oct_3, btns[i].X + 25, btns[i].Y, btns[i].X + 75, btns[i].Y);
                }
                if (i > 3 && i < 11)
                {
                    e.Graphics.DrawLine(oct_4, btns[i].X + 25, btns[i].Y, btns[i].X + 75, btns[i].Y);
                }
                if (i > 10)
                {
                    e.Graphics.DrawLine(oct_5, btns[i].X + 25, btns[i].Y, btns[i].X + 75, btns[i].Y);
                }
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            BTN? hovered = null;
            //BTN? hovered_2 = null;
            int hoveredIndex = -1;
            //int hoveredIndex_2 = -1;

            for (int i = 0; i < btns.Count; i++)
            {
                if (btns[i].isInside(e.X, e.Y))
                {
                    hovered = btns[i];
                    hoveredIndex = i;
                    /*
                    if (hovered != null)
                    {
                        hovered_2 = btns[i];
                        hoveredIndex_2 = i;
                        break;
                    }
                    */
                    break;
                }
            }

            if (hovered == null)
                return;
            activeBtn = hovered;

            activeBtn.change_pitchband(e.X);
            label1.Text = Convert.ToString(hoveredIndex) + activeBtn.Note + Convert.ToString(activeBtn.Pitchband);

            if (activeBtn.Chanel == -1)
            {
                activeBtn.Chanel = getch();
                _midiDevice.SendEvent(new NoteOnEvent((SevenBitNumber)activeBtn.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn.Chanel });
                _midiDevice.SendEvent(new PitchBendEvent((ushort)activeBtn.Pitchband) { Channel = (FourBitNumber)activeBtn.Chanel });
                _midiDevice.SendEvent(new ControlChangeEvent((SevenBitNumber)activeBtn.Hight, (SevenBitNumber)100) { Channel = (FourBitNumber)activeBtn.Chanel });
            }
            /*
            if (hovered_2 != null)
            {
                activeBtn_2 = hovered_2;
                activeBtn_2.change_pitchband(e.X);
                label2.Text = Convert.ToString(hoveredIndex_2) + activeBtn_2.Note + Convert.ToString(activeBtn_2.Pitchband);
                if (activeBtn.Chanel == -1)
                {
                    activeBtn.Chanel = getch();
                    _midiDevice_second.SendEvent(new NoteOnEvent((SevenBitNumber)activeBtn_2.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn_2.Chanel });
                    _midiDevice_second.SendEvent(new PitchBendEvent((ushort)activeBtn_2.Pitchband) { Channel = (FourBitNumber)activeBtn_2.Chanel });
                    _midiDevice_second.SendEvent(new ControlChangeEvent((SevenBitNumber)activeBtn_2.Hight, (SevenBitNumber)100) { Channel = (FourBitNumber)activeBtn_2.Chanel });
                }
            }
            */
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            BTN? hovered = null;
            //BTN? hovered_2 = null;
            int hoveredIndex = -1;
            //int hoveredIndex_2 = -1;
            for (int i = 0; i < btns.Count; i++)
            {
                if (btns[i].isInside(e.X, e.Y))
                {
                    hovered = btns[i];
                    hoveredIndex = i;
                    /*
                    if (hovered != null)
                    {
                        hovered_2 = btns[i];
                        hoveredIndex_2 = i;
                        break;
                    }
                    */
                    break;
                }
            }
            if (hovered != activeBtn)
            {
                if (activeBtn != null)
                {
                    activeBtn.Pitchband = 8192;
                    _midiDevice.SendEvent(new NoteOffEvent((SevenBitNumber)activeBtn.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn.Chanel });
                    _midiDevice.SendEvent(new PitchBendEvent((ushort)activeBtn.Pitchband) { Channel = (FourBitNumber)activeBtn.Chanel });
                    activeBtn.Chanel = -1;
                }
                activeBtn = hovered;
            }
            /*
            if (hovered_2 != null) {
                if (hovered_2 != activeBtn_2)
                {
                    if (activeBtn != null)
                    {
                        activeBtn_2.Pitchband = 8192;
                        _midiDevice_second.SendEvent(new NoteOffEvent((SevenBitNumber)activeBtn_2.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn_2.Chanel });
                        _midiDevice_second.SendEvent(new PitchBendEvent((ushort)activeBtn_2.Pitchband) { Channel = (FourBitNumber)activeBtn_2.Chanel });
                        activeBtn_2.Chanel = -1;
                    }
                    activeBtn_2 = hovered_2;
                }
            }
            */

            if (activeBtn != null)
            {
                activeBtn.change_pitchband(e.X);
                label1.Text = Convert.ToString(hoveredIndex) + activeBtn.Note + Convert.ToString(activeBtn.Pitchband);

                if (activeBtn.Chanel == -1)
                {
                    activeBtn.Chanel = getch();

                    var noteOn = new NoteOnEvent((SevenBitNumber)activeBtn.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(noteOn);

                    var pitchBend = new PitchBendEvent((ushort)activeBtn.Pitchband) { Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(pitchBend);

                    var volume = new ControlChangeEvent((SevenBitNumber)activeBtn.Chanel, (SevenBitNumber)100) { Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(volume);
                }
                else
                {
                    var pitchBend = new PitchBendEvent((ushort)activeBtn.Pitchband) { Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(pitchBend);
                }
            }
            /*
            if (activeBtn_2 != null)
            {
                activeBtn_2.change_pitchband(e.X);
                label2.Text = Convert.ToString(hoveredIndex) + activeBtn_2.Note + Convert.ToString(activeBtn_2.Pitchband);

                if (activeBtn_2.Chanel == -1)
                {
                    activeBtn_2.Chanel = getch();

                    var noteOn = new NoteOnEvent((SevenBitNumber)activeBtn_2.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn_2.Chanel };
                    _midiDevice_second.SendEvent(noteOn);

                    var pitchBend = new PitchBendEvent((ushort)activeBtn_2.Pitchband) { Channel = (FourBitNumber)activeBtn_2.Chanel };
                    _midiDevice_second.SendEvent(pitchBend);

                    var volume = new ControlChangeEvent((SevenBitNumber)activeBtn_2.Chanel, (SevenBitNumber)100) { Channel = (FourBitNumber)activeBtn_2.Chanel };
                    _midiDevice_second.SendEvent(volume);
                }
                else
                {
                    var pitchBend = new PitchBendEvent((ushort)activeBtn_2.Pitchband) { Channel = (FourBitNumber)activeBtn_2.Chanel };
                    _midiDevice_second.SendEvent(pitchBend);
                }
            }
            */
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }





        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < btns.Count; i++)
            {
                btns[i].Pitchband = 8192;
                if (btns[i].Chanel != -1)
                {
                    var noteOff = new NoteOffEvent((SevenBitNumber)btns[i].Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)btns[i].Chanel };
                    _midiDevice.SendEvent(noteOff);
                    //_midiDevice_second.SendEvent(noteOff);
                    var pitchBend = new PitchBendEvent((ushort)btns[i].Pitchband) { Channel = (FourBitNumber)btns[i].Chanel };
                    _midiDevice.SendEvent(pitchBend);
                    //_midiDevice_second.SendEvent(pitchBend);
                }
                btns[i].Chanel = -1;

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _midiDevice?.Dispose();
            //_midiDevice_second?.Dispose();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
