using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public static class SymbolTable
    {
        private static Dictionary<string,Symbol> symbolTable=new Dictionary<string, Symbol>();
        private static Dictionary<string, List<Symbol>> ScopeTable = new Dictionary<string, List<Symbol>>();
        public static void Add(Symbol data)
        {
            if (symbolTable.ContainsKey(data.symid))
            {
                if(data.symid[0]== 'L'|| data.symid[0] == 'P'|| data.symid[0] == 't')
                {
                    Parser.offset += 4;
                    Parser.methodSize += 4;
                }
                return;
            }
            if (data.symid[0] == 'M' || data.symid[0] == 'X' || data.symid[0] == 'Y' || data.symid[0] == 'C')
            {
                //Console.WriteLine(data.value + " " + data.symid + " size: " + data.size);
            }
            else if (data.symid[0] == 'V')
            {
                data.offset = Parser.classMemberOffset;
                Parser.classMemberOffset += data.size;
            }
            else if (data.symid[0] == 'N')
            {
                data.size = 4;
            }
            else if (data.symid[0] == 'H' || data.symid[0] == 'B')
            {
                data.size = 1;
            }
            else if (data.symid[0] == 'L')
            {
                data.offset = Parser.offset;
                Parser.offset += 4;
                Parser.methodSize += 4;
            }
            else if (data.symid[0] == 'P')
            {
                data.offset = Parser.offset;
                Parser.offset += 4;
            }
            else if (data.symid[0] == 't')//temp variable
            {
                string scope;
                string methodName = "";
                bool staticInit = false;
                Symbol symbol;
                data.size = 4;
                data.offset = Parser.offset;
                data.scope = Parser.scope;
                Parser.offset += 4;
                Parser.methodSize += 4;
                if (data.scope.Split('.')[1] == "main")
                {
                    methodName = "main";
                    scope = "g";
                }
                else if (data.scope.Split('.').Length == 3)//temporary variable in a method g.class.method
                {
                    methodName = data.scope.Split('.')[2];
                    scope = "g." + data.scope.Split('.')[1];
                }
                else
                //temp variable is in static initializer
                {
                    methodName = data.scope.Split('.')[1] + "StaticInit";
                    staticInit = true;
                    scope = "g." + data.scope.Split('.')[1];
                }
                for (int i = 0; i < ScopeTable[scope].Count; ++i)
                {
                    symbol = ScopeTable[scope][i];
                    if (symbol.value.Equals(methodName))
                    {
                        if (staticInit)
                        {
                            Parser.classTempMemberOffset += 4;
                            symbol.size += Parser.classTempMemberOffset;
                        }
                        else
                        {
                            symbol.size = Parser.methodSize;
                        }
                        symbolTable[symbol.symid].size = symbol.size;
                        break;
                    }
                }
            }
            data.address = Parser.address;
            if (!ScopeTable.ContainsKey(data.scope))
            {
                ScopeTable.Add(data.scope, new List<Symbol>());
            }
            symbolTable.Add(data.symid, data);
            ScopeTable[data.scope].Add(data);
            Parser.address += data.size;
            //data.Print();
        }

        public static string iExists(SAR s)
        {
            Symbol symbol;
            Symbol check;
            Symbol defined;
            bool flag = false;
            string tempScope = "";
            string scope = s.scope;
            string parameterTypes = "";
            int definedParamCount;
            while (scope != "")
            {
                if (ScopeTable.ContainsKey(scope))
                {
                    for(int i = 0; i < ScopeTable[scope].Count; ++i)
                    {
                        symbol = ScopeTable[scope][i];
                        if (symbol.value.Equals(s.value) && symbol.symid[1] !='A')
                        {
                            tempScope = scope;
                            if (symbol.parameters.Split(',')[0] != "")
                            {
                                definedParamCount= symbol.parameters.Split(',').Length;
                            }
                            else
                            {
                                definedParamCount = 0;
                            }
                            if (s.dataType == "@")
                            {
                                string type = symbol.data[0].Split(':')[1];
                                if (type.Length<1 || type[0] != '@')
                                {
                                    continue;
                                }
                                if (type[0] == '@')
                                {
                                    type = type.Substring(1, type.Length - 1);
                                }
                                if (s.value == "i")
                                {
                                    Console.Write(" ");
                                }
                                string symid = "tArr" + SemanticActions.uniqueCounter++;
                                string[] data = new string[2];
                                data[0] = "dataType:"+ type;
                                data[1] = "accessMod:public";
                                Symbol temp = new Symbol("t", symid, "t"+s.value, "tvar", data);
                                Add(temp);
                                s.dataType = data[0];
                                s.symid = symid;
                                s.scope = "t";
                                ICode.AEF(symbol.symid, s.argList[0], temp.symid);
                                return s.symid;
                            }
                            else if (s.argList.Count != definedParamCount)
                            {
                                for (int j = 0; j < s.argList.Count; ++j)
                                {
                                    parameterTypes += symbolTable[s.argList[s.argList.Count - j - 1]].data[0].Split(':')[1]+",";
                                }
                                continue;
                            }
                            else
                            {
                                parameterTypes = "";
                                for(int j = 0; j < s.argList.Count; ++j)
                                {
                                    check = symbolTable[s.argList[j]];//args are in reverse order
                                    defined = symbolTable[symbol.parameters.Split(',')[j]];
                                    if (check.data[0].Split(':')[1] != defined.data[0].Split(':')[1])
                                    {
                                        flag = true;
                                    }
                                    parameterTypes += check.data[0].Split(':')[1]+",";
                                }
                                if (flag)
                                {
                                    continue;
                                }
                            }
                            s.symid = ScopeTable[scope][i].symid;
                            s.scope = scope;
                            return s.symid;
                        }
                    }
                }
                if (!scope.Contains("."))
                {
                    if(parameterTypes!=null && parameterTypes.Length > 0)
                    {
                        s.value += "(" + parameterTypes.Substring(0,parameterTypes.Length-1) + ")";
                    }
                    if (tempScope != "")
                    {
                        s.scope = tempScope;
                    }
                    return null;
                }
                string[] scopes = scope.Split('.');
                scope = scopes[0];
                for (int i = 1; i < scopes.Length-1; ++i)
                {
                    scope += "."+scopes[i];
                }
            }
            s.value += "(" + parameterTypes.Substring(0, parameterTypes.Length - 1) + ")";
            s.scope = tempScope;
            return null;
        }

        public static Symbol rExists(SAR s)
        {
            Symbol temp=null;
            string args="";
            string scope = "g";
            bool func = false;
            bool match = false;
            string[] parameters= { };
            for (int i = 0; i < ScopeTable[scope].Count; ++i)
            {
                if (ScopeTable[scope][i].value.Equals(s.classType))
                {
                    scope += "." + s.classType;
                    for (int j = 0; j < ScopeTable[scope].Count; ++j)
                    {
                        temp = ScopeTable[scope][j];
                        if(temp.kind == "Param")
                        {
                            continue;
                        }
                        if (temp.value == s.value)
                        {
                            match = true;
                            s.symid = temp.symid;
                        }
                        else
                        {
                            continue;
                        }
                        if ((temp.kind == "method" || temp.kind == "constructor"))
                        {
                            func = true;
                            match = false;
                            if (temp.parameters == "")
                            {
                                if (s.argList.Count == 0)
                                {
                                    s.symid = temp.symid;
                                    match = true;
                                    break;
                                }
                                continue;
                            }
                            args = "";
                            for (int q = 0; q < s.argList.Count; ++q)
                            {
                                if (q != 0)
                                {
                                    args += ",";
                                }
                                args += symbolTable[s.argList[q]].data[0].Split(':')[1];
                            }
                            parameters = temp.parameters.Split(',');
                            if (s.argList.Count != parameters.Length)//method need to have matching number of parameters
                            {
                                continue;
                            }
                            Symbol definitionArg;
                            Symbol passedArg;
                            bool error = false;
                            for (int p = 0; p < parameters.Length; ++p)
                            {
                                definitionArg = SymbolTable.GetValue(parameters[p]);
                                passedArg = SymbolTable.GetValue(s.argList[p]);
                                if (definitionArg.data[0] != passedArg.data[0])
                                {
                                    error = true;
                                    break;
                                }
                            }
                            if (error)
                            {
                                continue;
                            }
                            s.symid = temp.symid;
                            match = true;
                            break;
                        }
                        else if (match == true)
                        {
                            break;
                        }
                    }
                    if (!match)
                    {
                        if (func)
                        {
                            s.value += "(" + args + ")";
                        }
                        s.scope = "class " + s.classType;
                        return null;
                    }
                    /*string symid = "t" + SemanticActions.uniqueCounter++;
                    Symbol ref_sar = new Symbol(s.scope, symid, s.value, "ref_sar", temp.data);
                    ref_sar.parameters = args;
                    symbolTable.Add(symid, ref_sar);*/
                    return temp;
                }
            }
            s.scope = "class " + s.classType;
            if (func)
            {
                s.value += "(";
                for (int i = 0; i< s.argList.Count; ++i)
                {
                    if (i != 0)
                    {
                        s.value += ",";
                    }
                    s.value += symbolTable[s.argList[i]].data[0].Split(':')[1];
                }
                 s.value+=  ")";
            }
            s.value += " is";
            return null;
        }

        public static bool tExists(SAR s)
        {
            string scope = "g";
            for (int i = 0; i < ScopeTable[scope].Count; ++i)
            {
                if (ScopeTable[scope][i].value.Equals(s.value))
                {
                    s.symid = ScopeTable[scope][i].symid;
                    s.scope = scope;
                    return true;
                }
            }
            return false;
        }

        public static bool dup(Token t, string scope)
        {
            int count = 0;
            for (int i = 0; i < ScopeTable[scope].Count; ++i)
            {
                if (ScopeTable[scope][i].value.Equals(t.lexeme))
                {
                    ++count;
                }
            }
            if (count > 1)
            {
                return true;
            }
            return false;
        }

        public static bool return_(SAR s,string scope)
        {
            string[] temp = scope.Split('.');
            scope = "g";
            if (temp.Length > 2)
            {
                scope += ("."+ temp[1]);
            }
            string funcName = temp[temp.Length - 1]; //get the name of enclosing function
            for (int i = 0; i < ScopeTable[scope].Count; ++i)
            {
                if (ScopeTable[scope][i].value.Equals(funcName))
                {
                    string returnType = ScopeTable[scope][i].data[0].Split(':')[1];
                    if (s == null && returnType == "void")
                    {
                        return true;
                    }
                    if (s.dataType == returnType)
                    {
                        s.dataType = returnType;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool NewObj(SAR s,string[] argsPassed)
        {
            Symbol definition;
            string[] argsDefined= { };
            string scope = "g."+s.value;
            s.paramType = "";
            for(int i = 0; i < ScopeTable["g"].Count; ++i)
            {
                if (ScopeTable["g"][i].value == s.value)
                {
                    s.size = ScopeTable["g"][i].size;
                }
            }
            for (int i = 0; i < ScopeTable[scope].Count; ++i)
            {
                if (ScopeTable[scope][i].value.Equals(s.value))
                {
                    definition = ScopeTable[scope][i];
                    s.symid = definition.symid;
                    if (definition.parameters != ""&&definition.parameters != null)
                    {
                        argsDefined = definition.parameters.Split(',');
                    }
                    if (argsPassed.Length != argsDefined.Length)
                    {
                        continue;
                    }
                    bool flag = false;
                    for(int j = 0; j < argsPassed.Length; ++j)
                    {
                        if (symbolTable[argsPassed[j]].data[0].Split(':')[1] != symbolTable[argsDefined[j]].data[0].Split(':')[1])
                        {
                            flag = true;
                            break;
                        }
                        if (j != 0)
                        {
                            s.paramType += ",";
                        }
                        s.paramType += symbolTable[argsPassed[j]].data[0].Split(':')[1];
                    }
                    if (flag)
                    {
                        continue;
                    }
                    s.dataType = definition.data[0].Split(':')[1];
                    string className = scope.Split('.')[1];
                    foreach (Symbol classSymbol in ScopeTable["g"])
                    {
                        if(classSymbol.value== className)
                        {
                            s.size = classSymbol.size;
                            break;
                        }
                    }
                    return true;
                }
            }
            s.value += "(" + s.paramType + ")";
            s.scope = scope;
            return false;
        }

        public static bool ContainsSymid(string s)
        {
			return symbolTable.ContainsKey(s);
		}

        public static Symbol GetValue(string symid)
        {
            if (!symbolTable.ContainsKey(symid))
            {
                return null;
            }
            return symbolTable[symid];
        }
        public static List<Symbol> GetSymbolsInScope(string scope)
        {
            if (!ScopeTable.ContainsKey(scope))
            {
                return null;
            }
            return ScopeTable[scope];
        }
        public static void printAll()
        {
            foreach(Symbol s in symbolTable.Values)
            {
                if (s.symid[0] == 'M' || s.symid[0] == 'X' || s.symid[0] == 'Y' || s.symid[0] == 'C')
                {
                    Console.WriteLine(s.symid + " " + s.value + " " + " size: " + s.size);
                }
                else
                {
                    Console.WriteLine(s.symid + " " + s.value + " " + " offset: " + s.offset);
                }
            }
        }
    }

}
