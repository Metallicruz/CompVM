using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Token
    {
        private const int SIZE = 3;
        private string[] tokens;
        public enum Type { Number, Character, BoolSymbol, Identifier, Punctuation, 
		Keyword, TypeKeyword, MathSymbol, Symbol, EOF,Comment, Unknown };
        
        public Type type { get; private set; }
        public int lineNumber { get; private set; }
        public String lexeme { get; private set; }
        public Token(Type type, String lexeme, int lineNumber)
        {
            tokens = new string[SIZE];
            this.type = type;
            this.lexeme = lexeme;
            this.lineNumber = lineNumber;
            tokens[0] = Enum.GetName(typeof(Type), type);
            tokens[1] = lineNumber.ToString();
            tokens[2] = lexeme;
        }

        public string[] GetTokenInfo()
        { 
            return tokens;
        }
    }
}
