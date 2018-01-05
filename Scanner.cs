using System;
using System.Collections.Generic;
using System.Text;
//Added
using System.IO;

public sealed class Lexer
{
    private readonly IList<object> listLexemes;

    public Lexer(TextReader inputFile)
    {
        listLexemes = new List<object>();
        ReadProgramFile(inputFile);
    }

    public IList<object> GetLexemes
    {
        get 
        { 
            return listLexemes; 
        }
    }

    public enum Tokens
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Terminator,
        EqualTo,
        LessThan,
        GreaterThan,
        Comma,
        OpenParen,
        CloseParen,
        OpenBrac,
        CloseBrac
    }

    private void ReadProgramFile(System.IO.TextReader programFile)
    {
        while (programFile.Peek() != -1)
        {
            char symbol = (char)programFile.Peek();

            if (char.IsWhiteSpace(symbol))
            {
                // if the symbol is whitespace then move to next symbol
                programFile.Read();
            }
            else if (char.IsLetter(symbol) || symbol == '_')
            {
                //identify the tokens

                StringBuilder token = new StringBuilder();

                while (char.IsLetter(symbol) || symbol == '_')
                {
                    token.Append(symbol);
                    programFile.Read();

                    if (programFile.Peek() == -1)
                    {
                        break;
                    }
                    else
                    {
                        symbol = (char)programFile.Peek();
                    }
                }

                listLexemes.Add(token.ToString());
            }
            else if (symbol == '"')
            {
                // string literal
                StringBuilder stringLiteral = new StringBuilder();

                programFile.Read(); // skip the '"'

                if (programFile.Peek() == -1)
                {
                    throw new Exception("String literal is not terminated");
                }

                while ((symbol = (char)programFile.Peek()) != '"')
                {
                    stringLiteral.Append(symbol);
                    programFile.Read();

                    if (programFile.Peek() == -1)
                    {
                        throw new Exception("String literal is not terminated");
                    }
                }

                // skip the terminating "
                programFile.Read();
                listLexemes.Add(stringLiteral);
            }
            else if (char.IsDigit(symbol))
            {
                // numeric literal

                StringBuilder numericLiteral = new StringBuilder();

                while (char.IsDigit(symbol))
                {
                    numericLiteral.Append(symbol);
                    programFile.Read();

                    if (programFile.Peek() == -1)
                    {
                        break;
                    }
                    else
                    {
                        symbol = (char)programFile.Peek();
                    }
                }

                listLexemes.Add(int.Parse(numericLiteral.ToString()));
            }
            else switch (symbol)
            {
                case '(':
                    programFile.Read();
                    listLexemes.Add(Tokens.OpenParen);
                    break;

                case ')':
                    programFile.Read();
                    listLexemes.Add(Tokens.CloseParen);
                    break;

                case '=':
                    programFile.Read();
                    listLexemes.Add(Tokens.EqualTo);
                    break;

                case ';':
                    programFile.Read();
                    listLexemes.Add(Tokens.Terminator);
                    break;

                case '<':
                    programFile.Read();
                    listLexemes.Add(Tokens.LessThan);
                    break;

                case '>':
                    programFile.Read();
                    listLexemes.Add(Tokens.GreaterThan);
                    break;

                case ',':
                    programFile.Read();
                    listLexemes.Add(Tokens.Comma);
                    break;

                case '/':
                    programFile.Read();
                    listLexemes.Add(Tokens.Divide);
                    break;

                case '*':
                    programFile.Read();
                    listLexemes.Add(Tokens.Multiply);
                    break;

                case '-':
                    programFile.Read();
                    listLexemes.Add(Tokens.Subtract);
                    break;

                case '+':
                    programFile.Read();
                    listLexemes.Add(Tokens.Add);
                    break;

                case '{':
                    programFile.Read();
                    listLexemes.Add(Tokens.OpenBrac);
                    break;

                case '}':
                    programFile.Read();
                    listLexemes.Add(Tokens.CloseBrac);
                    break;

                default:
                    ExceptionHandler("unidentified symbol '" + symbol + "'");
                    break;
            }
        }    
    }

    public void ExceptionHandler(string strException)
    {
        throw new Exception(strException);
    }
}
