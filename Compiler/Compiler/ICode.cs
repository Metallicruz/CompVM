using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    static class ICode
    {
        public const string SKIPIF = "SKIP_";
        public const string SKIPELSE = "SKIP_";
        public const string ENDWHILE = "ENDWHILE_";
        public const string BEGIN = "BEGIN_";
        public static int labelCounter = 1;
        private const string CHAR_TYPE = ".BYT";
        private const string INT_TYPE = ".INT";
        private const string COMMENT_FORMAT = "  ;------ ";
        private static int staticInitIndex = 0;
        private static string previousLine;
        private static string _filename;
        private static string bufferedLine = "";
        private static StreamWriter sw = null;
        private static List<string> staticInits = new List<string>();
        private static List<string> QuadList = new List<string>();
        private static Stack<string> stackIf = new Stack<string>();
        private static Stack<string> stackElse = new Stack<string>();
        private static Stack<string> stackWhile = new Stack<string>();
        private static Stack<string> stackEndWhile = new Stack<string>();
        public static string[] _instructions = new string[]
        {
            "ADD", "ADI", "SUB", "MUL", "DIV", "LT", "GT", "NE", "EQ", "LE", "GE", "AND", "OR", "BF", "BT",
            "JMP", "PUSH", "POP", "PEEK", "FRAME", "CALL", "RTN", "RETURN", "FUNC", "NEWI", "NEW", "MOV", "MOVI",
            "WRITE", "READ","CONVERT", "WRTC", "WRTI", "RDC", "RDI", "REF", "AEF"
        };
        public enum Instructions
        {
            ADD, ADI, SUB, MUL, DIV, LT, GT, NE, EQ, LE, GE, AND, OR, BF, BT, JMP, PUSH, POP, PEEK, FRAME,
            CALL, RTN, RETURN, FUNC, NEWI, NEW, MOV, MOVI, WRITE, READ, CONVERT, WRTC, WRTI, RDC, RDI, REF, AEF
        };
        public static string StackIf()
        {
            return stackIf.Pop();
        }
        public static string StackElse()
        {
            return stackElse.Pop();
        }
        public static string StackWhile()
        {
            return stackWhile.Pop();
        }
        public static string StackEndWhile()
        {
            return stackEndWhile.Pop();
        }
        public static void Print(string s)
        {
            BackPatch(s);
            bufferedLine += s;
        }
        public static void Flush()
        {
            sw = new StreamWriter(_filename, false);
            for (int i = 0; i < QuadList.Count; ++i)
            {
                sw.WriteLine(QuadList[i]);
            }
            sw.Flush();
            sw.Close();
            TCode.Start(ref QuadList, _filename);
        }
        public static void BackPatch(string s)
        {
            string[] tokens;
            bool labelsFound=false;
            bufferedLine=bufferedLine.Trim();
            s=s.Trim();
            if (bufferedLine == "")
            {
                return;
            }
            for (int i = 0; i < QuadList.Count; ++i)
            {
                QuadList[i]=QuadList[i].Trim();
                tokens = QuadList[i].Split(' ');
                for(int j=0; j < tokens.Length;++j)
                {
                    if (tokens[j] == bufferedLine)
                    {
                        labelsFound = true;
                        tokens[j] = s;
                    }
                }
                if (labelsFound)
                {
                    QuadList[i] = string.Join(" ", tokens);
                }
                labelsFound = false;
            }
            bufferedLine = "";
        }
        
        public static void StackIf(string s)
        {
            stackIf.Push(s);
        }
        public static void StackElse(string s)
        {
            stackElse.Push(s);
        }
        public static void StackWhile(string s)
        {
            stackWhile.Push(s);
        }
        public static void StackEndWhile(string s)
        {
            stackEndWhile.Push(s);
        }
        public static void StaticInit()
        {
            int count=0;
            string constructor;
            for(int i = 0; i < Parser.memberVariables.Count; ++i)
            {
                ++count;
                if (Parser.memberVariables[i][0] == 'X')
                {
                    constructor = Parser.memberVariables[i];
                    QuadList.Add( "Y" + constructor.Substring(1) + " FUNC " + "Y" + constructor.Substring(1));
                    break;
                }
            }
            for (int i = 0; i < count; ++i)
            {
                Parser.memberVariables.RemoveAt(0);
            }
        }
        public static void StaticInitInsertVars()
        {
            for(int i = staticInitIndex+1; i < QuadList.Count; ++i)
            {
                if (QuadList[i][0] == 'Y')
                {
                    staticInitIndex = i;
                    break;
                }
            }
            for (int i = staticInits.Count-1; i >=0 ; --i)
            {
                QuadList.Insert(staticInitIndex + 1, staticInits[i]);
            }
            staticInits.Clear();
        }

        public static void SetFilename(string filename)
        {
            _filename = filename;
            if (_filename == null || _filename == "")
            {
                _filename = "ICode.pxi";
            }
            _filename = _filename.Split('.')[0];
            _filename += ".iCode";
            foreach(Symbol s in SymbolTable.GetSymbolsInScope("g"))
            {
                if (s.symid[0] == 'H')
                {
                    if (s.symid.Length == 1)
                    {
                        OutputGlobal(s.symid, CHAR_TYPE, " ");
                    }
                    else
                    {
                        OutputGlobal(s.symid, CHAR_TYPE, s.symid.Substring(1, (s.symid.Length - 1)));
                    }
                }
                else if (s.symid[0] == 'N' || s.symid[0] == 'B')
                {
                    OutputGlobal(s.symid, INT_TYPE, s.value);
                }
            }
            FRAME("MAIN", "NULL");
            CALL("MAIN");
            JMP("END");//after globals are declared, jump to main
        }

        public static void ADD(string a, string b, string c)
        {
            Output(Instructions.ADD, a, b, c);
        }

        public static void ADI(string a, string b)
        {
            Output(Instructions.ADI, a, b);
        }

        public static void SUB(string a, string b, string c)
        {
            Output(Instructions.SUB, a, b, c);
        }

        public static void MUL(string a, string b, string c)
        {
            Output(Instructions.MUL, a, b, c);
        }

        public static void DIV(string a, string b, string c)
        {
            Output(Instructions.DIV, a, b, c);
        }

        public static void LT(string a, string b, string c)
        {
            Output(Instructions.LT, a, b, c);
        }

        public static void GT(string a, string b, string c)
        {
            Output(Instructions.GT, a, b, c);
        }

        public static void NE(string a, string b, string c)
        {
            Output(Instructions.NE, a, b, c);
        }

        public static void EQ(string a, string b, string c)
        {
            Output(Instructions.EQ, a, b, c);
        }

        public static void LE(string a, string b, string c)
        {
            Output(Instructions.LE, a, b, c);
        }

        public static void GE(string a, string b, string c)
        {
            Output(Instructions.GE, a, b, c);
        }

        public static void AND(string a, string b, string c)
        {
            Output(Instructions.AND, a, b, c);
        }

        public static void OR(string a, string b, string c)
        {
            Output(Instructions.OR, a, b, c);
        }

        public static void BF(string a,string b)
        {
            Output(Instructions.BF, a, b);
        }

        public static void BT(string a, string b)
        {
            Output(Instructions.BT, a, b);
        }

        public static void JMP(string a)
        {
            Output(Instructions.JMP, a);
        }

        public static void PUSH(string a)
        {
            Output(Instructions.PUSH, a);
        }

        public static void POP(string a)
        {
            Output(Instructions.POP, a);
        }

        public static void PEEK(string a)
        {
            Output(Instructions.PEEK, a);
        }

        public static void FRAME(string a, string b)
        {
            Output(Instructions.FRAME, a, b);
        }

        public static void CALL(string a)
        {
            Output(Instructions.CALL, a);
        }

        public static void RTN()
        {
            string value = QuadList.Last<string>().Split(' ')[0];
            if (value != "RTN" && value != "RETURN")
            {
                Output(Instructions.RTN);
            }
            else
            {
                int index = QuadList.Count-1;
                QuadList[index] = bufferedLine + QuadList[index];
                bufferedLine = "";
            }
        }

        public static void RETURN(string a)
        {
            string value = QuadList.Last<string>().Split(' ')[0];
            if (value != "RTN" && value != "RETURN")
            {
                Output(Instructions.RETURN, a);
            }
            else
            {
                int index = QuadList.Count - 1;
                QuadList[index] = bufferedLine + QuadList[index];
                bufferedLine = "";
            }
        }

        public static void FUNC(string a)
        {
            Output(a, Instructions.FUNC, a);
        }

        public static void NEWI(string a, string b)
        {
            Output(Instructions.NEWI, a, b);
        }

        public static void NEW(string a, string b)
        {
            Output(Instructions.NEW, a, b);
        }

        public static void MOV(string a, string b)
        {
            Output(Instructions.MOV, a, b);
        }

        public static void MOVI(string a, string b)
        {
            Output(Instructions.MOVI, a, b);
        }

        public static void WRITE(string a)
        {
            Output(Instructions.WRITE, a);
        }

        public static void READ(string a)
        {
            Output(Instructions.READ, a);
        }

        public static void CONVERT(string a)
        {
            Output(Instructions.CONVERT, a);
        }

        public static void REF(string a, string b, string c)
        {
            Output(Instructions.REF, a, b, c);
        }

        public static void AEF(string a, string b, string c)
        {
            Output(Instructions.AEF, a, b, c);
        }

        private static void Output(Instructions instruction)
        {
            QuadList.Add(bufferedLine+_instructions[(int)instruction]);
            bufferedLine = "";
            //sw.WriteLine(_instructions[(int)instruction]);
            //sw.Flush();
        }
        private static void Output(Instructions instruction, string a)
        {
            QuadList.Add(bufferedLine + _instructions[(int)instruction] + " " + a + ScannerLine());
            bufferedLine = "";
            //sw.WriteLine(_instructions[(int)instruction] + " " + a);
            //sw.Flush();
        }
        private static void Output(Instructions instruction, string a, string b)
        {
            QuadList.Add(bufferedLine + _instructions[(int)instruction] + " " + a + " " + b + ScannerLine());
            bufferedLine = "";
            //sw.WriteLine(_instructions[(int)instruction]+" " + a + ", " + b);
            //sw.Flush();
        }
        private static void Output(string a, Instructions instruction, string b)
        {
            QuadList.Add(bufferedLine + a + " " + _instructions[(int)instruction] + " " +b + ScannerLine());
            bufferedLine = "";
            //sw.WriteLine(_instructions[(int)instruction]+" " + a + ", " + b);
            //sw.Flush();
        }
        private static void Output(Instructions instruction, string a, string b,string c)
        {
            QuadList.Add(bufferedLine + _instructions[(int)instruction] + " " + a + " " + b + " " + c+ScannerLine());
            bufferedLine = "";
            //sw.WriteLine(_instructions[(int)instruction]+" " + a + ", " + b + ", " + c);
            //sw.Flush();
        }
        private static void OutputGlobal(string symid, string type, string value)
        {
            QuadList.Add(symid + " " + type + " " + value);
            bufferedLine = "";
        }
        private static string ScannerLine()
        {
            if (previousLine != Scanner.currentLine)
            {
                previousLine = Scanner.currentLine;
                return (COMMENT_FORMAT + Scanner.currentLine.Split(';')[0]+ COMMENT_FORMAT + Scanner.count);
            }
            previousLine = Scanner.currentLine;
            return "";
        }
    }
}
