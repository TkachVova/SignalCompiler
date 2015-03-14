using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SignalCompiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LexicalAnalizer lexicalAnalizer = new LexicalAnalizer();
            string path = System.IO.Directory.GetCurrentDirectory() + @"\test.txt";
            List<LexicalAnalizerOutput> result = lexicalAnalizer.MakeLexemLine(path);
            foreach (var item in result)
            {
                Debug.Print("Lexem: {0}\tCode: {1}", item.lexem, item.code);
            }
            Debug.Print("\n");
            if (lexicalAnalizer.errors.Count() > 0)
            {
                foreach (var item in lexicalAnalizer.errors)
                {
                    Debug.Print(item.message + " in row {0}, position {1}", item.row.ToString(), item.pos.ToString());
                }
            }
        }
    }
}
