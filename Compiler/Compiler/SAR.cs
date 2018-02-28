using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SAR
    {
        public enum SARtype { Identifier, Literal, Type, Ref, Function, BAL, AL }
        public SARtype sarType;
        public int size;
        public string symid;
        public string value;
        public string scope;
        public string dataType;
        public string paramType;
        public string classType;
        public string tempVar;
        public List<string> argList;
        public Token token;
        public SAR(SARtype sarType, Token token, string value, string scope)
        {
            this.sarType = sarType;
            this.value = value;
            this.scope = scope;
            this.token = token;
            classType = "";
            dataType="";
            paramType="";
            symid = "";
            tempVar = "";
            argList = new List<string>();
        }
    }
}
