using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Symbol
    {
        public int size;
        public int offset;
        public int address;
        public string scope;
        public string symid { get; private set; }
        public string value { get; private set; }
        public string kind { get; private set; }
        public string[] data { get; private set; }
        public string parameters;
        public Symbol(string scope,string symid, string value, string kind, string[] data)
        {
            this.scope = scope;
            this.symid = symid;
            this.value = value;
            this.kind = kind;
            this.data = data;
            this.parameters = "";
            this.size = 4;
        }
        public void Print()
        {
            Console.Write("Symbole Table: " + scope + " " + symid +
             " " + value + " " + kind + " ");
            if (data == null)
            {
                Console.WriteLine();
                return;
            }
            foreach (string s in data)
            {
                Console.Write(s + " ");
            }
            Console.WriteLine();
        }
    }
}
