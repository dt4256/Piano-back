using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;



namespace Piano_test
{

    public partial class Form1 : Form
    {
        private BTN? activeBtn = null;
        public int chn = 0;
        public int getch()
        {
            int curr = chn;
            chn = (chn + 1) % 16;
            if (chn == 9) ++chn;
            return curr;
        }
        private OutputDevice? _midiDevice;
        public Form1()
        {
            InitializeComponent();
            try
            {
                _midiDevice = OutputDevice.GetByName("DawPort");
                _midiDevice.PrepareForEventsSending();

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
            if (e.Button != MouseButtons.Left)
                return;

            BTN? hovered = null;
            int hoveredIndex = -1;

            for (int i = 0; i < btns.Count; i++)
            {
                if (btns[i].isInside(e.X, e.Y))
                {
                    hovered = btns[i];
                    hoveredIndex = i;
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
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            BTN? hovered = null;
            int hoveredIndex = -1;
            for (int i = 0; i < btns.Count; i++)
            {
                if (btns[i].isInside(e.X, e.Y))
                {
                    hovered = btns[i];
                    hoveredIndex = i;
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


            if (activeBtn != null)
            {
                activeBtn.change_pitchband(e.X);
                label1.Text = Convert.ToString(hoveredIndex) + activeBtn.Note + Convert.ToString(activeBtn.Pitchband);

                if (activeBtn.Chanel == -1)
                {
                    activeBtn.Chanel = getch();

                    var noteOn = new NoteOnEvent((SevenBitNumber)activeBtn.Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(noteOn);

                    var pitchBend = new PitchBendEvent((ushort)activeBtn.Pitchband){ Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(pitchBend);

                    var volume = new ControlChangeEvent((SevenBitNumber)activeBtn.Chanel,(SevenBitNumber)100){ Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(volume);
                }
                else
                {
                    var pitchBend = new PitchBendEvent((ushort)activeBtn.Pitchband){ Channel = (FourBitNumber)activeBtn.Chanel };
                    _midiDevice.SendEvent(pitchBend);
                }
            }
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
                    var pitchBend = new PitchBendEvent((ushort)btns[i].Pitchband) { Channel = (FourBitNumber)btns[i].Chanel };
                    _midiDevice.SendEvent(pitchBend);
                }
                btns[i].Chanel = -1;
                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _midiDevice?.Dispose();
        }
    }
}
