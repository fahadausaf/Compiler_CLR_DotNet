using System;
using System.Collections.Generic;
using System.Text;

public sealed class Parser
{
    private int currentToken;    
    private readonly Statement parsingOutput;
    private IList<object> listTokens;


    public Parser(IList<object> tokens)
    {
        listTokens = tokens;
        currentToken = 0;
        parsingOutput = ParseStatement();

    }

    public Statement GetParsedStatement
    {
        get { return parsingOutput; }
    }

    public void ExceptionHandler(string strException)
    {
        throw new Exception(strException);
    }
    
    private Statement ParseStatement()
    {
        Statement parsedStatement;

        if (currentToken == listTokens.Count)
        {
            ExceptionHandler("statement was expected before end of file");
        }

        if (listTokens[currentToken].Equals("print"))
        {
            currentToken++;
            Write _Write = new Write();
            _Write.Expression = ParseExpression();
            parsedStatement = _Write;
        }

        else if (listTokens[currentToken].Equals("var"))
        {
            currentToken++;
            DeclareVariable _DeclareVariable = new DeclareVariable();

            if (currentToken < listTokens.Count && listTokens[currentToken] is string)
            {
                _DeclareVariable.Identifier = (string)listTokens[currentToken];
            }
            else
            {
                ExceptionHandler("variable name was expected after 'var'");
            }

            currentToken++;

            if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.EqualTo.ToString())
            {
                ExceptionHandler("= sign was expected after 'var identifier'");
            }

            currentToken++;

            _DeclareVariable.Expression = ParseExpression();
            parsedStatement = _DeclareVariable;
        }

        else if (listTokens[currentToken].Equals("call"))
        {
            currentToken++;
            CallFunction _CallFunction = new CallFunction();
            if (currentToken < listTokens.Count && listTokens[currentToken] is string)
            {
                _CallFunction.FunctionName = (string)listTokens[currentToken];
            }
            else
            {
                ExceptionHandler("function name is expected after 'call'");
            }
            currentToken++;

            _CallFunction.Parameter1 = ParseExpression();

            //index++;

            if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.Comma.ToString())
            {
                ExceptionHandler("',' sign was expected after first parameter");
            }

            currentToken++;

            _CallFunction.Parameter2 = ParseExpression();

            //index++;

            if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.Comma.ToString())
            {
                ExceptionHandler("',' sign was expected after second parameter");
            }

            currentToken++;

            _CallFunction.Parameter3 = ParseExpression();

            //index++;

            if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.Terminator.ToString())
            {
                ExceptionHandler("';' sign was expected after third parameter");
            }

            parsedStatement = _CallFunction;
        }

        else if (listTokens[currentToken].Equals("string") || listTokens[currentToken].Equals("numeric") || listTokens[currentToken].Equals("void"))
        {            
            DeclareFunction _DeclareFunction = new DeclareFunction();
            _DeclareFunction.ReturnType = listTokens[currentToken].ToString();
            currentToken++;

            if (currentToken < listTokens.Count && listTokens[currentToken] is string)
            {
                _DeclareFunction.FunctionName = (string)listTokens[currentToken];
            }
            else
            {
                ExceptionHandler("function name is expected after return type");
            }

            currentToken++;

            if (listTokens[currentToken].Equals("var"))
            {
                currentToken++;
                DeclareVariable _DeclareVariable = new DeclareVariable();

                if (currentToken < listTokens.Count && listTokens[currentToken] is string)
                {
                    _DeclareVariable.Identifier = (string)listTokens[currentToken];
                }
                else
                {
                    ExceptionHandler("variable name was expected after 'var'");
                }

                currentToken++;

                if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.EqualTo.ToString())
                {
                    ExceptionHandler("= sign was expected after 'var identifier'");
                }

                currentToken++;

                _DeclareVariable.Expression = ParseExpression();
                _DeclareFunction.Parameter1 = _DeclareVariable;
            }

            currentToken++;

            if (listTokens[currentToken].Equals("var"))
            {
                currentToken++;
                DeclareVariable _DeclareVariable = new DeclareVariable();

                if (currentToken < listTokens.Count && listTokens[currentToken] is string)
                {
                    _DeclareVariable.Identifier = (string)listTokens[currentToken];
                }
                else
                {
                    ExceptionHandler("variable name was expected after 'var'");
                }

                currentToken++;

                if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.EqualTo.ToString())
                {
                    ExceptionHandler("= sign was expected after 'var identifier'");
                }

                currentToken++;

                _DeclareVariable.Expression = ParseExpression();

                _DeclareFunction.Parameter2 = _DeclareVariable;
            }

            currentToken++;

            if (listTokens[currentToken].Equals("var"))
            {
                currentToken++;
                DeclareVariable _DeclareVariable = new DeclareVariable();

                if (currentToken < listTokens.Count && listTokens[currentToken] is string)
                {
                    _DeclareVariable.Identifier = (string)listTokens[currentToken];
                }
                else
                {
                    ExceptionHandler("variable name was expected after 'var'");
                }

                currentToken++;

                if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.EqualTo.ToString())
                {
                    ExceptionHandler("= sign was expected after 'var identifier'");
                }

                currentToken++;

                _DeclareVariable.Expression = ParseExpression();

                _DeclareFunction.Parameter3 = _DeclareVariable;
            }

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("begin"))
            {
                ExceptionHandler("expected 'begin' after input parameters");
            }

            currentToken++;
            _DeclareFunction.Body = ParseStatement();
            parsedStatement = _DeclareFunction;
            
            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("endfunc"))
            {
                ExceptionHandler("unterminated function', 'endfunc' expected at the end");
            }

            currentToken++;
        }

        else if (listTokens[currentToken].Equals("read"))
        {
            currentToken++;
            ReadInput _ReadInput = new ReadInput();

            if (currentToken < listTokens.Count && listTokens[currentToken] is string)
            {
                _ReadInput.Identifier = (string)listTokens[currentToken++];
                parsedStatement = _ReadInput;
            }
            else
            {
                ExceptionHandler("variable name is expected after 'read'");
                parsedStatement = null;
            }
        }


        //    IfThenElse ifThenElse = new IfThenElse();

        //    RelationalExpression relExpr = new RelationalExpression();
        //    relExpr.Left = ParseExpression();
        //    if (listTokens[index] == Scanner.EqualTo)
        //        relExpr.Operand = RelationalOperands.EqualTo;
        //    else if (listTokens[index] == Scanner.LessThan)
        //        relExpr.Operand = RelationalOperands.LessThan;
        //    else if (listTokens[index] == Scanner.GreaterThan)
        //        relExpr.Operand = RelationalOperands.GreaterThan;
        //    else
        //    {
        //        ExceptionHandler("expected relational operand");
        //    }

        //    index++;
        //    relExpr.Right = ParseExpression();
        //    ifThenElse.If = relExpr;
        //    index++;
        //    ifThenElse.Then = ParseStatement();

        //    parsedStatement = ifThenElse;

        //    //index++;



        else if (listTokens[this.currentToken].Equals("while"))
        {
            currentToken++;
            While _while = new While();

            _while.LeftExpression = ParseExpression();

            if (listTokens[currentToken].ToString() == Lexer.Tokens.EqualTo.ToString())
                _while.Operand = RelationalOperands.EqualTo;
            else if (listTokens[currentToken].ToString() == Lexer.Tokens.LessThan.ToString())
                _while.Operand = RelationalOperands.LessThan;
            else if (listTokens[currentToken].ToString() == Lexer.Tokens.GreaterThan.ToString())
                _while.Operand = RelationalOperands.GreaterThan;
            currentToken++;

            _while.RightExpression = ParseExpression();

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("do"))
            {
                ExceptionHandler("expected 'do' after while");
            }
            currentToken++;
            _while.Body = ParseStatement();
            parsedStatement = _while;

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("endwhile"))
            {
                ExceptionHandler("unterminated 'while', endwhile expected at the end");
            }

            currentToken++;
        }

        else if (listTokens[this.currentToken].Equals("if"))
        {
            currentToken++;
            IfThen _IfThen = new IfThen();

            _IfThen.LeftExpression = ParseExpression();

            if (listTokens[currentToken].ToString() == Lexer.Tokens.EqualTo.ToString())
                _IfThen.Operand = RelationalOperands.EqualTo;
            else if (listTokens[currentToken].ToString() == Lexer.Tokens.LessThan.ToString())
                _IfThen.Operand = RelationalOperands.LessThan;
            else if (listTokens[currentToken].ToString() == Lexer.Tokens.GreaterThan.ToString())
                _IfThen.Operand = RelationalOperands.GreaterThan;
            currentToken++;

            _IfThen.RightExpression = ParseExpression();

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("then"))
            {
                ExceptionHandler("expected 'then' after if");
            }
            currentToken++;
            _IfThen.ThenBody = ParseStatement();

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("else"))
            {
                ExceptionHandler("'else' is expected");
            }
            currentToken++;
            _IfThen.ElseBody = ParseStatement();

            parsedStatement = _IfThen;

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("endif"))
            {
                ExceptionHandler("unterminated 'if', endif expected at the end");
            }

            currentToken++;
        }

        else if (listTokens[currentToken].Equals("for"))
        {
            currentToken++;
            For _For = new For();

            if (currentToken < listTokens.Count && listTokens[currentToken] is string)
            {
                _For.Identifier = (string)listTokens[currentToken];
            }
            else
            {
                ExceptionHandler("expected identifier after 'for'");
            }

            currentToken++;

            if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.EqualTo.ToString())
            {
                ExceptionHandler("for missing '='");
            }

            currentToken++;

            _For.From = ParseExpression();

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("to"))
            {
                ExceptionHandler("expected 'to' after for");
            }

            currentToken++;

            _For.To = ParseExpression();

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("do"))
            {
                ExceptionHandler("expected 'do' after from expression in for loop");
            }

            currentToken++;

            _For.Body = ParseStatement();
            parsedStatement = _For;

            if (currentToken == listTokens.Count || !listTokens[currentToken].Equals("end"))
            {
                ExceptionHandler("unterminated 'for' loop body");
            }

            currentToken++;
        }

        else if (listTokens[currentToken] is string)
        {
            // assignment

            Assignment _Assignment = new Assignment();
            _Assignment.Identifier = (string)listTokens[currentToken++];

            if (currentToken == listTokens.Count || listTokens[currentToken].ToString() != Lexer.Tokens.EqualTo.ToString())
            {
                ExceptionHandler("expected '='");
            }

            currentToken++;

            _Assignment.Expression = ParseExpression();
            parsedStatement = _Assignment;
        }
        else
        {
            ExceptionHandler("parse error at token " + currentToken + ": " + listTokens[currentToken]);
            parsedStatement = null;
        }

        if (currentToken < listTokens.Count && listTokens[currentToken].ToString() == Lexer.Tokens.Terminator.ToString())
        {
            currentToken++;

            if (currentToken < listTokens.Count && !listTokens[currentToken].Equals("end") 
                && !listTokens[currentToken].Equals("else") && !listTokens[currentToken].Equals("endif")
                && !listTokens[currentToken].Equals("endwhile") && !listTokens[currentToken].Equals("endfunc"))
            {
                StatementSequence sequence = new StatementSequence();
                sequence.Left = parsedStatement;
                sequence.Right = ParseStatement();
                parsedStatement = sequence;
            }
        }

        return parsedStatement;
    }

    private Expression ParseExpression()
    {
        Expression parsedExpression;

        if (currentToken == listTokens.Count)
        {
            ExceptionHandler("expected expression, got EOF");
        }

        if (listTokens[currentToken] is StringBuilder)
        {
            string value = ((StringBuilder)listTokens[currentToken++]).ToString();
            AlphaNumericValue _AlphaNumericValue = new AlphaNumericValue();
            _AlphaNumericValue.Value = value;
            parsedExpression = _AlphaNumericValue;
        }
        else if (listTokens[currentToken] is int)
        {
            int intValue = (int)listTokens[currentToken++];
            NumericValue _NumericValue = new NumericValue();
            _NumericValue.Value = intValue;
            parsedExpression = _NumericValue;
        }
        else if (listTokens[currentToken] is string)
        {
            string identifier = (string)listTokens[currentToken++];
            Identifier _Identifier = new Identifier();
            _Identifier.IdentifierName = identifier;
            parsedExpression = _Identifier;
        }
        else
        {
            ExceptionHandler("expected string literal, int literal, or variable");
            return null;
        }

        //if (index < listTokens.Count && listTokens[index] != Scanner.Terminator && listTokens[index].GetType() != typeof(StringBuilder))
        if (listTokens[currentToken].ToString() != Lexer.Tokens.Terminator.ToString())
        {
            if (listTokens[currentToken].ToString() == Lexer.Tokens.Add.ToString() || listTokens[currentToken].ToString() == Lexer.Tokens.Subtract.ToString()
                || listTokens[currentToken].ToString() == Lexer.Tokens.Multiply.ToString() || listTokens[currentToken].ToString() == Lexer.Tokens.Divide.ToString())
            {
                ArithmaticExpression arithmaticExpression = new ArithmaticExpression();
                arithmaticExpression.Left = parsedExpression;
                if (listTokens[currentToken].ToString() == Lexer.Tokens.Add.ToString())
                    arithmaticExpression.Operand = ArithmaticOperands.Add;
                else if (listTokens[currentToken].ToString() == Lexer.Tokens.Subtract.ToString())
                    arithmaticExpression.Operand = ArithmaticOperands.Subtract;
                else if (listTokens[currentToken].ToString() == Lexer.Tokens.Multiply.ToString())
                    arithmaticExpression.Operand = ArithmaticOperands.Multiply;
                else if (listTokens[currentToken].ToString() == Lexer.Tokens.Divide.ToString())
                    arithmaticExpression.Operand = ArithmaticOperands.Division;
                currentToken++;

                arithmaticExpression.Right = ParseExpression();
                parsedExpression = arithmaticExpression;
            }
        }
        return parsedExpression;
    }
}
