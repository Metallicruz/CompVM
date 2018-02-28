using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Compiler
{
    static class TCode
    {
        private static int pc = 0;
        private const int THIS_OFFSET = 8;
        private const int VAR_SIZE = 4;
        private const int INSTRUCT_SIZE = VAR_SIZE*3;
        private const string EXIT_SUCCESS = "EXIT_SUCCESS";
        private const string SIZE_INT = "SIZE_OF_INT";
        private const string FALSE = "N0";
        private const string TRUE = "N1";
        private const string NEGATIVE = "NEGATIVE";
        private const string OVERFLOW = "OVERFLOW";
        private const string UNDERFLOW = "UNDERFLOW";
        private const string FREE = "FREE";
        private const string END = "END";
        private const string COMMENT_FORMAT = "  ;----- ";
        private static StreamWriter sw;
        private static List<string> _iCode;
        private static string bufferedLine;
        private static string comment = "";
        enum OpCode { JMP, JMR, BNZ, BGT, BLT, BRZ, MOV, LDA, STR, LDR, STB, LDB, ADD, ADI, SUB, MUL, DIV, AND, OR, CMP, TRP, STRI, LDRI, STBI, LDBI };
        enum Register { False, True, OP1, OP2, VAR_ADDRESS, R5, PFP, This, PC, SL, SP, FP, SB };
        private static string[] registers = { "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "PC", "SL", "SP", "FP", "SB" };
        private static string[] OpCodes = { "JMP", "JMR", "BNZ", "BGT", "BLT", "BRZ", "MOV", "LDA", "STR", "LDR", "STB", "LDB", "ADD", "ADI", "SUB", "MUL", "DIV", "AND", "OR", "CMP", "TRP" };
        private static string[] _instructions =
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



        public static void Start(ref List<string> iCode, string filename)
        {
            int labelOffset = 0;
            string operand1 = "";
            string operand2 = "";
            string operand3 = "";
            string instruction;
            string[] line;
            string[] comments;
            string[] data = new string[2];
            data[0] = "returnType:int";
            data[1] = "accessMod:public";
            string[] dataChar = new string[2];
            dataChar[0] = "returnType:char";
            dataChar[1] = "accessMod:public";
            SymbolTable.Add(new Symbol("g", TRUE, "1", "ilit", data));
            SymbolTable.Add(new Symbol("g", FALSE, "0", "ilit", data));
            SymbolTable.Add(new Symbol("g", "N4", "4", "ilit", data));
            SymbolTable.Add(new Symbol("g", "N-1", "-1", "ilit", data));
            SymbolTable.Add(new Symbol("g", "H!", "!", "clit", dataChar));
            SymbolTable.Add(new Symbol("g", "FREE", "0", "ilit", data));
            filename = filename.Split('.')[0] + ".asm";
            sw = new StreamWriter(filename, false);
            List<Symbol> globalVars = SymbolTable.GetSymbolsInScope("g");
            Symbol var;
            pc = 0;
            for (int i=0; i< globalVars.Count; ++i)//print all global variables at top of assembly file
            {
                var = globalVars[i];
                if (var.symid[0] == 'H')
                {
                    if (var.symid.Length == 1)
                    {
                        sw.WriteLine(var.symid + " .BYT  ");
                    }
                    else
                    {
                        sw.WriteLine(var.symid + " .BYT " + var.symid.Substring(1,(var.symid.Length-1)));
                    }
                    ++pc;
                }
                else if(var.symid[0] == 'N'|| var.symid[0] == 'B' || var.symid == "null")
                {
                    sw.WriteLine(var.symid + " .INT " + var.value);
                    pc += 4;
                }
            }
            sw.WriteLine(FREE + " .INT 0");//address of available heap
            pc += 4;
            sw.WriteLine(";------------------------END OF DATA SEGMENT-----------------------");
            LDR(registers[(int)Register.True],TRUE);
            LDR(registers[(int)Register.False], FALSE);
            LDA(registers[(int)Register.OP1], END);//load address of final instruction in code segment
            ADI(registers[(int)Register.OP1], (3*INSTRUCT_SIZE).ToString());//get address of first free heap location
            STR(registers[(int)Register.OP1], FREE);//update free heap location
            MOV(registers[(int)Register.OP1], registers[(int)Register.FP]);
            ADI(registers[(int)Register.OP1], (-1*VAR_SIZE).ToString());
            STR(registers[(int)Register.FP], registers[(int)Register.OP1]);//set fp to pfp (bottom of stack)
            
            _iCode = iCode;
            foreach (string s in _iCode)
            {
                bufferedLine = "";
                labelOffset = 0;
                operand1 = "";
                operand2 = "";
                operand3 = "";
                comments = s.Split(';');
                line = comments[0].Trim().Split(' ');
                comment = COMMENT_FORMAT + comments[0];
                for(int i =1; i < comments.Length; ++i)
                {
                    comment += comments[i];
                }

                if (!_instructions.Contains<string>(line[0]))
                {
                    //label
                    labelOffset = 1;
                    instruction = line[1];
                    bufferedLine = line[0]+" ";
                }
                else
                {
                    labelOffset = 0;
                    instruction = line[0];
                }
                if (line.Length > labelOffset + 1)//contains 1 or more operands
                {
                    operand1 = line[labelOffset + 1];
                }
                if (instruction == "FUNC")
                {
                    operand2 = operand1;
                }
                else
                {
                    if (line.Length > labelOffset + 2)//contains 2 or more operands
                    {
                        operand2 = line[labelOffset + 2];
                    }
                    if (line.Length > labelOffset + 3)//contains 2 or more operands
                    {
                        operand3 = line[labelOffset + 3];
                    }
                }
                InstructionCall(instruction, operand1, operand2, operand3);
            }
            sw.WriteLine(OVERFLOW+" LDB R3 H!");
            pc += 12;
            sw.WriteLine("TRP 3");
            pc += 12;
            sw.WriteLine(UNDERFLOW + " LDB R3 H!");
            pc += 12;
            sw.WriteLine("TRP 3");
            pc += 12;
            sw.Write(END +" TRP 0");
            sw.Flush();
            sw.Close();
        }
        private static string GetLocation(string operand)
        {
            bool refDeclaration = false;
            if (operand[0] == '=')
            {
                refDeclaration = true;
                operand = operand.Substring(1, operand.Length - 1);
            }
            if (operand == "this")
            {
                Output(OpCode.MOV, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.FP]);//load current fp address
                Output(OpCode.ADI, registers[(int)Register.VAR_ADDRESS], (-1 * getOffset("this")).ToString());//get frame's object.this address
                return registers[(int)Register.VAR_ADDRESS];//return register containing address of operand
            }
            /*if (operand[0] == 't' || operand[1] == 'a')//is an array index (every non-array ref symid is a single char followed by numbers)
            {
                Output(OpCode.MOV, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.FP]);//get current frame pointer
                Output(OpCode.ADI, registers[(int)Register.VAR_ADDRESS], (-1 * getOffset(operand)).ToString());//decrement by offset to get location of address of array
                //Output(OpCode.LDR, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.VAR_ADDRESS]);//load this heap address of array index from stack
                return registers[(int)Register.VAR_ADDRESS];//return register containing address of operand        
            }*/
            else if (operand[0] == 'M' || operand[0] == 'X' || operand[0] == 'Y')
            {
                MOV(registers[(int)Register.VAR_ADDRESS], registers[(int)Register.SP]);
                return registers[(int)Register.VAR_ADDRESS];//return register containing address of operand
            }
            else if ((operand[0] == 'P' && operand != "PC") || operand[0] == 'L' || operand[0] == 't')//parameter or local or temp variable so it must be in a func call (on stack)
            {
                Output(OpCode.MOV, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.FP]);//get current frame pointer
                Output(OpCode.ADI, registers[(int)Register.VAR_ADDRESS], (-1*getOffset(operand)).ToString());//decrement by offset to get operand address
               
                if (operand[operand.Length - 1] == 'r' && !refDeclaration)//if a is a ref (temp holding address of actual object in memory and we want to get address of value
                {
                    LDR(registers[(int)Register.VAR_ADDRESS], registers[(int)Register.VAR_ADDRESS]);//get value at address of temp ref
                }
                if (operand[1] == 'A')//if array
                {
                    Output(OpCode.LDR, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.VAR_ADDRESS]);//load address of array/ref into register
                }
                return registers[(int)Register.VAR_ADDRESS];//return register containing address of operand
            }
            else if (operand[0] == 'V')//instance variable at "this" object on heap
            {
                Output(OpCode.MOV, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.FP]);//load current fp address
                Output(OpCode.ADI, registers[(int)Register.VAR_ADDRESS], (-1 * getOffset("this")).ToString());//get frame's object.this address
                Output(OpCode.LDR, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.VAR_ADDRESS]);//load value at object.this into register
                Output(OpCode.ADI, registers[(int)Register.VAR_ADDRESS], SymbolTable.GetValue(operand).offset.ToString());//get a's address offset in object                if (operand[1] == 'A')
                if (operand[1] == 'A')
                {
                    Output(OpCode.LDR, registers[(int)Register.VAR_ADDRESS], registers[(int)Register.VAR_ADDRESS]);//load address of array into register
                }
                return registers[(int)Register.VAR_ADDRESS];//return register containing address of operand
            }
            return operand;//if global just return symid (label)
        }
        private static int getOffset(string symid)
        {
            if (symid == "this")
            {
                return THIS_OFFSET;
            }
            int offset = (12 + SymbolTable.GetValue(symid).offset);
            return offset;
        }
        private static bool IsRegister(string operand)
        {
            return registers.Contains<string>(operand);
        }
        private static void InstructionCall(string instruct, string operand1, string operand2,string operand3)
        {
            int instructIndex = -1;
            for(int i = 0; i < _instructions.Length; ++i)
            {
                if (_instructions[i] == instruct)
                {
                    instructIndex = i;
                    break;
                }
            }
            switch (instructIndex)
            {
                case (int)Instructions.ADD:
                    ADD(operand1, operand2, operand3);
                    break;
                case (int)Instructions.ADI:
                    ADI(operand1, operand2);
                    break;
                case (int)Instructions.SUB:
                    SUB(operand1, operand2, operand3);
                    break;
                case (int)Instructions.MUL:
                    MUL(operand1, operand2,operand3);
                    break;
                case (int)Instructions.DIV:
                    DIV(operand1, operand2, operand3);
                    break;
                case (int)Instructions.LT:
                    LT(operand1, operand2,operand3);
                    break;
                case (int)Instructions.GT:
                    GT(operand1, operand2, operand3);
                    break;
                case (int)Instructions.NE:
                    NE(operand1, operand2, operand3);
                    break;
                case (int)Instructions.EQ:
                    EQ(operand1, operand2, operand3);
                    break;
                case (int)Instructions.LE:
                    LE(operand1, operand2, operand3);
                    break;
                case (int)Instructions.GE:
                    GE(operand1, operand2, operand3);
                    break;
                case (int)Instructions.AND:
                    AND(operand1, operand2, operand3);
                    break;
                case (int)Instructions.OR:
                    OR(operand1, operand2, operand3);
                    break;
                case (int)Instructions.BF:
                    BF(operand1, operand2);
                    break;
                case (int)Instructions.BT:
                    BT(operand1, operand2);
                    break;
                case (int)Instructions.JMP:
                    JMP(operand1);
                    break;
                case (int)Instructions.PUSH:
                    PUSH(operand1);
                    break;
                case (int)Instructions.POP:
                    POP(operand1);
                    break;
                case (int)Instructions.PEEK:
                    PEEK(operand1);
                    break;
                case (int)Instructions.FRAME:
                    FRAME(operand1, operand2);
                    break;
                case (int)Instructions.CALL:
                    CALL(operand1);
                    break;
                case (int)Instructions.RTN:
                    RTN();
                    break;
                case (int)Instructions.RETURN:
                    RETURN(operand1);
                    break;
                case (int)Instructions.FUNC:
                    FUNC(operand1);
                    break;
                case (int)Instructions.NEWI:
                    NEWI(operand1, operand2);
                    break;
                case (int)Instructions.NEW:
                    NEW(operand1, operand2);
                    break;
                case (int)Instructions.MOV:
                    MOV(operand1, operand2);
                    break;
                case (int)Instructions.MOVI:
                    MOVI(operand1, operand2);
                    break;
                case (int)Instructions.WRITE:
                    WRITE(operand1);
                    break;
                case (int)Instructions.READ:
                    READ(operand1);
                    break;
                case (int)Instructions.CONVERT:
                    CONVERT(operand1);
                    break;
                case (int)Instructions.REF:
                    REF(operand1, operand2, operand3);
                    break;
                case (int)Instructions.AEF:
                    AEF(operand1, operand2, operand3);
                    break;
            }
        }
        
        public static void MOV(string a, string b)
        {
            /*if(a[1] == 'A' || b[1] == 'A')//is an arrray
            {
                if (a[1] == 'A')
                {
                    Output(OpCode.MOV, registers[(int)Register.OP1], registers[(int)Register.FP]);//get current frame pointer
                    Output(OpCode.ADI, registers[(int)Register.OP1], (-1 * getOffset(a)).ToString());//decrement by offset to get location of address of array
                    Output(OpCode.LDR, registers[(int)Register.OP1], registers[(int)Register.OP1]);//load address of array into register
                }
                else
                {
                    Output(OpCode.MOV, registers[(int)Register.OP1], GetLocation(a));//get address of a
                }
                if(b[1] == 'A')
                {
                    Output(OpCode.MOV, registers[(int)Register.OP2], registers[(int)Register.FP]);//get current frame pointer
                    Output(OpCode.ADI, registers[(int)Register.OP2], (-1 * getOffset(b)).ToString());//decrement by offset to get location of address of array
                    Output(OpCode.LDR, registers[(int)Register.OP2], registers[(int)Register.OP2]);//load address of array into register
                    Output(OpCode.LDR, registers[(int)Register.OP2], registers[(int)Register.OP2]);//load value at address of array into register
                }
                else
                {
                    Output(OpCode.LDR, registers[(int)Register.OP2], GetLocation(b));//get value at address of b
                }
                Output(OpCode.STR, registers[(int)Register.OP2], registers[(int)Register.OP1]);//load address of array into register
            }
            else */if (IsRegister(a))//a is a register (TCode)
            {
                Output(OpCode.MOV, a, b);//move value in register b to register a (TCode MOV)
            }
            else//if a is not a register (ICode MOV)
            {
                //a = '='+a;
                MOV(registers[(int)Register.OP2], GetLocation(a));//get address of a
                LDR(registers[(int)Register.OP1], GetLocation(b));//get value at address of b
                STR(registers[(int)Register.OP1], registers[(int)Register.OP2]);//store value at address of a
            }
        }
        public static void MOVI(string a, string b)
        {
            STR(b, GetLocation(a));//store value at address of b
        }
        
        ///takes two non registers
        ///
        public static void ADD(string a, string b, string c)
        {
            if (IsRegister(a) && IsRegister(b))//if both a is a register and b is a register (TCode ADD)
            {
                Output(OpCode.ADD, a, b);//add contents of register b to register a
            }
            else//a and b are not registers (ICode ADD
            {
                Output(OpCode.LDR, registers[(int)Register.OP1], GetLocation(a));//get value at address of "a"
                Output(OpCode.LDR, registers[(int)Register.OP2], GetLocation(b));//get value at address of "b"
                Output(OpCode.ADD, registers[(int)Register.OP1], registers[(int)Register.OP2]);//add values of "b" and "a"
                Output(OpCode.STR, registers[(int)Register.OP1], GetLocation(c));//store value of "a+b" at address of "c"
            }
        }
        public static void ADI(string a, string b)
        {
            if (IsRegister(a))//if a is a register (TCode ADD)
            {
                Output(OpCode.ADI, a, b);//add contents of register b to register a
            }
            else//if a is not a register
            {
                Output(OpCode.LDR, registers[(int)Register.OP1], GetLocation(a));//get value at address of "a"
                RegisterImmediate(OpCode.ADI, registers[(int)Register.OP1], b);//add value in reg a to immediate value of b
                Output(OpCode.STR, registers[(int)Register.OP1], GetLocation(a));//store value of "a+b" at address of "c"
            }
        }
        public static void SUB(string a, string b, string c)
        {
            if (IsRegister(a) && IsRegister(b))//if both a is a register and b is a register (TCode)
            {
                Output(OpCode.SUB, a, b);//sub contents of register b from register a, results in register a
            }
            else//a and b are not registers
            {
                Output(OpCode.LDR, registers[(int)Register.OP1], GetLocation(a));//get value at address of "b"
                Output(OpCode.LDR, registers[(int)Register.OP2], GetLocation(b));//get value at address of "b"
                Output(OpCode.SUB, registers[(int)Register.OP1], registers[(int)Register.OP2]);//add values of "b" and "a"
                Output(OpCode.STR, registers[(int)Register.OP1], GetLocation(c));//store value of "a+b" at address of "c"
            }
        }
        public static void MUL(string a, string b, string c)
        {
            if (IsRegister(a) && IsRegister(b))//if both a is a register and b is a register
            {
                Output(OpCode.MUL, a, b);//move contents of register b to register a
            }
            else//a and b are not registers
            {
                Output(OpCode.LDR, registers[(int)Register.OP1], GetLocation(a));//get value at address of "b"
                Output(OpCode.LDR, registers[(int)Register.OP2], GetLocation(b));//get value at address of "b"
                Output(OpCode.MUL, registers[(int)Register.OP1], registers[(int)Register.OP2]);//add values of "b" and "a"
                Output(OpCode.STR, registers[(int)Register.OP1], GetLocation(c));//store value of "a+b" at address of "c"
            }
        }
        public static void DIV(string a, string b, string c)
        {
            if (IsRegister(a) && IsRegister(b))//if both a is a register and b is a register
            {
                Output(OpCode.DIV, a, b);//move contents of register b to register a
            }
            else//a and b are not registers
            {
                Output(OpCode.LDR, registers[(int)Register.OP1], GetLocation(a));//get value at address of "b"
                Output(OpCode.LDR, registers[(int)Register.OP2], GetLocation(b));//get value at address of "b"
                Output(OpCode.DIV, registers[(int)Register.OP1], registers[(int)Register.OP2]);//add values of "b" and "a"
                Output(OpCode.STR, registers[(int)Register.OP1], GetLocation(c));//store value of "a+b" at address of "c"
            }
        }

        public static void EQ(string a, string b, string c)
        {
            string label = ICode.SKIPIF + ICode.labelCounter++;
            string label2 = ICode.SKIPIF + ICode.labelCounter++;
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BRZ(registers[(int)Register.OP1], label);
            STR(registers[(int)Register.False],GetLocation(c));//set false
            JMP(label2);
            sw.Write(label + " ");
            STR(registers[(int)Register.True], GetLocation(c));//set true
            sw.Write(label2 + " ");
        }

        public static void LT(string a, string b, string c)
        {
            string label = ICode.SKIPIF + ICode.labelCounter++;
            string label2 = ICode.SKIPIF + ICode.labelCounter++;
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BLT(registers[(int)Register.OP1], label);
            STR(registers[(int)Register.False], GetLocation(c));//set false
            JMP(label2);
            sw.Write(label + " ");
            STR(registers[(int)Register.True], GetLocation(c));//set true
            sw.Write(label2 + " ");
        }

        public static void GT(string a, string b, string c)
        {
            string label = ICode.SKIPIF + ICode.labelCounter++;
            string label2 = ICode.SKIPIF + ICode.labelCounter++;
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BGT(registers[(int)Register.OP1], label);
            STR(registers[(int)Register.False], GetLocation(c));//set false
            JMP(label2);
            sw.Write(label + " ");
            STR(registers[(int)Register.True], GetLocation(c));//set true
            sw.Write(label2 + " ");
        }

        public static void NE(string a, string b, string c)
        {
            string label = ICode.SKIPIF + ICode.labelCounter++;
            string label2 = ICode.SKIPIF + ICode.labelCounter++;
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BNZ(registers[(int)Register.OP1], label);
            STR(registers[(int)Register.False], GetLocation(c));//set false
            JMP(label2);
            sw.Write(label + " ");
            STR(registers[(int)Register.True], GetLocation(c));//set true
            sw.Write(label2 + " ");
        }

        public static void LE(string a, string b, string c)
        {
            string label = ICode.SKIPIF + ICode.labelCounter++;
            string label2 = ICode.SKIPIF + ICode.labelCounter++;
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BLT(registers[(int)Register.OP1], label);
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BRZ(registers[(int)Register.OP1], label);
            STR(registers[(int)Register.False], GetLocation(c));//set false
            JMP(label2);
            sw.Write(label + " ");
            STR(registers[(int)Register.True], GetLocation(c));//set true
            sw.Write(label2 + " ");
        }

        public static void GE(string a, string b, string c)
        {
            string label = ICode.SKIPIF + ICode.labelCounter++;
            string label2 = ICode.SKIPIF + ICode.labelCounter++;
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BGT(registers[(int)Register.OP1], label);
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            CMP(registers[(int)Register.OP1], registers[(int)Register.OP2]);
            BRZ(registers[(int)Register.OP1],label);
            STR(registers[(int)Register.False], GetLocation(c));//set false
            JMP(label2);
            sw.Write(label + " ");
            STR(registers[(int)Register.True], GetLocation(c));//set true
            sw.Write(label2 + " ");
        }

        public static void AND(string a, string b, string c)
        {
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            Output(OpCode.AND, registers[(int)Register.OP1], registers[(int)Register.OP2]);
            STR(registers[(int)Register.OP1], GetLocation(c));
        }

        public static void OR(string a, string b, string c)
        {
            LDR(registers[(int)Register.OP1], GetLocation(a));
            LDR(registers[(int)Register.OP2], GetLocation(b));
            Output(OpCode.OR, registers[(int)Register.OP1], registers[(int)Register.OP2]);
            STR(registers[(int)Register.OP1], GetLocation(c));
        }

        public static void BF(string a, string b)
        {
            LDR(registers[(int)Register.OP1], GetLocation(a));
            Output(OpCode.BRZ, registers[(int)Register.OP1], b);
        }

        public static void BT(string a, string b)
        {
            LDR(registers[(int)Register.OP1], GetLocation(a));
            Output(OpCode.BNZ, registers[(int)Register.OP1], b);
        }

        public static void JMP(string a)
        {
            Output(OpCode.JMP, a);
        }

        public static void PUSH(string a)
        {
            if (a == "null")
            {
                LDR(registers[(int)Register.OP1], "null");
                STR(registers[(int)Register.OP1], registers[(int)Register.SP]);
            }
            else
            { 
                MOV(registers[(int)Register.PFP], registers[(int)Register.FP]);
                ADI(registers[(int)Register.FP], (-1 * VAR_SIZE).ToString());
                LDR(registers[(int)Register.FP], registers[(int)Register.FP]);//pfp
                LDR(registers[(int)Register.OP1], GetLocation(a));
                STR(registers[(int)Register.OP1], registers[(int)Register.SP]);
                MOV(registers[(int)Register.FP], registers[(int)Register.PFP]);
            }
            ADI(registers[(int)Register.SP], (-1 * VAR_SIZE).ToString());
        }

        public static void POP(string a)
        {
            LDR(registers[(int)Register.OP1], registers[(int)Register.SP]);
            STR(registers[(int)Register.OP1], GetLocation(a));
            ADI(registers[(int)Register.SP], VAR_SIZE.ToString());
        }

        public static void PEEK(string a)
        {
            LDR(registers[(int)Register.OP1], registers[(int)Register.SP]);
            if (IsRegister(a))
            {
                MOV(registers[(int)Register.OP1], a);
            }
            else
            {
                STR(registers[(int)Register.OP1], GetLocation(a));
            }
            //ADI(registers[(int)Register.OP1], VAR_SIZE.ToString());
        }

        public static void FRAME(string a, string b)
        {
            MOV(registers[(int)Register.OP1], registers[(int)Register.SP]);//get stack pointer
            ADI(registers[(int)Register.OP1], ((12+SymbolTable.GetValue(a).size) * -1).ToString());//move pointer equal to size of function
            CMP(registers[(int)Register.OP1], registers[(int)Register.SL]);//if the pointer passes the stack limit
            BLT(registers[(int)Register.OP1], OVERFLOW);//jump to label overflow
            MOV(registers[(int)Register.PFP], registers[(int)Register.FP]);//save current frame pointer in register (will be PFP for next frame)
            MOV(registers[(int)Register.FP], registers[(int)Register.SP]);//set frame pointer = stack pointer
            ADI(registers[(int)Register.SP], (VAR_SIZE * -1).ToString());//move stack pointer up one (passed new frame's return address)
            STR(registers[(int)Register.PFP], registers[(int)Register.SP]);//store PFP at location contained in stack pointer
            ADI(registers[(int)Register.SP], (VAR_SIZE * -1).ToString());//move stack pointer up one (passed new frame's PFP)
            if (b == "this")
            {
                ADI(registers[(int)Register.PFP], (VAR_SIZE*-2).ToString());
                LDR(registers[(int)Register.OP1], registers[(int)Register.PFP]);//load value at address b (which is an address to the object on the heap)
                STR(registers[(int)Register.OP1], registers[(int)Register.SP]);//store "this" pointer at location contained in stack pointer
            }
            else if (a != "MAIN")
            {
                //ADI(registers[(int)Register.PFP], (-1*(12+SymbolTable.GetValue(b).offset)).ToString());
                ADI(registers[(int)Register.PFP], (-1*getOffset(b)).ToString());
                LDR(registers[(int)Register.OP1], registers[(int)Register.PFP]);//load value at address b (which is an address to the object on the heap)
                STR(registers[(int)Register.OP1], registers[(int)Register.SP]);//store "this" pointer at location contained in stack pointer
            }
            ADI(registers[(int)Register.SP], (VAR_SIZE * -1).ToString());//move stack pointer up one (passed new frame's "this" pointer)
        }

        public static void CALL(string a)
        {
            MOV(registers[(int)Register.OP1], registers[(int)Register.PC]);//get pc
            ADI(registers[(int)Register.OP1], (INSTRUCT_SIZE*3).ToString());//calculate return address (pc + size of 3 instructions)
            STR(registers[(int)Register.OP1], registers[(int)Register.FP]);//store return address at current frame pointer
            JMP(a);
        }



        //Icode exclusive instructions

        public static void RTN()
        {
            MOV(registers[(int)Register.SP], registers[(int)Register.FP]);//de-allocate activation record (set stack pointer = frame pointer)
            MOV(registers[(int)Register.OP1], registers[(int)Register.SP]);
            CMP(registers[(int)Register.OP1], registers[(int)Register.SB]);//check if stack pointer is less than SB
            BGT(registers[(int)Register.OP1], UNDERFLOW);//if stack pointer > SB jump to label underflow
            LDR(registers[(int)Register.OP1], registers[(int)Register.FP]);//get return address and move it to a register
            ADI(registers[(int)Register.FP], (VAR_SIZE *- 1).ToString()); //set PFP to current FP
            LDR(registers[(int)Register.FP], registers[(int)Register.FP]);//get address in PFP and move it to FP
            JMR(registers[(int)Register.OP1]);//jump to return address stored in register
        }

        public static void RETURN(string a)
        {
            if (a[0] == 'M' || a[0] == 'X' || a[0] == 'Y')//return a method return value (value at sp)
            {
                PEEK(registers[(int)Register.R5]);
            }
            MOV(registers[(int)Register.SP], registers[(int)Register.FP]);//de-allocate activation record (set stack pointer = frame pointer)
            MOV(registers[(int)Register.OP1], registers[(int)Register.SP]);
            CMP(registers[(int)Register.OP1], registers[(int)Register.SB]);//check if stack pointer is less than SB
            BGT(registers[(int)Register.OP1], UNDERFLOW);//if stack pointer > SB jump to label underflow
            LDR(registers[(int)Register.OP2], registers[(int)Register.FP]);//get return address and move it to a register
            if(a[0] != 'M' && a[0] != 'X' && a[0] != 'Y')
            {

                LDR(registers[(int)Register.R5], GetLocation(a));//load value at address of a into register
            }
            ADI(registers[(int)Register.FP], (VAR_SIZE * -1).ToString()); //set PFP to current FP
            LDR(registers[(int)Register.FP], registers[(int)Register.FP]);//get address in PFP and move it to FP
            STR(registers[(int)Register.R5], registers[(int)Register.SP]);//store value into address of SP
            JMR(registers[(int)Register.OP2]);//jump to return address stored in register
        }
        
        public static void FUNC(string a)
        {
            ADI(registers[(int)Register.SP], (-1 * SymbolTable.GetValue(a).size).ToString());//get size of a and allocate space on stack (move sp up size amount)
        }
        public static void NEWI(string a, string b)//allocate object on heap NEWI(8,t100)
        {
            LDR(registers[(int)Register.OP1], FREE);//load value (contains an address) at label FREE
            STR(registers[(int)Register.OP1], GetLocation(b));//Store value at address of b
            ADI(registers[(int)Register.OP1], a);//allocate size of a
            STR(registers[(int)Register.OP1], FREE);//update heap
        }
        public static void NEW(string a, string b)//allocate array on heap (a is total size of array, b is address of index 0)
        {
            LDR(registers[(int)Register.R5], FREE);//load value (contains an address) at label FREE
            STR(registers[(int)Register.R5], GetLocation(b));//Store value at address of b
            LDR(registers[(int)Register.OP2], GetLocation(a));//allocate size of array, contained in "a"
            ADD(registers[(int)Register.R5], registers[(int)Register.OP2], null);
            STR(registers[(int)Register.R5], FREE);//update heap
        }
        public static void WRITE(string a)
        {
            string type = SymbolTable.GetValue(a).data[0].Split(':')[1];
            LDR(registers[(int)Register.OP2], GetLocation(a));//load value at label address of a
            if (type == "char")
            {
                Output(OpCode.TRP, "3");
            }
            else
            {
                Output(OpCode.TRP, "1");
            }
        }
        public static void READ(string a)
        {
            string type = SymbolTable.GetValue(a).data[0].Split(':')[1];
            if (type == "char")
            {
                Output(OpCode.TRP, "4");
            }
            else
            {
                Output(OpCode.TRP, "2");
            }
            MOV(registers[(int)Register.OP1], GetLocation(a));
            STR(registers[(int)Register.OP2], registers[(int)Register.OP1]);
        }
        public static void CONVERT(string a)
        {
            string type = SymbolTable.GetValue(a).data[0].Split(':')[1];
            LDR(registers[(int)Register.OP2], GetLocation(a));//load value at label address of a
            if (type == "char")//itoa
            {
                Output(OpCode.TRP, "11");
            }
            else//atoi
            {
                Output(OpCode.TRP, "10");
            }
            MOV(registers[(int)Register.OP1], GetLocation(a));
            STR(registers[(int)Register.OP2], registers[(int)Register.OP1]);
        }
        public static void REF(string a, string b, string c)
        {
            LDR(registers[(int)Register.OP1], GetLocation(a));//load address of a
            ADI(registers[(int)Register.OP1], SymbolTable.GetValue(b).offset.ToString());//load value at address of b (address of object on heap)
            //string type = SymbolTable.GetValue(a).data[0].Split(':')[1];//get type
            c = "=" + c;
            STR(registers[(int)Register.OP1], GetLocation(c));//store value at address of c
        }
        public static void AEF(string a, string b, string c)//a=array symid, b = index of array (offset from a), c is temp to store value
        {
            LDR(registers[(int)Register.OP1], GetLocation(a));//load address of a
            LDR(registers[(int)Register.OP2], GetLocation(b));//load value of b
            LDR(registers[(int)Register.R5], "N4");
            MUL(registers[(int)Register.OP2], registers[(int)Register.R5], null);//multiply by sizeof
            ADD(registers[(int)Register.OP1], registers[(int)Register.OP2], null);//load value at offset of b (address of index on heap)
            MOV(registers[(int)Register.OP2], registers[(int)Register.FP]);//load address of c
            ADI(registers[(int)Register.OP2], (-1 * getOffset(c)).ToString());//load address of c
            STR(registers[(int)Register.OP1], registers[(int)Register.OP2]);//store address of a at address of c
        }
        

        // Assembly instructions not in ICode

        private static void LDR(string a, string b)
        {
            if (!IsRegister(b) && SymbolTable.GetValue(b).data[0].Split(':')[1] == "char")
            {
                LDB(a, b);
            }
            else
            {
                Output(OpCode.LDR, a, b);
            }
        }
        private static void LDB(string a, string b)
        {
            if (!IsRegister(b) && SymbolTable.GetValue(b).data[0].Split(':')[1] != "char")
            {
                LDR(a, b);
            }
            else
            {
                Output(OpCode.LDB, a, b);
            }
        }
        private static void LDA(string a, string b)
        {
            Output(OpCode.LDA, a, b);
        }
        private static void STR(string a, string b)
        {
            Output(OpCode.STR, a, b);
        }
        private static void CMP(string a, string b)
        {
            if (registers.Contains<string>(a) && registers.Contains<string>(b))
            {
                Output(OpCode.CMP, a, b);
            }
            else
            {
                LDR(registers[(int)Register.OP1], GetLocation(a));
                LDR(registers[(int)Register.OP2], GetLocation(b));
                Output(OpCode.CMP, registers[(int)Register.OP1], registers[(int)Register.OP2]);
                STR(registers[(int)Register.OP1], GetLocation(a));
            }
        }
        private static void JMR(string a)
        {
            Output(OpCode.JMR, a);
        }
        private static void BRZ(string a, string b)
        {
            if (registers.Contains<string>(a))
            {
                Output(OpCode.BRZ, a, b);
            }
            else
            {
                LDR(registers[(int)Register.OP1], GetLocation(a));
                Output(OpCode.BRZ, registers[(int)Register.OP1], b);
            }
        }
        private static void BNZ(string a, string b)
        {
            if (registers.Contains<string>(a))
            {
                Output(OpCode.BNZ, a, b);
            }
            else
            {
                LDR(registers[(int)Register.OP1], GetLocation(a));
                Output(OpCode.BNZ, registers[(int)Register.OP1], b);
            }
        }
        private static void BGT(string a, string b)
        {
            if (registers.Contains<string>(a))
            {
                Output(OpCode.BGT, a, b);
            }
            else
            {
                LDR(registers[(int)Register.OP1], GetLocation(a));
                Output(OpCode.BGT, registers[(int)Register.OP1], b);
            }
        }
        private static void BLT(string a, string b)
        {
            if (registers.Contains<string>(a))
            {
                Output(OpCode.BLT, a, b);
            }
            else
            {
                LDR(registers[(int)Register.OP1], GetLocation(a));
                Output(OpCode.BLT, registers[(int)Register.OP1], b);
            }
        }
        /* 
        */

        private static void RegisterRegister(OpCode op, string a, string b, string c)
        {
            Output(OpCode.LDR, registers[(int)Register.OP1], a);
            Output(OpCode.LDR, registers[(int)Register.OP2], b);
            Output(op, registers[(int)Register.OP1], registers[(int)Register.OP2]);
            Output(OpCode.STR, registers[(int)Register.OP1], c);
        }
        private static void RegisterRegister(OpCode op, string a, string b)
        {
            Output(OpCode.LDR, registers[(int)Register.OP1], a);
            Output(OpCode.LDR, registers[(int)Register.OP2], b);
            Output(op, registers[(int)Register.OP1], registers[(int)Register.OP2]);
        }
        private static void RegisterImmediate(OpCode op, string a, string b)
        {
            Output(op, a, b);
        }

        private static void Output(OpCode opcode)
        {
            sw.WriteLine(bufferedLine + OpCodes[(int)opcode]+" ;===="+pc + comment);
            bufferedLine = "";
            comment = "";
            pc += 12;
            sw.Flush();
        }
        private static void Output(OpCode opcode, string a)
        {
            sw.WriteLine(bufferedLine + OpCodes[(int)opcode]  + " " + a + " ;====" + pc + comment);
            bufferedLine = "";
            comment = "";
            pc += 12;
            sw.Flush();
        }
        private static void Output(OpCode opcode, string a, string b)
        {
            sw.WriteLine(bufferedLine + OpCodes[(int)opcode]  + " " + a + " " + b + " ;====" + pc + comment);
            bufferedLine = "";
            comment = "";
            pc += 12;
            sw.Flush();
        }
        private static void Output(string a, OpCode opcode, string b)
        {
            sw.WriteLine(bufferedLine + OpCodes[(int)opcode] + " " + a + " " + b + " ;====" + pc + comment);
            bufferedLine = "";
            comment = "";
            pc += 12;
            sw.Flush();
        }
        private static void Output(OpCode opcode, string a, string b, string c)
        {
            sw.WriteLine(bufferedLine+OpCodes[(int)opcode] + " " + a + " " + b + " ;====" + pc + c+comment);
            bufferedLine = "";
            comment = "";
            pc += 12;
            sw.Flush();
        }
    }
}
