using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCompiler.Domain
{
    // infp about constant
    public class Constant
    {
        public Constant()
        {
            value = 0;
            id = 501;
        }
        public Constant(int value, int id = 501)
        {
            this.value = value;
            this.id = id;
        }
        public int value { get; set; }
        public int id { get; set; }
    }
}
