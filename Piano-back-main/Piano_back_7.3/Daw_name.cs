using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piano_back_7._3
{
    internal class Daw_name
    {
        private string daw_name;
        private string DAW_name {
            get {
                return daw_name;
            }
            set
            {
                daw_name = value;
            }
        }
        public Daw_name(string daw_name) { 
        this.daw_name = daw_name;
        }
    }
}
