using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    static class SemanticActions
    {
        public static int uniqueCounter = 100;
        public static bool variableInit = false;
        private static Stack<string> OS=new Stack<string>();
        private static Stack<int> OSprecidence=new Stack<int>();
        private static Stack<SAR> SAS= new Stack<SAR>();

        public static void SemanticError(SAR sar)
        {
            Console.WriteLine(sar.token.lineNumber + ": " + sar.dataType + " " + sar.value + " not defined in " + sar.scope);
            Environment.Exit(0);
        }
        public static void SemanticPrivateError(SAR sar)
        {
            Console.WriteLine(sar.token.lineNumber + ": " + sar.dataType + " " + sar.value + " is private and can't be accessed in "+sar.scope);
            Environment.Exit(0);
        }
        public static void SemanticDupError(Token t, string scope)
        {
            Console.WriteLine(t.lineNumber + ": duplicate found, " + t.lexeme + " is already declared in " + scope);
            Environment.Exit(0);
        }
        public static void SemanticArrError(SAR identifier,SAR expression)
        {
            Console.WriteLine(identifier.token.lineNumber + ": " +identifier.value +"["+expression.dataType+" "+ expression.value + "] arr index requires type int");
            Environment.Exit(0);
        }
        public static void SemanticReturnError(SAR s, string scope)
        {
            Console.WriteLine(s.token.lineNumber + ": "+s.dataType + " " + s.value + " does not match return type of " + scope);
            Environment.Exit(0);
        }
        public static void SemanticBoolError(Token t)
        {
            Console.WriteLine(t.lineNumber + ": type bool expected for if/while");
            Environment.Exit(0);
        }
        public static void SemanticTypeError(Token t, string s)
        {
            Console.WriteLine(t.lineNumber + ": found "+t.lexeme+" expecting "+s);
            Environment.Exit(0);
        }
        public static void SemanticLiteralError(Token t)
        {
            Console.WriteLine(t.lineNumber + ": literal expected found: "+t.lexeme);
            Environment.Exit(0);
        }
        public static void SemanticConstructorError(Token t)
        {
            Console.WriteLine(t.lineNumber + ": " + " constructor name doesn't match class name");
            Environment.Exit(0);
        }
        public static void SemanticOperationError(SAR x, SAR y, string symbol)
        {
            Console.WriteLine(x.token.lineNumber + ": Invalid Operation: " + x.dataType + " " + x.value +
                " " + symbol + " " + y.dataType + " " + y.value);
            Environment.Exit(0);
        }
        public static void SemanticCoutError(SAR sar)
        {
            Console.WriteLine(sar.token.lineNumber + ": cout requires char got " + sar.dataType);
            Environment.Exit(0);
        }
        public static void SemanticCinError(SAR sar)
        {
            Console.WriteLine(sar.token.lineNumber + ": cin requires char got " + sar.dataType);
            Environment.Exit(0);
        }
        public static void SemanticAtoiError(SAR sar)
        {
            Console.WriteLine(sar.token.lineNumber + ": " + sar.dataType+ " cannot be converted to an int");
            Environment.Exit(0);
        }
        public static void SemanticItoaError(SAR sar)
        {
            Console.WriteLine(sar.token.lineNumber + ": " + sar.dataType+ " cannot be converted to an int");
            Environment.Exit(0);
        }
        public static void SASError()
        {
            if (SAS.Count > 0)
            {
                Console.WriteLine(SAS.Pop().token.lineNumber+": Too many operators: \"" + OS.Pop() + "\"  cannot be used");
            }
            else
            {
                Console.WriteLine("Too many operators: OS.Count = " + (OS.Count-1) + "| SAS.Count = " + SAS.Count);
            }
            Environment.Exit(0);
        }

        public static void iPush(Token token, string scope)
        {
            SAR temp = new Compiler.SAR(SAR.SARtype.Identifier, token, token.lexeme, scope);
            SAS.Push(temp);
        }

        public static void lPush(Symbol symbol,Token token, string scope)
        {
            SAR temp = new Compiler.SAR(SAR.SARtype.Literal, token, token.lexeme, scope);
            if (token.type == Token.Type.Number)
            {
                temp.dataType = "int";
            }
            else if (token.type == Token.Type.Character)
            {
                temp.dataType = "char";
            }
            else if (token.type == Token.Type.BoolSymbol||token.lexeme=="true"||token.lexeme=="false")
            {
                temp.dataType = "bool";
            }
            else if (token.lexeme == "null")
            {
                temp.dataType = "null";
            }
            else
            {
                SemanticLiteralError(token);
            }
            temp.symid = symbol.symid;
            SAS.Push(temp);
        }

        public static void oPush(Token token)
        {
            int precedence = -1;
            string operator_ = token.lexeme;
            if (operator_ == "(" || operator_ == "[" )
            {
                precedence = 8;
            }
            else if (operator_ == ")" || operator_ == "]")
            {
                ShuntYardAll();
                return;
            }
            else if (operator_ == ",")
            {
                OSprecidence.Push(precedence);
                OS.Push(operator_);
                ShuntYardAll();
                return;
            }
            else if (operator_ == "*" || operator_ == "/")
            {
                precedence = 6;
            }
            else if (operator_ == "+" || operator_ == "-")
            {
                precedence = 5;
            }
            else if (operator_[0] == '<' || operator_[0] == '>')
            {
                precedence = 4;
            }
            else if (operator_ == "==" || operator_ == "!=")
            {
                precedence = 3;
            }
            else if (operator_ == "and")
            {
                precedence = 2;
            }
            else if (operator_ == "or")
            {
                precedence = 1;
            }
            else if (operator_ == "=")
            {
                precedence = 0;
            }
            if (OS.Count <= 0)
            {
                OS.Push(" ");
                OSprecidence.Push(0);
            }
            else if((precedence!=-1) && (precedence<OSprecidence.Peek()))//less precedence means previous needs to be used first
            {
                while(precedence < OSprecidence.Peek())
                {
                    if(OSprecidence.Peek() == 8)//stop on left paranthesis
                    {
                        break;
                    }
                    ShuntYardOne();
                }
            }
            OSprecidence.Push(precedence);
            OS.Push(operator_);
        }

        public static void tPush(Token token, string scope)
        {
            SAS.Push(new Compiler.SAR(SAR.SARtype.Type, token, token.lexeme, scope));
        }

        public static void iExist()
        {
            SAR top_sar = SAS.Pop();
            if (top_sar.value == "this")
            {
                SAS.Push(top_sar);
                return;
            }
            string args = "";
            if (top_sar.argList.Count != 0)
            {
                args= top_sar.argList[0];
                for(int i = 1; i<top_sar.argList.Count; ++i)
                {
                    args += ","+top_sar.argList[i];
                }
            }
            top_sar.paramType = args;
            string symid = SymbolTable.iExists(top_sar);
            if (symid==null || !SymbolTable.ContainsSymid(symid))
            {
                SemanticError(top_sar);
            }
            Symbol symbol = SymbolTable.GetValue(symid);
            if (symbol.data != null)
            {
                top_sar.dataType = symbol.data[0].Split(':')[1];
                top_sar.dataType.Trim();
            }
            top_sar.paramType = symbol.parameters;
            top_sar.symid = symbol.symid;
            if (symbol.kind == "method")
            {
                ICode.FRAME(symbol.symid, "this");
                if (symbol.parameters != null && symbol.parameters != "")
                {
                    for (int i = 0; i < top_sar.argList.Count; ++i)
                    {
                        ICode.PUSH(top_sar.argList[i]);
                    }
                }
                ICode.CALL(symbol.symid);
                Symbol temp = new Symbol("t", "t" + uniqueCounter++, "t_" + symbol.value + "_ReturnValue", "tvar", symbol.data);
                SymbolTable.Add(temp);
                top_sar.symid = temp.symid;
                if (symbol.data[0].Split(':')[1] != "void" && symbol.data[0].Split(':')[1] != "null")
                {
                    ICode.PEEK(temp.symid);
                }
            }
            SAS.Push(top_sar);
        }

        public static void vPush(Symbol s, Token t, string scope)
        {
            SAR sar = new Compiler.SAR(SAR.SARtype.Identifier, t, s.value, scope);
            sar.dataType = s.data[0].Split(':')[1];
            sar.symid = s.symid;
            SAS.Push(sar);
            variableInit = true;
        }

        public static void rExist()
        {
            SAR top_sar = SAS.Pop();
            SAR LHS = SAS.Peek();
            bool this_ = false;
            string scope = top_sar.scope;
            if (SAS.Count > 0)
            {
                if (SAS.Peek().value != "this")
                {
                    top_sar.classType = SAS.Pop().dataType;
                }
                else
                {
                    this_ = true;
                    string[] temp = top_sar.scope.Split('.');
                    top_sar.classType = temp[temp.Length - 1];
                    SAS.Pop();
                }
            }
            else
            {
                SemanticError(top_sar);
            }
            Symbol symbol;
            if ((symbol = SymbolTable.rExists(top_sar))==null)
            {
                SemanticError(top_sar);
            }
            if (symbol.data != null) //not a class
            {
                if ((symbol.data[symbol.data.Length - 1] == "accessMod:public") || this_) //member is public
                {
                    top_sar.dataType = symbol.data[0].Split(':')[1];
                    top_sar.dataType.Trim();
                    top_sar.sarType = SAR.SARtype.Ref;
                    top_sar.symid = "t" + uniqueCounter++;
                    top_sar.scope = "t";
                    string[] data = { "returnType:" + top_sar.dataType, "accessMod:private" };
                    if (symbol.kind == "method")
                    {
                        ICode.FRAME(symbol.symid, LHS.symid);
                        if (symbol.parameters != null && symbol.parameters != "")
                        {
                            for (int i = 0; i < top_sar.argList.Count; ++i)
                            {
                                ICode.PUSH(top_sar.argList[i]);
                            }
                        }
                        ICode.CALL(symbol.symid);
                        ICode.PEEK(top_sar.symid);
                    }
                    else
                    {
                        top_sar.symid += "r";
                        if (LHS.value == "this")
                        {
                            LHS.symid = "this";
                        }
                        ICode.REF(LHS.symid, symbol.symid, top_sar.symid);
                    }
                    Symbol temp = new Symbol(top_sar.scope, top_sar.symid, "t"+top_sar.value, "tvar", data);
                    SymbolTable.Add(temp);
                    SAS.Push(top_sar);
                    return;
                }
                else
                {
                    SemanticPrivateError(top_sar);
                }
            }
            /*if (SymbolTable.ContainsValue(scope + "." + top_sar.value))
            {
                Symbol symbol = SymbolTable.GetValue(SymbolTable.GetSymid(scope + "." + top_sar.value));
                if (symbol.data != null) //not a class
                {
                    if (symbol.data[symbol.data.Length - 1]=="accessMod:public") //member is public
                    {
                        top_sar.dataType = symbol.data[0].Split(':')[1];
                        top_sar.dataType.Trim();
                        if (symbol.data.Length == 3)
                        {
                            top_sar.paramType = symbol.data[1];
                        }
                        top_sar.sarType = SAR.SARtype.Ref;
                        SAS.Push(top_sar);
                        return;
                    }
                }
            }*/
            SemanticError(top_sar);
        }

        public static void tExist()
        {
            SAR type_sar = SAS.Pop();
            if (Scanner.typeKeyWords.Contains(type_sar.value))
            {
                return;
            }
            if (!SymbolTable.tExists(type_sar))
            {
                SemanticError(type_sar);
            }
        }

        public static void BAL()
        {
            SAS.Push(new SAR(SAR.SARtype.BAL, null, null, null));
        }

        public static void EAL()
        {
            List<string> argList = new List<string>();
            SAR top_sar;
            while ((top_sar = SAS.Pop()).sarType != SAR.SARtype.BAL)
            {
                argList.Add(top_sar.symid);
            }
            argList.Reverse();
            top_sar = new SAR(SAR.SARtype.AL, null, null, null);
            top_sar.argList = argList;
            SAS.Push(top_sar);
        }

        public static void func()
        {
            SAR al_sar = SAS.Pop();
            SAR id_sar = SAS.Pop();
            id_sar.argList = al_sar.argList;
            id_sar.sarType = SAR.SARtype.Function;
            SAS.Push(id_sar);
        }

        public static void arr()
        {
            SAR expression = SAS.Pop();
            SAR identifier = SAS.Pop();
            if (expression.dataType !="int")
            {
                SemanticArrError(identifier, expression);
            }
            identifier.argList = new List<string>();
            identifier.argList.Add(expression.symid);
            identifier.paramType = "int";
            identifier.dataType = "@";
            SAS.Push(identifier);
        }

        public static void if_(Token t)
        {
            if (SAS.Count == 0)
            {
                SemanticBoolError(t);
            }
            SAR expression = SAS.Pop();
            if (expression.dataType != "bool")
            {
                SemanticBoolError(t);
            }
            string SKIPIF = (ICode.SKIPIF + ICode.labelCounter++) + " ";
            ICode.BF(expression.symid, SKIPIF);
            ICode.StackIf(SKIPIF);
        }

        public static void while_(Token t)
        {
            if (SAS.Count == 0)
            {
                SemanticBoolError(t);
            }
            SAR expression = SAS.Pop();
            if (expression.dataType != "bool")
            {
                SemanticBoolError(t);
            }
            string ENDWHILE = (ICode.ENDWHILE + ICode.labelCounter++) + " ";
            ICode.BF(expression.symid, ENDWHILE);
            ICode.StackEndWhile(ENDWHILE);
        }

        public static void return_(Token t, string scope)
        {
            SAR expression=null;
            ShuntYardAll();
            if (SAS.Count > 0)
            {
                expression = SAS.Pop();
                if (expression.value == "true" || expression.value == "false")
                {
                    expression.dataType = "bool";
                }
            }
            if (!SymbolTable.return_(expression,scope))
            {
                SemanticReturnError(expression,scope);
            }
            if (expression == null)
            {
                ICode.RTN();
            }
            else
            {
                ICode.RETURN(expression.symid);
            }
        }

        public static void cout()
        {
            ShuntYardAll();
            SAR exp = SAS.Pop();
            string type = exp.dataType;
            if((type[0] == '@') && (exp.argList.Count > 0))
            {
                type=type.Substring(1, type.Length - 1);
            }
            if (type != "char"&& type != "int")
            {
                SemanticCoutError(exp);
            }
            ICode.WRITE(exp.symid);
        }

        public static void cin()
        {
            ShuntYardAll();
            SAR exp = SAS.Pop();
            string type = exp.dataType;
            if ((type[0] == '@') && (exp.argList.Count > 0))
            {
                type = type.Substring(1, type.Length - 1);
            }
            if (type != "char" && type != "int")
            {
                SemanticCoutError(exp);
            }
            ICode.READ(exp.symid);
        }

        public static void atoi()
        {
            SAR exp = SAS.Pop();
            if (exp.dataType != "char" && exp.dataType != "int" && exp.dataType != "bool")
            {
                SemanticAtoiError(exp);
            }
            string[] data = { "returnType:int", "accessMod:private" };
            Symbol temp = new Symbol("t", "t" + uniqueCounter++, "t" + exp.value, "tvar", data);
            SymbolTable.Add(temp);
            ICode.MOV(temp.symid, exp.symid);
            ICode.CONVERT(temp.symid);
            exp.dataType = "int";
            exp.symid = temp.symid;
            SAS.Push(exp);
        }

        public static void itoa()
        {
            SAR exp = SAS.Pop();
            if (exp.dataType != "int")
            {
                SemanticAtoiError(exp);
            }
            string[] data = { "returnType:char", "accessMod:private" };
            Symbol temp = new Symbol("t", "t" + uniqueCounter++, "t" + exp.value, "tvar", data);
            SymbolTable.Add(temp);
            ICode.MOV(temp.symid, exp.symid);
            ICode.CONVERT(temp.symid);
            exp.dataType = "char";
            exp.symid = temp.symid;
            SAS.Push(exp);
        }

        public static void NewObj()
        {
            SAR al = SAS.Pop();
            SAR type = SAS.Pop();
            if(!SymbolTable.NewObj(type, al.argList.ToArray()))
            {
                SemanticError(type);
            }
            string functionSymid = type.symid;
            type.sarType = SAR.SARtype.Ref;
            type.symid = "t" + uniqueCounter++;
            string[] data = { "returnType:" + type.dataType, "accessMod:private" };
            Symbol instance = new Symbol("t", type.symid, "t"+type.value, "tvar", data);
            SymbolTable.Add(instance);
            string[] data2 = { "returnType:" + type.dataType, "accessMod:private" };
            Symbol peekTemp = new Symbol("t", "t" + uniqueCounter++, "t" + type.value+"ReturnValue", "tvar", data2);
            SymbolTable.Add(peekTemp);
            Symbol staticInit = SymbolTable.GetValue(("Y"+functionSymid.Substring(1)));
            ICode.NEWI(type.size.ToString(), type.symid);
            //call constructor
            ICode.FRAME(functionSymid, type.symid);
            if (al.argList != null && al.argList.Count>0 && al.argList[0]!="")
            {
                for (int i = 0; i < al.argList.Count; ++i)
                {
                    ICode.PUSH(al.argList[i]);
                }
            }
            ICode.CALL(functionSymid);
            ICode.PEEK(peekTemp.symid);
            type.symid = peekTemp.symid;
            SAS.Push(type);
        }

        public static void new_arr()
        {
            SAR exp = SAS.Pop();
            SAR type = SAS.Pop();
            if (exp.dataType != "int")
            {
                SemanticArrError(type, exp);
            }
            if (type.value == "null")
            {
                SemanticArrError(type, exp);
            }
            string[] data1 = { "returnType:int", "accessMod:private" };
            Symbol size = new Symbol("t", "t"+uniqueCounter++, "t"+type.value, "tvar", data1);
            SymbolTable.Add(size);
            string[] data = { "returnType:"+"@" + type.value, "accessMod:private" };
            Symbol temp = new Symbol("t", "t"+uniqueCounter++, "t"+type.value, "tvar", data);
            SymbolTable.Add(temp);
            type.symid = temp.symid;
            int classSize = SymbolTable.GetValue(type.symid).size;
            string[] sizeData = { "returnType:int", "accessMod:public" };
            Symbol sizeLiteral = new Symbol("g", "N4", "4", "ilit", sizeData);
            SymbolTable.Add(sizeLiteral);
            ICode.MUL(sizeLiteral.symid, exp.symid, size.symid);
            ICode.NEW(size.symid,type.symid);
            type.dataType = "@" + type.value;
            SAS.Push(type);
        }

        public static void CD(Token t, string scope)
        {
            if (t.lexeme != scope.Split('.')[1])
            {
                SemanticConstructorError(t);
            }
        }

        public static void dup(Token t, string scope)
        {
            if(SymbolTable.dup(t,scope))
            {
                SemanticDupError(t,scope);
            }
        }

        public static void spawn()
        {
            /*iExist();
            SAR identifier = SAS.Pop();
            SAR func = SAS.Pop();
            if (identifier.dataType != "int")
            {
                SemanticTypeError(identifier.token,"int");
            }
            if (func.sarType != SAR.SARtype.Function)
            {
                SemanticTypeError(identifier.token,"function");
            }*/
        }
        public static void block() { }
        public static void lock_()
        {
            /*SAR identifier = SAS.Pop();
            if (identifier.dataType != "sym")
            {
                SemanticTypeError(identifier.token, "sym");
            }*/
        }
        public static void release()
        {
            /*SAR identifier = SAS.Pop();
            if (identifier.dataType != "sym")
            {
                SemanticTypeError(identifier.token, "sym");
            }*/
        }

        public static void EOE()
        {
            ShuntYardAll();
            if (SAS.Count > 0)
            {
                SAS.Pop();
            }
            if (variableInit)
            {
                variableInit = false;
            }
        }
        public static void ShuntYardAll()
        {
            string symbol;
            bool comma = false;
            while (OS.Count>1 && OS.Peek()!="(" && OS.Peek()!="[")
            {
                if (SAS.Count <= 1)
                {
                    SASError();
                    return;
                }
                symbol = OS.Peek();
                if (symbol == ",")
                {
                    comma = true;
                }
                if (symbol.Equals("="))
                {
                    AssignmentOperator();
                }
                else if (symbol.Equals("+") || symbol.Equals("-") || symbol.Equals("*") || symbol.Equals("/"))
                {
                    MathOperator();
                }
                else if (symbol[0]=='!'||symbol[0]=='<' || symbol[0] == '>' || symbol == "==")
                {
                    if (symbol.Length == 2)
                    {
                        if (symbol[1] != '=')
                        {
                            SASError();
                        }
                    }
                    BoolOperator();
                }
                else if (symbol == "and" || symbol=="or")
                {
                    LogicOperator();
                }
                else if (symbol == ",")
                {
                    OS.Pop();
                    OSprecidence.Pop();
                }
            }
            if(OS.Count>0 && !comma && (OS.Peek()=="(" || OS.Peek() == "["))
            {
                OS.Pop();
                OSprecidence.Pop();
            }
        }
        public static void ShuntYardOne()
        {
            string symbol;
            if (SAS.Count <= 1)
            {
                SASError();
                return;
            }
            symbol = OS.Peek();
            if (symbol.Equals("="))
            {
                AssignmentOperator();
            }
            else if (symbol.Equals("+") || symbol.Equals("-") || symbol.Equals("*") || symbol.Equals("/"))
            {
                MathOperator();
            }
            else if (symbol[0]=='!'||symbol[0]=='<' || symbol[0] == '>' || symbol == "==")
            {
                if (symbol.Length == 2)
                {
                    if (symbol[1] != '=')
                    {
                        SASError();
                    }
                }
                BoolOperator();
            }
            else if (symbol == "and" || symbol=="or")
            {
                LogicOperator();
            }
        }

        public static void AssignmentOperator()
        {
            SAR y = SAS.Pop();
            SAR x = SAS.Pop();
            if (x.dataType[0] == '@' && (x.argList!=null && x.argList.Count>0))
            {
                x.dataType = x.dataType.Substring(1);
            }
            if (y.dataType[0] == '@' && (y.argList != null && y.argList.Count > 0))
            {
                y.dataType = y.dataType.Substring(1);
            }
            if ((x.sarType==SAR.SARtype.Identifier||x.sarType==SAR.SARtype.Ref) && 
                (x.dataType == y.dataType || y.value == "null"|| y.dataType == "null"))
            {
                if((y.value=="null" || y.dataType == null) && (x.dataType == "int" || x.dataType == "char" || x.dataType == "bool"))
                {
                }
                else
                {
                    ICode.MOV(x.symid, y.symid);
                    OS.Pop();
                    OSprecidence.Pop();
                    return;
                }
            }
            SemanticOperationError(x, y, OS.Peek());
        }

        public static void MathOperator()
        {
            bool valid = false;
            SAR y = SAS.Pop();
            SAR x = SAS.Pop();
            SAR z = new SAR(SAR.SARtype.Identifier,x.token,x.value,x.scope);
            z.dataType = x.dataType;
            string op = OS.Peek();
            if (x.dataType[0] == '@'&&x.argList!=null &&x.argList.Count!=0)
            {
                if(x.dataType.Substring(1, x.dataType.Length - 1) == y.dataType)
                {
                    valid = true;
                }
            }
            if (y.dataType[0] == '@'&&y.argList!=null &&y.argList.Count!=0)
            {
                if(y.dataType.Substring(1, y.dataType.Length - 1) == x.dataType)
                {
                    valid = true;
                }
            }
            if (((x.dataType == y.dataType) && (x.dataType=="int"))||valid)
            {
                z.symid = "t" + uniqueCounter++;
                z.value += "_" + y.value;
                z.scope = "t";
                string[] data = { "returnType:"+z.dataType, "accessMod:private" };
                Symbol temp = new Symbol("t", z.symid, z.value, "tvar", data);
                SymbolTable.Add(temp);
                if (op == "+")
                {
                    ICode.ADD(x.symid, y.symid,z.symid);
                }
                else if (op == "-")
                {
                    ICode.SUB(x.symid, y.symid, z.symid);
                }
                else if (op == "*")
                {
                    ICode.MUL(x.symid, y.symid, z.symid);
                }
                else if (op == "/")
                {
                    ICode.DIV(y.symid, x.symid, z.symid);
                }
                SAS.Push(z);
                OS.Pop();
                OSprecidence.Pop();
                return;
            }
            SemanticOperationError(x, y, OS.Peek());
        }

        public static void BoolOperator()
        {
            SAR y = SAS.Pop();
            SAR x = SAS.Pop();
            SAR z = new SAR(SAR.SARtype.Identifier, x.token, x.value, x.scope);
            string op = OS.Peek();
            if (op == "=="||op=="!=")
            {
                if((x.dataType != y.dataType) && (y.dataType != "null"))
                {
                    SemanticOperationError(x, y, OS.Pop());
                }
            }
            else if ((x.dataType != y.dataType) )
            {
                /*if(x.dataType == "int" || x.dataType == "char")
                {
                   
                }
                else
                {*/
                    SemanticOperationError(x, y, OS.Pop());
               // }
            }

            z.symid = "t" + uniqueCounter++;
            z.value += "_"+ y.value;
            z.scope = "t";
            z.dataType = "bool";
            string[] data = { "returnType:" + z.dataType, "accessMod:private" };
            Symbol temp = new Symbol("t", z.symid, z.value, "tvar", data);
            SymbolTable.Add(temp);
            if (op == "<")
            {
                ICode.LT(x.symid, y.symid, z.symid);
            }
            else if (op == ">")
            {
                ICode.GT(x.symid, y.symid, z.symid);
            }
            else if (op == "!=")
            {
                ICode.NE(x.symid, y.symid, z.symid);
            }
            else if (op == "==")
            {
                ICode.EQ(x.symid, y.symid, z.symid);
            }
            else if (op == "<=")
            {
                ICode.LE(x.symid, y.symid, z.symid);
            }
            else if (op == ">=")
            {
                ICode.GE(x.symid, y.symid, z.symid);
            }
            SAS.Push(z);
            OS.Pop();
            OSprecidence.Pop();
            return;
        }

        public static void LogicOperator()
        {
            SAR y = SAS.Pop();
            SAR x = SAS.Pop();
            SAR z = new SAR(SAR.SARtype.Identifier, x.token, x.value, x.scope);
            string op = OS.Peek();
            if ((x.dataType != y.dataType) || (x.dataType!="bool"))
            {
                SemanticOperationError(x, y, OS.Pop());
            }
            z.symid = "t" + uniqueCounter++;
            z.value += "_" + y.value;
            z.scope = "t";
            z.dataType = "bool";
            string[] data = { "returnType:" + z.dataType, "accessMod:private" };
            Symbol temp = new Symbol("t", z.symid, z.value, "tvar", data);
            SymbolTable.Add(temp);
            if (op == "and")
            {
                ICode.AND(x.symid, y.symid, z.symid);
            }
            else if (op == "or")
            {
                ICode.OR(x.symid, y.symid, z.symid);
            }
            SAS.Push(z);
            OS.Pop();
            OSprecidence.Pop();
            return;
        }

    }
}
