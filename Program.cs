using System;
using System.Collections.Generic;
using System.Text;

// Added
using System.IO;
using System.Diagnostics;

namespace MCG_CS
{
    class Program
    {
        static void Main(string[] args)
        {
            bool bExit = false;

            while (bExit == false)
            {
                Console.WriteLine("\nPlease enter the file path or type quit to exit the application");
                string strFileName = Console.ReadLine();

                if (strFileName == "quit")
                    bExit = true;
                else
                {

                    try
                    {

                        Stopwatch timerParser = new Stopwatch();
                        Stopwatch timerCompiler = new Stopwatch();



                        //string strInputCodeFile = "Factorial.kcl";

                        //string strFileName = "C:\\Users\\fahad\\Desktop\\MCG-CS\\Code\\" + strInputCodeFile;

                        timerParser.Start();
                        TextReader inputFile = File.OpenText(strFileName);
                        Lexer _Lexer = new Lexer(inputFile);

                        IList<object> lexemes = _Lexer.GetLexemes;
                        Parser parser = new Parser(lexemes);

                        Console.WriteLine("\nParsing Time: " + (timerParser.ElapsedTicks).ToString());

                        timerCompiler.Start();

                        strFileName = Path.GetFileNameWithoutExtension(strFileName);
                        CodeGenerator codeGenerator = new CodeGenerator(parser.GetParsedStatement, strFileName + ".exe");

                        timerParser.Stop();
                        timerCompiler.Stop();


                        Console.WriteLine("Compiling Time: " + (timerCompiler.ElapsedTicks).ToString());
                        Console.WriteLine("Overall Time: " + (timerParser.ElapsedTicks).ToString());

                        Console.WriteLine("\nCode has been compiled successfully!");
                        Console.WriteLine("\nAn executable assembly file has been generated with the same name as input file and saved in BIN\\DEBUG folder");


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("\nPress any key to continue...");
                    }

                }
            }
        }
    }
}
