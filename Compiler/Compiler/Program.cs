using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Program
    {
        static int Main(string[] args)
        {
            bool printFile = false;
            bool printConsole = false;
            string fileName;
            string outputFileName=null;
            if (args.Length == 0)
            {
                Console.Write("Enter filename with file extension pxi: ");
                return 0;
            }
            else
            {
                if(args.Length > 1){
                    if (args.Length > 2)
                    {
                        outputFileName = args[2];
                    }
                    if (args[1].ToString().ToLower()=="f")
                    {
                        printFile = true;
                    }
                    if (args[1].ToString().ToLower() == "c")
                    {
                        printConsole = true;
                    }
                }
                if (args[0] == "help")
                {
                    Console.WriteLine("---- f for ouput tokens to file, c for display tokens on console ----");
                    return 0;
                }
                fileName = args[0];
            }
            if (!fileName.Contains(".pxi"))
            {
                if (fileName.Contains("."))
                {
                    Console.WriteLine("Requires file with .pxi extension");
                    return 0;
                }
                fileName = fileName + ".pxi";
            }
            if (!File.Exists(fileName))
            {
                Console.WriteLine("{0} not found.", fileName);
                return 0;
            }
			
			
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception)
            {
                Console.WriteLine("could not open file {0}", fileName);
                return 0;
            }
            
			Scanner.ScanFile(ref sr);
			if (printFile)
			{
				Scanner.PrintTokensFile(outputFileName);
			}
			else if(printConsole)
			{
				Scanner.PrintTokensConsole();
            }
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            sr.BaseStream.Position = 0;
            Scanner.ScanFile(ref sr);
			Parser.ParseTokens();
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            sr.BaseStream.Position = 0;
            ICode.SetFilename(fileName);
            Scanner.ScanFile(ref sr);   //semantic pass
			Parser.ParseTokens();
            ICode.Flush();
            //SymbolTable.printAll();
            return 0;
            
        }
    }
}
