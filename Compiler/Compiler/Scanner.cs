using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public static class Scanner
    {
        public static string currentLine = "";
		public static int count = 0;
        public static string[] keyWords = new string[] { "atoi", "bool", "class", "char", "cin", "cout", "else", "false", "if", "int", "itoa", "main",
                "new", "null", "object", "public", "private", "return", "string", "this", "true", "void", "while", "spawn", "lock",
                "release", "block", "sym", "pxi", "protected", "unprotected", "and", "or" };
		public static string[] typeKeyWords = new string[]{"int","char","bool","void","sym"};
        public static char[] symbols = new char[] { '!','=', '[', ']', '{', '}', '(', ')', '+', '-', '*', '/', '<', '>' };
        private static List<Token> tokens=new List<Token>();
		private static StreamReader sr=null;
        //TokenizeFile
        //Purpose: Tokenize a pxi file line by line
        ///<param name = "fileName">Tokenize pxi file at given path.</param>
        public static void ScanFile(ref StreamReader SR)
        {
            tokens.Clear();
            Tokens.reset();
            count = 0;
			sr = SR;
        }


        //TokenizeLine
        //Purpose: Tokenize a line
        ///<param name = "fileName">Tokenize pxi file at given path.</param>
        private static void TokenizeLine()
        {
			tokens.Clear();
            ++count;
			if(sr.EndOfStream){
                tokens.Add(new Token(Token.Type.EOF, "", count));
                tokens.Add(new Token(Token.Type.EOF, "", count));
                tokens.Add(new Token(Token.Type.EOF, "", count));
                return;
			}
			string line = sr.ReadLine();
			line = line.Trim();
            while (line == "") {
                line = sr.ReadLine();
                line = line.Trim();
				++count;
            }
            int value = 0;
            int identifierIndex = -1;
            bool keyword = false;
            bool identifier = false;
            bool number = false;
			bool isSymbol = false;
            currentLine = line;
            for(int i = 0; i<line.Length; ++i)//iterate through each char
            {
                if (line.Length > i+1){
					
					if (line[i] == '/' && line[i+1] == '/')//discard comments
                    {
                        //tokens.Add(new Token(Token.Type.Comment, line.Substring(identifierIndex, i - identifierIndex), count));
                        return;
					}
				}

                if (line[i] == ' ')
                {
                    if (identifier)
                    {
                        tokens.Add(new Token(Token.Type.Identifier, line.Substring(identifierIndex, i - identifierIndex), count));
                        identifier = false;
                    }
                    continue;
                }

                if(line[i] == '\'')
                {
                    if (identifier)//previous iteration was last char of an identifier
                    {
						tokens.Add(new Token(Token.Type.Identifier, line.Substring(identifierIndex, i - identifierIndex), count));
                        identifier = false;
                    }
                    if (i<(line.Length-3) && line[i + 2]=='\'')
                    {
                        tokens.Add(new Token(Token.Type.Character, "\'"+char.ToString(line[i+1])+"\'", count));
                        i += 2;
                    }
                    else if((i<line.Length-4) && line[i+1] == '\\' && line[i+3]=='\'')
                    {
                        value = line[i + 2];
                        if ((value >= 65 && value <= 90) || (value >= 97 && value <= 122))//printable ascii char
                        {
                            tokens.Add(new Token(Token.Type.Character, line.Substring(i + 1, 2), count));
                            i += 3;
                        }
                        else//non printable ascii
                        {
                            tokens.Add(new Token(Token.Type.Unknown, line.Substring(i + 1, 2), count));
                            i += 3;
                        }
                    }
                    else
                    {
                        tokens.Add(new Token(Token.Type.Unknown, char.ToString(line[i]), count));
                    }
                    continue;
                }

                foreach (char symbol in symbols) {
                    if(line[i] == symbol)//current iteration is a symbol
                    {
                        if (identifier)//previous iteration was last char of an identifier
                        {
							tokens.Add(new Token(Token.Type.Identifier, line.Substring(identifierIndex, i - identifierIndex), count));
                            identifier = false;
                        }
						
                        if (line.Length > i+1)//don't go out of array bounds
                        {
                            if((line[i] == '<' && line[i+1]=='<') || (line[i]=='>' && line[i+1]=='>'))//"<<" ">>" in next iteration makes a double char symbol
                            {
								tokens.Add(new Token(Token.Type.Symbol, line.Substring(i, 2), count));
								isSymbol=true;
								++i;
								break;
							}
                            else if(line[i+1] == '=')//an '=' or '<' or '>' in next iteration makes a double char symbol
                            {
								if(line[i] == '=' || line[i] == '<' || line[i] == '>' ||line[i] == '!')//check all double char symbols
								{
									tokens.Add(new Token(Token.Type.BoolSymbol, line.Substring(i, 2), count));
								    isSymbol = true;
								    ++i;
                                    break;
								}
                            }
                        }
						if(line[i]=='<' || line[i]=='>')
						{
							tokens.Add(new Token(Token.Type.BoolSymbol, char.ToString(line[i]), count));
						}
						else if(line[i]=='+' || line[i]=='-' || line[i]=='*' ||line[i]=='/')
						{
							tokens.Add(new Token(Token.Type.MathSymbol, char.ToString(line[i]), count));
						}
						else{
							tokens.Add(new Token(Token.Type.Symbol, char.ToString(line[i]), count));
						}
						isSymbol = true;
                        break;
                    }
                }
				if(isSymbol){
					isSymbol = false;
					continue;
				}

                if (line[i] == ',' || line[i] == ';' ||line[i] == '.') //punctuation
                {
                    if (identifier)
                    {
						tokens.Add(new Token(Token.Type.Identifier, line.Substring(identifierIndex, i - identifierIndex), count));
                        identifier = false;
                    }
                    tokens.Add(new Token(Token.Type.Punctuation, char.ToString(line[i]), count));//create token for punctuation
                    continue;
                }
				
				if (!identifier)
				{
					foreach (string keyWord in keyWords)//search for other keywords in line
					{
						if(keyWord.Length+i <= line.Length)//check string size
						{
							string lexeme = line.Substring(i, keyWord.Length);
							if(lexeme == keyWord){//keyword starts at current index
                                if (keyWord.Length + i < line.Length)
                                {
                                    char nextValue = line[keyWord.Length +i];
                                    if ((nextValue >= 48 && nextValue <= 57) || (nextValue >= 65 && nextValue <= 90) || (nextValue >= 97 && nextValue <= 122))
                                    {
                                        continue;
                                    }
                                }
                                if (typeKeyWords.Contains(lexeme))
								{
									tokens.Add(new Token(Token.Type.TypeKeyword, lexeme, count));//create token for keyword						
								}
								else
								{
									tokens.Add(new Token(Token.Type.Keyword, lexeme, count));//create token for keyword
								}
								i += keyWord.Length - 1;//skip rest of keyword in line
                                keyword = true;
                                break;

                            }
						}
					}
                    if (keyword)
                    {
                        keyword = false;
                        continue;
                    }
                }

                if (char.IsNumber(line[i]))//current iteration is numeric
                {
                    if (identifier)//current iteration is part of an identifier
                    {
                        continue;
                    }
                    number = true;
                    for(int j = i+1; j<line.Length; ++j)//iterate until non numeric char is found
                    {
                        if (!char.IsNumber(line[j])){ //non numeric number encountered
                            tokens.Add(new Token(Token.Type.Number, line.Substring(i,j-i), count));
                            i = j-1;
                            number = false;
                            break;
                        }
                    }
                    if (number)//rest of line is all numeric
                    {
                        tokens.Add(new Token(Token.Type.Number, line.Substring(i, line.Length-i), count));
                        number = false;
                        return;
                    }
                    continue;
                }

                value = line[i];
                if((value >= 65 && value <= 90) || (value >= 97 && value <= 122))//a-z or A-Z
                {
                    if (!identifier)
                    {
                        identifierIndex = i;
                    }
                    identifier = true;
                    continue;
                }

                tokens.Add(new Token(Token.Type.Unknown, char.ToString(line[i]), count));
                
            }
            if (identifier)
            {
                tokens.Add(new Token(Token.Type.Identifier, line.Substring(identifierIndex, line.Length- identifierIndex), count));
            }
        }
		
		public static Token GetToken(ref int tokenIndex){
			if(tokenIndex>=tokens.Count){
				tokenIndex=0;
				TokenizeLine();
			}
			if(tokens.Count<=tokenIndex){
				return null;
			}
			return tokens[tokenIndex++];
		}

        //PrintToken
        //Purpose: prints out all tokens from a list
        ///<param name = "tokens">prints tokens to console</param>
        public static void PrintTokensConsole()
        {
			if(sr==null){
				return;
			}
			while(sr.Peek()!=-1 || !sr.EndOfStream)
			{
				string[] tokenData;
				TokenizeLine();
                if (tokens == null)
                {
                    continue;
                }
				foreach (Token t in tokens)
				{
					tokenData = t.GetTokenInfo();
					Console.WriteLine("{0}, Line {1}, Lexeme {2}", tokenData[0], tokenData[1], tokenData[2]);
					/*Console.WriteLine("Line: " + t.lineNumber);
					Console.WriteLine("Type: " + t.type);
					Console.WriteLine("Lexeme: " + t.lexeme);
					Console.WriteLine();*/
				}
			}
            tokens.Clear();
        }


        //PrintToken
        //Purpose: prints out all tokens from a list
        ///<param name = "tokens">prints tokens to console</param>
        public static void PrintTokensFile(string fileName)
        {
            StreamWriter sw = null;
            if (fileName == null)
            {
                fileName = "Tokens.txt";
            }
            int lineNum = 1;
            string[] tokenData;
            using (sw = new StreamWriter(fileName, false)){
                while (sr.Peek() != -1 || !sr.EndOfStream)
                {
                    TokenizeLine();
                    if (tokens == null)
                    {
                        continue;
                    }
                    foreach (Token t in tokens)
                    {
                        tokenData = t.GetTokenInfo();
                        while (int.Parse(tokenData[1]) > lineNum)
                        {
                            ++lineNum;
                            sw.WriteLine();
                        }
                        sw.Write("<Token>" + tokenData[0] + ' ' + tokenData[1] + ' ' + tokenData[2] + "</Token>");
                        /*Console.WriteLine("Line: " + t.lineNumber);
                        Console.WriteLine("Type: " + t.type);
                        Console.WriteLine("Lexeme: " + t.lexeme);
                        Console.WriteLine();*/
                    }
                }
            }

        }
    }
}
