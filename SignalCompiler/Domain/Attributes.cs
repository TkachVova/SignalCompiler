using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCompiler.Domain
{
    //info about symbols
    public class Attributes
    {
        public Attributes()
        {
            symbol = '\0'; // default
            type = -1; // default
        }
        public char symbol { get; set; }
        public int type { get; set; }
    }
}
