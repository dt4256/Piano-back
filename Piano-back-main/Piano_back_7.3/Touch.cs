using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piano_back_7._3
{
    internal class Touch
    {
        private int x;
        private int y;
        public int X 
        { 
            get 
            { 
                return x; 
            } 
            set 
            {  
                x = value; 
            }
        }
        public int Y 
        { 
        get
            {
                return y;
            }
            set 
            { 
                y = value;
            }
        }
        public Touch(int x, int y) 
        { 
            this.x = x;
            this.y = y;
        }
    }
}
