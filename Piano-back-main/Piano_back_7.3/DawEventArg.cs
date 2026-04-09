using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piano_back_7._3
{
    public class DawEventArg : EventArgs
    {
        string name;    
        public DawEventArg(string name )
        {
            this.name = name;
        }
    }
}
