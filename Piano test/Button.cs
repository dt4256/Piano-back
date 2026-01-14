using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Piano_test
{
    class BTN
    {
        private int hight,pitchband,pressure;
        private int config;//просто про расположение черных клавиш рядом(заморочка с pitch band) где 0 слева справа есть черные  клавиши(pitch band не резать) -1  черная клавиша только слева 1 черная клавиша только справа
        private char note;
        private bool pressured;
        private int x, y;
        private int power = 127;
        private int width, height;

        public bool Pressured
        {
            get { return pressured; }
            set { pressured = value; }
        }
        public int Width
        {
            get { return width; }
        }
        public char Note
        {
            get { return note; }
        }
        public int Pitchband
        {
            get { return pitchband; }
        }
        public BTN(int x,int y, int hight, int config, char note)
        {
            this.x= x; this.y = y;
            this.config = config;
            this.hight = hight;
            this.note = note;
            pitchband = 8192;
            pressure = 0;
            width = 100;
            height = 200;
        }

        public void Draw(Graphics g)
        {
            Pen clr = new Pen(Color.Black);
            g.DrawLine(clr, x, y, x + width, y);
            g.DrawLine(clr, x, y, x, y + height);
            g.DrawLine(clr, x + width, y, x + width, y + height);
            if (config == 0) g.DrawLine(clr, x, y + height, x + width, y + height);
            if (config == -1) g.DrawLine(new Pen(Color.Blue), x, y + height, x + width, y + height);
            if (config == 1) g.DrawLine(new Pen(Color.Red), x, y + height, x + width, y + height);
        }

        public bool isInside(int x, int y) {
            if(x>=this.x && x <= this.x + width)
            {
                if(y>=this.y && y <= this.y + height)
                {
                    return true;
                }
            }
            return false;
        }
        private int Map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
             return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }
        public void change_pitchband(int x)
        {
            pitchband = Map(x-this.x, 0, width, 0, 16383);
        }
        
    }
}
