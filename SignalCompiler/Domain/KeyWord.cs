using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCompiler.Domain
{
    //Words reserved by system
    public class KeyWord
    {
        public KeyWord()
        {
            keyWord = "";
            id = 301;
        }
        public KeyWord(string word, int id)
        {
            keyWord = word;
            this.id = id;
        }
        public string keyWord { get; set; }
        public int id { get; set; }
    }
}
