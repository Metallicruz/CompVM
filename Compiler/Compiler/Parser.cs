using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public static class Parser
    {
        private static int classSize;
        private static int sizeParameters;
        private static bool semanticPass = false;
        private static string currentClassConstructorSymid;
        private static string parameters;
        private static string currentIdentifier;
        private static string currentType;
        private static string accessMod;
        private static string currentMethodName;
        public static string scope;
        public static int methodSize;
        public static int address;
        public static int offset;
        public static int globalOffset;
        public static int classMemberOffset;
        public static int classTempMemberOffset;
        private static int uniqueCounter;
        public static List<string> memberVariables;
        public static Token identifierToken;
        public static Symbol identifierSymbol;
        public static void ParseTokens()
        {
            accessMod = "public";
            uniqueCounter = 100;
            scope = "g";
            parameters = "";
            currentType="";
            currentIdentifier = "";
            if (!Start())
            {
				SyntaxError(Tokens.GetToken(), "Start Symbol");
			}
            if(Tokens.GetToken().type != Token.Type.EOF)
            {
                SyntaxError(Tokens.GetToken(), "EOF");
            }
            semanticPass = true;
        }

        private static bool type()
        {
            Token.Type tokenType = Tokens.GetToken().type;
            if (tokenType != Token.Type.TypeKeyword && tokenType != Token.Type.Identifier)
            {
                return false;
            }
            currentType = Tokens.GetToken().lexeme;
            if (semanticPass)
            {
                   SemanticActions.tPush(Tokens.GetToken(), scope);
            }
            return true;
        }
        private static bool character_literal()
        {
            if (Tokens.GetToken().type == Token.Type.Character)
            {
                string value = Tokens.GetToken().lexeme.Replace("\'", "");
                string[] data = new string[2];
                data[0] = "returnType:char";
                data[1] = "accessMod:public";
                Symbol symbol = new Symbol("g", ("H" + value.Trim()), "CHAR_"+value, "clit", data);
                SymbolTable.Add(symbol);
                if (semanticPass)
                {
                    SemanticActions.lPush(symbol, Tokens.GetToken(), scope);
                }
                return true;
            }
            return false;
        }

        private static bool numeric_literal()
        {
            if (Tokens.GetToken().lexeme == "+" || Tokens.GetToken().lexeme == "-")
            {
                if (Tokens.PeekToken().type != Token.Type.Number)
                {
                    return false;
                }
                string sign = Tokens.GetToken().lexeme;
                if (sign == "+")
                {
                    sign = "";
                }
                Tokens.NextToken();
                string[] data = new string[2];
                data[0] = "returnType:int";
                data[1] = "accessMod:public";
                Symbol symbol = new Symbol("g", ("N" + sign + Tokens.GetToken().lexeme), (sign+Tokens.GetToken().lexeme), "ilit", data);
                SymbolTable.Add(symbol);
                if (semanticPass)
                {
                    SemanticActions.lPush(symbol, Tokens.GetToken(), scope);
                }
            }
            else if (Tokens.GetToken().type == Token.Type.Number)
            {
                string[] data = new string[2];
                data[0] = "returnType:int";
                data[1] = "accessMod:public";
                Symbol symbol = new Symbol("g", ("N" + Tokens.GetToken().lexeme), Tokens.GetToken().lexeme, "ilit", data);
                SymbolTable.Add(symbol);
                if (semanticPass)
                {
                    SemanticActions.lPush(symbol, Tokens.GetToken(), scope);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

		private static bool Start()
        {
            globalOffset = 12;//first line in assembly is a jmp
            if (!semanticPass)
            {
                address = 0;
                offset = 0;
                memberVariables = new List<string>();
            }
			while(ClassDeclaration());
			if(!Tokens.GetToken().lexeme.Equals("void"))
            {
				SyntaxError(Tokens.GetToken(), "void return for main");
			}
			Tokens.NextToken();
			if(!Tokens.GetToken().lexeme.Equals("pxi"))
            {
				SyntaxError(Tokens.GetToken(), "pxi");
			}
			Tokens.NextToken();
			if(!Tokens.GetToken().lexeme.Equals("main"))
            {
				SyntaxError(Tokens.GetToken(), "main method");
            }
            string[] data = new string[2];
            data[0] = "returnType:void";
            data[1] = "accessMod:public";
            Symbol symbol = new Symbol("g", ("MAIN"), Tokens.GetToken().lexeme, "main", data);
            SymbolTable.Add(symbol);
            currentMethodName = "M";
            identifierToken = Tokens.GetToken();
            currentIdentifier = "";
            scope += ".main";
            if (semanticPass)
            {
                ICode.FUNC(symbol.symid);
            }
            Tokens.NextToken();
			if(Tokens.GetToken().lexeme!="(")
            {
				SyntaxError(Tokens.GetToken(), "(");
			}
			Tokens.NextToken();
			if(Tokens.GetToken().lexeme!=")"){
				SyntaxError(Tokens.GetToken(), ")");
			}
			Tokens.NextToken();
			if(!MethodBody()){
				SyntaxError(Tokens.GetToken(), "method body");
            }
            return true;
		}
		
		private static bool ClassDeclaration()
        {
			if(Tokens.GetToken().lexeme!="class")
            {
				return false;
			}
			Tokens.NextToken();
            scope = "g" + "." + Tokens.GetToken().lexeme;
            if (Tokens.GetToken().type!=Token.Type.Identifier)
            {
				SyntaxError(Tokens.GetToken(), "an identifer");
			}
            Symbol symbol = new Symbol("g", ("C"+uniqueCounter++), Tokens.GetToken().lexeme, "Class", null);
            identifierToken = Tokens.GetToken();
            if (semanticPass)
            {
                SemanticActions.dup(identifierToken, "g");
                ICode.StaticInit();
            }
            Tokens.NextToken();
			if(Tokens.GetToken().lexeme!="{")
            {
				SyntaxError(Tokens.GetToken(), "{");
			}
			Tokens.NextToken();
            classSize = 0;
            classMemberOffset = 0;
            classTempMemberOffset = 0;
            while (ClassMemberDeclaration()){}
            if (semanticPass)
            {
                ICode.StaticInitInsertVars();
            }
            if (Tokens.GetToken().lexeme!="}")
            {
				SyntaxError(Tokens.GetToken(), "modifier or constructor or a closing brace");
			}
            else
            {
                memberVariables.Add(currentClassConstructorSymid);
            }
			Tokens.NextToken();
            symbol.size = classMemberOffset;
            SymbolTable.Add(symbol);
            scope ="g";
            return true;
			
		}
		
		private static bool ClassMemberDeclaration()
        {
			if(Tokens.GetToken().lexeme!="public" &&
			Tokens.GetToken().lexeme!="private")
            {
                parameters = "";
                methodSize = 0;
                return ConstructorDeclaration();
			}
			accessMod=Tokens.GetToken().lexeme;
			Tokens.NextToken();
            if (!type())
            {
                SyntaxError(Tokens.GetToken(), "a type");
                return false;
            }
            if (semanticPass)
            {
                SemanticActions.tExist();
            }
            Tokens.NextToken();
			if(Tokens.GetToken().type!=Token.Type.Identifier)
            {
				SyntaxError(Tokens.GetToken(), "identifer");
			}
            currentIdentifier = Tokens.GetToken().lexeme;
            identifierToken = Tokens.GetToken();
            if (semanticPass)
            {
                SemanticActions.dup(identifierToken, scope);
            }
            Tokens.NextToken();
            Symbol symbol=null;
            if(Tokens.GetToken().lexeme == "[")
            {
                currentType = "@"+currentType;
            }
            if (Tokens.GetToken().lexeme == "["||Tokens.GetToken().lexeme == "="||Tokens.GetToken().lexeme==";")
            {
                string[] data = new string[2];
                data[0] = "returnType:" + currentType;
                data[1] = "accessMod:"+accessMod;
                symbol = new Symbol(scope, ("V" + uniqueCounter++), currentIdentifier, "ivar", data);
                SymbolTable.Add(symbol);
                if (!semanticPass)
                {
                    memberVariables.Add(symbol.symid);
                }
                classSize += symbol.size;
            }
            identifierSymbol = symbol;
            if (!FieldDeclaration())
            {
				SyntaxError(Tokens.GetToken(), "field declaration");
			}
			accessMod="public";
			return true;
		}
		
		private static bool FieldDeclaration()
        {
            if (Tokens.GetToken().lexeme == "(")
            {
                Tokens.NextToken();
                parameters = "";
                string methodType = currentType;
                offset = 0;
                sizeParameters = 0;
                ParameterList();
                methodSize = 0;
                string[] data = new string[2];
                data[0] = "returnType:" + methodType;
                data[1] = "accessMod:" + accessMod;
                Symbol symbol = new Symbol(scope, ("M" + uniqueCounter++), currentIdentifier, "method", data);
                symbol.parameters=parameters;
                currentMethodName = "M";
                if(semanticPass)
                {
                    ICode.FUNC(symbol.symid);
                }
                if (Tokens.GetToken().lexeme != ")")
                {
                    SyntaxError(Tokens.GetToken(), ")");
                }
                Tokens.NextToken();
                if (!MethodBody())
                {
                    SyntaxError(Tokens.GetToken(), "method body");
                }
                symbol.size = methodSize;
                SymbolTable.Add(symbol);
                offset = 0;
                methodSize = 0;
                return true;
            }
            else
            {
                bool flag = false;
                if (Tokens.GetToken().lexeme == "[")
                {
                    flag = true;
                    if (Tokens.PeekToken().lexeme != "]")
                    {
                        return false;
                    }
                    Tokens.NextToken();
                    Tokens.NextToken();
                }
                if (semanticPass)
                {
                    SemanticActions.vPush(identifierSymbol,identifierToken, scope);
                }
                if (Tokens.GetToken().lexeme == "=")
                {
                    flag = true;
                    if (semanticPass)
                    {
                        SemanticActions.oPush(Tokens.GetToken());
                    }
                    Tokens.NextToken();
                    if (!AssignmentExpression())
                    {
                        return false;
                    }
                }
                if ((Tokens.GetToken().lexeme == ";"))
                {
                    if (semanticPass)
                    {
                        SemanticActions.EOE();
                    }
                    Tokens.NextToken();
                    return true;
                }
                if (flag)
                {
                    SyntaxError(Tokens.GetToken(), ";");
                }
            }
			return false;
		}
		
		private static bool ConstructorDeclaration()
        {
			if(Tokens.GetToken().type!=Token.Type.Identifier){
				return false;
			}
            currentIdentifier = Tokens.GetToken().lexeme;
            identifierToken = Tokens.GetToken();
            if (semanticPass)
            {
                SemanticActions.dup(identifierToken, scope);
                SemanticActions.CD(identifierToken,scope);
            }
            Tokens.NextToken();
			if(Tokens.GetToken().lexeme!="("){
				SyntaxError(Tokens.GetToken(), "(");
            }
            Tokens.NextToken();
            offset = 0;
            sizeParameters = 0;
            ParameterList();
            string[] data = new string[2];
            data[0] = "returnType:"+currentIdentifier;
            data[1] = "accessMod:"+accessMod;
            Symbol symbol = new Symbol(scope, ("X" + uniqueCounter++), currentIdentifier, "constructor", data);
            currentMethodName = symbol.symid;
            if (!semanticPass)
            {
                currentClassConstructorSymid = symbol.symid;
            }
            data[0] = "returnType:"+ currentIdentifier;
            data[1] = "accessMod:"+accessMod;
            Symbol symbol2 = new Symbol(scope, ("Y" + symbol.symid.Substring(1)), currentIdentifier+"StaticInit", "Init", data);
            if (semanticPass)
            {
                ICode.RETURN("this");
                ICode.FUNC(symbol.symid);
                ICode.FRAME(symbol2.symid, "this");
                ICode.CALL(symbol2.symid);
            }
            symbol.parameters = parameters;
            if (Tokens.GetToken().lexeme!=")"){
				SyntaxError(Tokens.GetToken(), ")");
			}
			Tokens.NextToken();
			if(!MethodBody()){
				SyntaxError(Tokens.GetToken(), "method body");
            }
            symbol.size = methodSize;
            symbol2.size = 0;
            SymbolTable.Add(symbol);
            SymbolTable.Add(symbol2);
            offset = 0;
            methodSize = 0;
            return true;
		}
		
		private static bool MethodBody()
        {
			if(Tokens.GetToken().lexeme!="{"){
				return false;
			}
            if (currentIdentifier != "")
            {
                scope +="."+ currentIdentifier;
            }
			Tokens.NextToken();
			while(VariableDeclaration());
			while(Statement());
            if (semanticPass)
            {
                if (currentMethodName[0] == 'X')
                {
                    ICode.RETURN("this");
                }
                else
                {
                    ICode.RTN();
                }
            }
            if (Tokens.GetToken().lexeme!="}")
            {
				SyntaxError(Tokens.GetToken(), "}  \n~Variable declarations after a statement not allowed~");
			}
            Tokens.NextToken();
            scope = leaveScope(scope);
            return true;
		}
		
		private static bool VariableDeclaration()
        {
            if (Tokens.PeekToken().type != Token.Type.Identifier)
            {
                return false;
            }
            if (!type())
            {
                return false;
            }
            if (semanticPass)
            {
                SemanticActions.tExist();
            }
			Tokens.NextToken();
            currentIdentifier = Tokens.GetToken().lexeme;
            identifierToken = Tokens.GetToken();
            Tokens.NextToken();
			if(Tokens.GetToken().lexeme=="["){
				Tokens.NextToken();
                currentType = "@"+currentType;
				if(Tokens.GetToken().lexeme!="]"){
					SyntaxError(Tokens.GetToken(), "]");
				}
				Tokens.NextToken();
            }
            string[] data = new string[2];
            data[0] = "returnType:"+currentType;
            data[1] = "accessMod:"+accessMod;
            Symbol symbol = new Symbol(scope, ("L" + uniqueCounter++), currentIdentifier, "lvar", data);
            identifierSymbol = symbol;
            if (semanticPass)
            {
                SemanticActions.dup(identifierToken, scope);
                SemanticActions.vPush(symbol, identifierToken, scope);
            }
            SymbolTable.Add(symbol);
            if (Tokens.GetToken().lexeme=="=")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!AssignmentExpression()){
					SyntaxError(Tokens.GetToken(), "assignment expression");
				}
			}
			if(Tokens.GetToken().lexeme!=";"){
				SyntaxError(Tokens.GetToken(), ";");
            }
            if (semanticPass)
            {
                SemanticActions.EOE();
            }
            Tokens.NextToken();
			return true;
		}
		
		private static bool ParameterList()
        {
            if (!Parameter()){
				return false;
			}
            sizeParameters = 4;
            while (Tokens.GetToken().lexeme==","){
				Tokens.NextToken();
                parameters += ",";
				if(!Parameter()){
					SyntaxError(Tokens.GetToken(), "parameter");
                    return false;
				}
                sizeParameters += 4;
            }
			return true;
		}
		
		private static bool Parameter()
        {
			if(!type())
            {
				return false;
            }
            if (semanticPass)
            {
                SemanticActions.tExist();
            }
            Tokens.NextToken();
			if(Tokens.GetToken().type!=Token.Type.Identifier){
				SyntaxError(Tokens.GetToken(), "identifer");
			}
            identifierToken = Tokens.GetToken();
            if (semanticPass)
            {
                SemanticActions.dup(identifierToken, (scope+"." + currentIdentifier));
            }
            if (Tokens.PeekToken().lexeme == "[")
            {
                currentType = "@"+currentType;
            }
            parameters += "P"+uniqueCounter;
            string[] data = new string[2];
            data[0] = "returnType:"+currentType;
            data[1] = "accessMod:"+accessMod;
            Symbol symbol = new Symbol((scope+"."+currentIdentifier), ("P" + uniqueCounter++), Tokens.GetToken().lexeme, "Param", data);
			Tokens.NextToken();
			if(Tokens.GetToken().lexeme=="["){
				Tokens.NextToken();
				if(Tokens.GetToken().lexeme !="]"){
					SyntaxError(Tokens.GetToken(), "]");
				}
			    Tokens.NextToken();
            }
            SymbolTable.Add(symbol);
            return true;
		}

        private static bool Statement()
        {
			if(Tokens.GetToken().lexeme=="{"){
				Tokens.NextToken();
				while(Statement());
				if(Tokens.GetToken().lexeme!="}"){
					SyntaxError(Tokens.GetToken(), "}");
				}
				Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="if")
            {
				Tokens.NextToken();
				if(Tokens.GetToken().lexeme!="("){
					SyntaxError(Tokens.GetToken(), "(");
                }
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Expression())
                {
					SyntaxError(Tokens.GetToken(), "an expression");
				}
				if(Tokens.GetToken().lexeme!=")")
                {
					SyntaxError(Tokens.GetToken(), ")");
                }
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.if_(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Statement())
                {
					SyntaxError(Tokens.GetToken(), "a statement");
                }
                if (Tokens.GetToken().lexeme=="else")
                {
					Tokens.NextToken();
                    if (semanticPass)
                    {
                        string SKIPELSE = ICode.SKIPELSE + ICode.labelCounter++ +" ";
                        ICode.JMP(SKIPELSE);
                        ICode.StackElse(SKIPELSE);
                        ICode.Print(ICode.StackIf());
                    }
					if(!Statement())
                    {
						SyntaxError(Tokens.GetToken(), "a statement");
                    }
                    if (semanticPass)
                    {
                        ICode.Print(ICode.StackElse());
                    }
                }
                else if(semanticPass)
                {
                    ICode.Print(ICode.StackIf());
                }
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="while")
            {
                if (semanticPass)
                {
                    string BEGIN = ICode.BEGIN + ICode.labelCounter++ + " ";
                    ICode.Print(BEGIN);
                    ICode.StackWhile(BEGIN);
                }
                Tokens.NextToken();
                if (Tokens.GetToken().lexeme!="("){
					SyntaxError(Tokens.GetToken(), "(");
                }
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Expression()){
					SyntaxError(Tokens.GetToken(), "an expression");
				}
				if(Tokens.GetToken().lexeme!=")"){
					SyntaxError(Tokens.GetToken(), ")");
                }
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.while_(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Statement()){
					SyntaxError(Tokens.GetToken(), "a statement");
                }
                if (semanticPass)
                {
                    string BEGIN = ICode.StackWhile();
                    ICode.JMP(BEGIN);
                    string ENDWHILE = ICode.StackEndWhile();
                    ICode.Print(ENDWHILE);
                }
                return true;
			}
			
			if(Tokens.GetToken().lexeme=="return")
            {
				Tokens.NextToken();
                Expression();
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.return_(Tokens.GetToken(),scope);
                }
                Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="cout"){
				Tokens.NextToken();
				if(Tokens.GetToken().lexeme!="<<"){
					SyntaxError(Tokens.GetToken(), "<<");
				}
				Tokens.NextToken();
				if(!Expression()){
					SyntaxError(Tokens.GetToken(), "an expression");
				}
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.cout();
                }
                Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="cin"){
				Tokens.NextToken();
				if(Tokens.GetToken().lexeme!=">>"){
					SyntaxError(Tokens.GetToken(), ">>");
				}
				Tokens.NextToken();
				if(!Expression()){
					SyntaxError(Tokens.GetToken(), "an expression");
				}
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.cin();
                }
                Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="spawn"){
				Tokens.NextToken();
				if(!Expression()){
					SyntaxError(Tokens.GetToken(), "an expression");
				}
				if(Tokens.GetToken().lexeme!="set"){
					SyntaxError(Tokens.GetToken(), "set");
				}
				Tokens.NextToken();
				if(Tokens.GetToken().type != Token.Type.Identifier){
					SyntaxError(Tokens.GetToken(), "an identifier");
                }
                if (semanticPass)
                {
                    SemanticActions.iPush(Tokens.GetToken(),scope);
                }
                Tokens.NextToken();
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.spawn();
                }
                Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="block"){
				Tokens.NextToken();
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.block();
                }
                Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="lock"){
				Tokens.NextToken();
				if(Tokens.GetToken().type != Token.Type.Identifier){
					SyntaxError(Tokens.GetToken(), "an identifier");
                }
                if (semanticPass)
                {
                    SemanticActions.iPush(Tokens.GetToken(), scope);
                }
                Tokens.NextToken();
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.lock_();
                }
                Tokens.NextToken();
				return true;
			}
			
			if(Tokens.GetToken().lexeme=="release"){
				Tokens.NextToken();
				if(Tokens.GetToken().type != Token.Type.Identifier){
					SyntaxError(Tokens.GetToken(), "an identifier");
                }
                if (semanticPass)
                {
                    SemanticActions.iPush(Tokens.GetToken(), scope);
                }
                Tokens.NextToken();
				if(Tokens.GetToken().lexeme!=";"){
					SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.release();
                }
                Tokens.NextToken();
				return true;
			}
			
            if (Expression())
            {
                if (Tokens.GetToken().lexeme != ";")
                {
                    SyntaxError(Tokens.GetToken(), ";");
                }
                if (semanticPass)
                {
                    SemanticActions.EOE();
                }
				Tokens.NextToken();
				return true;
            }
            return false;
        }

        private static bool Expression()
        {
			
			string lexeme = Tokens.GetToken().lexeme;
			if(lexeme=="(")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Expression()){
                    SyntaxError(Tokens.GetToken(), "expression");
				}
				if(Tokens.GetToken().lexeme!=")"){
                    SyntaxError(Tokens.GetToken(), ")");
				}
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                }
                Tokens.NextToken();
				Expressionz();
				return true;
			}
			else if(lexeme=="+"||lexeme=="-"||lexeme=="true"||lexeme=="false"||
			lexeme=="null"||lexeme=="this"||Tokens.GetToken().type==Token.Type.Number ||
			Tokens.GetToken().type==Token.Type.Character)
            {
                if (Tokens.GetToken().lexeme == "this")
                {
                    if (semanticPass)
                    {
                        SemanticActions.iPush(Tokens.GetToken(), scope);
                        SemanticActions.iExist();
                    }
                    Tokens.NextToken();
                    Member_Refz();
                    Expressionz();
                    return true;
                }
                else if (lexeme == "true" || lexeme == "false" ||lexeme == "null")
                {
                    string[] data = new string[2];
                    data[0] = "returnType:bool";
                    data[1] = "accessMod:public";
                    Symbol symbol;
                    if (lexeme == "true")
                    {
                        symbol = new Symbol("g", ("Btrue"), "1", "blit", data);
                    }
                    else if (lexeme == "false")
                    {
                        symbol = new Symbol("g", ("Bfalse"), "0", "blit", data);
                    }
                    else
                    {
                        data[0] = "returnType:null";
                        symbol = new Symbol("g", "null", "2018", "null", data);
                    }
                    SymbolTable.Add(symbol);
                    if (semanticPass)
                    {
                        SemanticActions.lPush(symbol, Tokens.GetToken(), scope);
                    }
                    Tokens.NextToken();
                    Expressionz();
                    return true;
                }
                else if (numeric_literal())
                {
                    Tokens.NextToken();
                    Expressionz();
                    return true;
                }
                else if (character_literal())
                {
                    Tokens.NextToken();
                    Expressionz();
                    return true;
                }
				return false;
			}
            else if (Tokens.GetToken().type == Token.Type.Identifier)
            {
                currentIdentifier = Tokens.GetToken().lexeme;
                if (semanticPass)
                {
                    SemanticActions.iPush(Tokens.GetToken(),scope);
                }
				Tokens.NextToken();
                Fn_Arr_Member();
                if (semanticPass)
                {
                    SemanticActions.iExist();
                }
                Member_Refz();
                Expressionz();
                return true;
            }
            return false;
        }

        private static bool Expressionz()
        {
            if(Tokens.GetToken().lexeme == "=")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
                if(!AssignmentExpression()){
					SyntaxError(Tokens.GetToken(), "assignment expression");                  
				}
				return true;
            }
			
			else if(Tokens.GetToken().lexeme=="and" || Tokens.GetToken().lexeme=="or"
			|| Tokens.GetToken().type == Token.Type.BoolSymbol || Tokens.GetToken().type == Token.Type.MathSymbol)
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Expression())
				{
					SyntaxError(Tokens.GetToken(), "expression");
				}
				return true;
			}
			
			return false;
        }

        private static bool AssignmentExpression()
        {
			if(Tokens.GetToken().lexeme == "new")
			{
				Tokens.NextToken();
                if (!type())
                {
                    SyntaxError(Tokens.GetToken(), "a type");
                    return false;
                }
                currentType = Tokens.GetToken().lexeme;
                Tokens.NextToken();
				if(!NewDeclaration()){
					SyntaxError(Tokens.GetToken(), "new  declaration");
                }
                return true;
            }
			else if(Tokens.GetToken().lexeme == "itoa"||Tokens.GetToken().lexeme == "atoi")
            {
                string lexeme = Tokens.GetToken().lexeme;
                Tokens.NextToken();
				if(Tokens.GetToken().lexeme!="("){
					SyntaxError(Tokens.GetToken(), "(");
                }
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
				if(!Expression())
				{
					SyntaxError(Tokens.GetToken(), "expression");
				}
				if(Tokens.GetToken().lexeme!=")"){
					SyntaxError(Tokens.GetToken(), ")");
				}
				Tokens.NextToken();
                if (semanticPass && (lexeme == "atoi"))
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.atoi();
                }
                else if (semanticPass && (lexeme == "itoa"))
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.itoa();
                }
                return true;
            }
            else if (Expression())
            {
                return true;
            }
            return false;
        }
		
		private static bool NewDeclaration()
		{
			if(Tokens.GetToken().lexeme == "(")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                    SemanticActions.BAL();
                }
                Tokens.NextToken();
				Argument_List();
				if(Tokens.GetToken().lexeme != ")")
				{
					SyntaxError(Tokens.GetToken(), ")");
                }
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.EAL();
                    SemanticActions.NewObj();
                }
                Tokens.NextToken();
				return true;
			}
			else if(Tokens.GetToken().lexeme == "[")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                Tokens.NextToken();
                if (!Expression())
                {
                    SyntaxError(Tokens.GetToken(), "expression");
                }
				if(Tokens.GetToken().lexeme != "]")
				{
					SyntaxError(Tokens.GetToken(), "]");
                }
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.new_arr();
                }
                Tokens.NextToken();
				return true;
			}
				
			return false;
		}
		
		private static bool Member_Refz()
        {
            if (Tokens.GetToken().lexeme == ".")
            {
				Tokens.NextToken();
				if(Tokens.GetToken().type != Token.Type.Identifier)
                {
					SyntaxError(Tokens.GetToken(), "Identifier");
				}
                if (semanticPass)
                {
                    SemanticActions.iPush(Tokens.GetToken(), scope);
                }
				Tokens.NextToken();
				Fn_Arr_Member();
                if (semanticPass)
                {
                    SemanticActions.rExist();
                }
                Member_Refz();
				return true;
            }
			return false;
		}
		
		
		private static bool Fn_Arr_Member()
        {
			if(Tokens.GetToken().lexeme=="(")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                    SemanticActions.BAL();
                }
                Tokens.NextToken();
                Argument_List();
				if(Tokens.GetToken().lexeme!=")")
                {
					SyntaxError(Tokens.GetToken(), ")");
                }
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.EAL();
                    SemanticActions.func();
                }
                Tokens.NextToken();
				return true;
			}
			else if(Tokens.GetToken().lexeme=="[")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
                currentType = "@" + currentType;
                Tokens.NextToken();
				if(!Expression()){
					SyntaxError(Tokens.GetToken(), "an expression"); 
				}
				if(Tokens.GetToken().lexeme!="]"){
					SyntaxError(Tokens.GetToken(), "]");
                }
                if (semanticPass)
                {
                    SemanticActions.ShuntYardAll();
                    SemanticActions.arr();
                }
				Tokens.NextToken();
				return true;
			}
			return false;
		}
		
		private static bool Argument_List()
        {
            if (!Expression())
            {
				return false;
            }
            while (Tokens.GetToken().lexeme==",")
            {
                if (semanticPass)
                {
                    SemanticActions.oPush(Tokens.GetToken());
                }
				Tokens.NextToken();
				if(!Expression()){
					SyntaxError(Tokens.GetToken(), "an expression");                  
				}
            }
            return true;
		}

        private static string leaveScope(string currentScope)
        {
            int index = 1;
			if(currentScope==null||currentScope==""){
				return "g";
			}
            for (int i = 0; i < currentScope.Length; ++i)
            {
                if (currentScope[i] == '.')
                {
                    index = i;
                }
            }
            return currentScope.Substring(0, index);
        }

        private static void SyntaxError(Token token, string expected)
        {
            Console.WriteLine(token.lineNumber + ": Found " + token.lexeme + " expecting " + expected);
            Environment.Exit(0);
        }
    }
}
