﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCompiler.Domain
{
    //class for error information
    class Error
    {
        public Error()
        {
            this.message = "";
            this.row = 0;
            this.pos = 0;
        }
        public Error(string message, int row, int pos)
        {
            this.message = message;
            this.row = row;
            this.pos = pos;
        }
        public string message { get; set; }
        public int row { get; set; }
        public int pos { get; set; } // symbol position
    }
}
