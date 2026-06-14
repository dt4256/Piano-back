using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;
using Piano_back_7._3;
using System.Runtime.InteropServices;
using System.IO.Ports;



namespace Piano_test
{
    

    
    public partial class Form1 : Form
    {

        private string port = "DawPort";
        private BTN activeBtn = null;
        public int chn = 0;
        private SerialPort EspSerial;
        private bool connected = false;
        private bool connection_event = false;
        private enum players{screen,phys};
        players play = players.screen;
        private int[] phch = new int[16];
        public int getch()
        {
            int curr = chn;
            chn = (chn + 1) % 16;
            if (chn == 9) ++chn;
            return curr;
        }

        private void btnConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EspSerial != null && EspSerial.IsOpen)
            {
                EspSerial.Close();
                EspSerial.Dispose();
                lblStatus.Text = "Disconnect";
                return;
            }

            try
            {
                EspSerial = new SerialPort(
                    comboBoxPorts.SelectedItem.ToString(),
                    baudRate: 9600,
                    Parity.None, 8, StopBits.One
                );
                EspSerial.DataReceived += SerialPort_DataReceived;
                EspSerial.Open();
                Thread.Sleep(2000);
                EspSerial.Write("connect?");

                connection_event = true;



                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия порта: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (connection_event == true) {
                    string ln = EspSerial.ReadLine().Trim();
                    Debug.WriteLine("RX: " + ln);
                    if (ln == "ardboardready")
                    {
                        connection_event= false;
                        lblStatus.Invoke((MethodInvoker)(() => lblStatus.Text = "Connected"));
                        connected = true;
                        return;
                    }
                }

                if (connected && play== players.phys)
                {
                    string line = EspSerial.ReadLine().Trim();
                    string[] comm = line.Split(',');
                    if (comm[0] == "on")
                    {
                        int temp = getch();
                        while (phch[temp]!=-1)temp = getch();
                        phch[temp] = Convert.ToInt32(comm[1]);
                        _midiDevice.SendEvent(new NoteOnEvent((SevenBitNumber)Convert.ToInt32(comm[1]), (SevenBitNumber)127) { Channel = (FourBitNumber)temp });
                        _midiDevice.SendEvent(new PitchBendEvent((ushort)Convert.ToInt32(comm[2])) { Channel = (FourBitNumber)temp });
                    }
                    else if (comm[0]== "pitch")
                    {
                        for(int i = 0; i < phch.Length; ++i)
                        {
                            if (phch[i] == Convert.ToInt32(comm[1]))
                            {
                                _midiDevice.SendEvent(new PitchBendEvent((ushort)Convert.ToInt32(comm[2])) { Channel = (FourBitNumber)i });
                            }
                        }
                    }
                    else if (comm[0] == "off")
                    {
                        for (int i = 0; i < phch.Length; ++i)
                        {
                            if (phch[i] == Convert.ToInt32(comm[1]))
                            {
                                _midiDevice.SendEvent(new NoteOffEvent((SevenBitNumber)Convert.ToInt32(comm[1]), (SevenBitNumber)127) { Channel = (FourBitNumber)i });
                                _midiDevice.SendEvent(new PitchBendEvent((ushort)(8192)) { Channel = (FourBitNumber)i });
                                phch[i] = -1;
                            }
                        }
                    }


                }
            }
            catch { }
        }

        

        private OutputDevice _midiDevice;
        public Form1()
        {
            
            InitializeComponent();
            Debug.WriteLine("OUTPUTS:");
            foreach (var d in OutputDevice.GetAll())
                Debug.WriteLine(d.Name);

            Debug.WriteLine("INPUTS:");
            foreach (var d in InputDevice.GetAll())
                Debug.WriteLine(d.Name);
        }

        List<BTN> btns = new List<BTN>();
        int number_of_channels = 1;
        private void Form1_Load(object sender, EventArgs e)
        {
            for(int i = 0; i < phch.Length; ++i)
            {
                phch[i] = -1;
            }
            try
            {
                comboBoxPorts.DropDownStyle = ComboBoxStyle.DropDownList;
                string[] ports = SerialPort.GetPortNames();
                comboBoxPorts.Items.AddRange(ports);
                if (comboBoxPorts.Items.Count > 0)
                    comboBoxPorts.SelectedIndex = 0;
                else
                {
                    comboBoxPorts.Items.Add("No Com ports");
                    comboBoxPorts.SelectedIndex = 0;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Com port error: {ex.Message}");
            }
            try
            {
                _midiDevice = OutputDevice.GetByName(port);
                _midiDevice.PrepareForEventsSending();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к MIDI: {ex.Message}");
            }
            int tonestart = 62; // для высоты ноты
            int xst = 100, yst = 100, dist = 0;
            string notes = "CDEFGAB";
            int[] statuses = { 1, 0, -1, 1, 0, 0, -1 };//главное начинать с до инициализацию
            for (int i = 0; i < 15; i++)
            {
                btns.Add(new BTN(xst, yst, tonestart + i, statuses[i % 7], notes[(i + 3) % 7]));
                Console.WriteLine(btns[i]);
                xst += btns[i].Width;
                xst += dist;
            }
        }


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Pen big = new Pen(Color.Black, 5);
            Pen oct_3 = new Pen(Color.ForestGreen, 5);
            Pen oct_4 = new Pen(Color.Orange, 5);
            Pen oct_5 = new Pen(Color.Blue, 5);
            for (int i = 0; i < btns.Count; i++)
            {
                btns[i].Draw(e.Graphics);
                if (i != 0)
                    e.Graphics.DrawLine(big, btns[i].X, btns[i].Y, btns[i].X, btns[i].Y + btns[i].Height);
                if (i < 4)
                {
                    e.Graphics.DrawLine(oct_3, btns[i].X + Convert.ToInt64(btns[i].Width*0.25), btns[i].Y, btns[i].X + Convert.ToInt64(btns[i].Width * 0.75), btns[i].Y);
                }
                if (i > 3 && i < 11)
                {
                    e.Graphics.DrawLine(oct_4, btns[i].X + Convert.ToInt64(btns[i].Width * 0.25), btns[i].Y, btns[i].X + Convert.ToInt64(btns[i].Width * 0.75), btns[i].Y);
                }
                if (i > 10)
                {
                    e.Graphics.DrawLine(oct_5, btns[i].X + Convert.ToInt64(btns[i].Width * 0.25), btns[i].Y, btns[i].X + Convert.ToInt64(btns[i].Width * 0.75), btns[i].Y);
                }
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (number_of_channels == 1 && play == players.screen)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                BTN hovered = null;
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
                }
            }
            if (number_of_channels == 2)
            {


            }
            }


            private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (number_of_channels == 1 && play == players.screen)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                BTN hovered = null;
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
                        _midiDevice.SendEvent(new PitchBendEvent((ushort)(activeBtn.Pitchband)) { Channel = (FourBitNumber)activeBtn.Chanel });
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

                        var noteOn = new NoteOnEvent((SevenBitNumber)(activeBtn.Hight), (SevenBitNumber)127) { Channel = (FourBitNumber)activeBtn.Chanel };
                        _midiDevice.SendEvent(noteOn);

                        var pitchBend = new PitchBendEvent((ushort)(activeBtn.Pitchband)) { Channel = (FourBitNumber)activeBtn.Chanel };
                        _midiDevice.SendEvent(pitchBend);

                        var volume = new ControlChangeEvent((SevenBitNumber)(activeBtn.Chanel), (SevenBitNumber)100) { Channel = (FourBitNumber)activeBtn.Chanel };
                        _midiDevice.SendEvent(volume);
                    }
                    else
                    {
                        var pitchBend = new PitchBendEvent((ushort)activeBtn.Pitchband) { Channel = (FourBitNumber)activeBtn.Chanel };
                        _midiDevice.SendEvent(pitchBend);
                    }
                }
            }
            if (number_of_channels == 2)
            {

            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }





        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (number_of_channels == 1 && play == players.screen)
            {

                for (int i = 0; i < btns.Count; i++)
                {
                    btns[i].Pitchband = 8192;
                    if (btns[i].Chanel != -1)
                    {
                        Console.WriteLine(Convert.ToString(i));
                        var noteOff = new NoteOffEvent((SevenBitNumber)btns[i].Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)btns[i].Chanel };
                
                        Console.WriteLine(noteOff);
                        _midiDevice.SendEvent(noteOff);
                        Console.WriteLine("end");

                        var pitchBend = new PitchBendEvent((ushort)btns[i].Pitchband) { Channel = (FourBitNumber)btns[i].Chanel };
                        _midiDevice.SendEvent(pitchBend);

                    }
                    btns[i].Chanel = -1;

                }
            }
            if (number_of_channels == 2)
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (number_of_channels == 1) {
                _midiDevice?.Dispose();
            }
            if (number_of_channels == 2)
            {
                _midiDevice?.Dispose();
            }
            EspSerial?.Close();
            EspSerial?.Dispose();

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void oneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            number_of_channels = 1;
        }

        private void twoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            number_of_channels = 2;
        }

        private void chanelsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form_rename = new Form2();
            int x = 10, y = 10;
            var inputDevices = InputDevice.GetAll();
            List<string> names = new List<string>();
            foreach (var device in inputDevices)
            {
                names.Add(device.Name);
                Button bt = new Button();
                bt.Text = device.Name;
                bt.Location = new Point(x, y);
                y += 30;
                bt.Click += on_bt_click;
                form_rename.Controls.Add(bt);
            }

            form_rename.ShowDialog();

        }
        private void on_bt_click(object sender, EventArgs e) 
        { 
            Button b1 = (Button)sender;
            port = b1.Text;
            _midiDevice = OutputDevice.GetByName(port);
            _midiDevice?.PrepareForEventsSending();
        }

        private void portToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'f')
            {
                if (play == players.screen)
                {
                    for (int i = 0; i < btns.Count; i++)
                    {
                        btns[i].Pitchband = 8192;
                        if (btns[i].Chanel != -1)
                        {
                            Console.WriteLine(Convert.ToString(i));
                            var noteOff = new NoteOffEvent((SevenBitNumber)btns[i].Hight, (SevenBitNumber)127) { Channel = (FourBitNumber)btns[i].Chanel };

                            Console.WriteLine(noteOff);
                            _midiDevice.SendEvent(noteOff);
                            Console.WriteLine("end");

                            var pitchBend = new PitchBendEvent((ushort)btns[i].Pitchband) { Channel = (FourBitNumber)btns[i].Chanel };
                            _midiDevice.SendEvent(pitchBend);

                        }
                        btns[i].Chanel = -1;

                    }
                }

            }
            
        }

        private void whoPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void screenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            play = players.screen;
            physicalToolStripMenuItem.Checked = false;
            screenToolStripMenuItem.Checked = true;
            for(int i = 0; i<btns.Count;++i)
            {
                btns[i].Chanel = -1; btns[i].Pitchband = 8192;
            }
            for(int i = 0; i < 16; ++i)
            {
                if (phch[i] >= 0)
                {
                    _midiDevice.SendEvent(new NoteOffEvent((SevenBitNumber)phch[i], (SevenBitNumber)0) { Channel = (FourBitNumber)i });
                    _midiDevice.SendEvent(new PitchBendEvent(8192) { Channel = (FourBitNumber)i });
                }
                phch[i] = -1;
            }
        }

        private void physicalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            play = players.phys;
            physicalToolStripMenuItem.Checked = true;
            screenToolStripMenuItem.Checked = false;
            for (int i = 0; i < 16; ++i) phch[i] = -1;

            for (int i = 0; i < btns.Count; ++i) {
                if (btns[i].Chanel >= 0)
                {
                    _midiDevice.SendEvent(new NoteOffEvent((SevenBitNumber)btns[i].Hight, (SevenBitNumber)0) { Channel = (FourBitNumber)btns[i].Chanel });
                    _midiDevice.SendEvent(new PitchBendEvent(8192) { Channel = (FourBitNumber)btns[i].Chanel });
                }
            }
        }    
    
    }
}
