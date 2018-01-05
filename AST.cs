using System;
using System.Collections.Generic;
using System.Text;


public abstract class Statement
{
}

public class DeclareVariable : Statement
{
    public string Identifier;
    public Expression Expression;
}

public class DeclareFunction : Statement 
{
    public string FunctionName;
    public DeclareVariable Parameter1;
    public DeclareVariable Parameter2;
    public DeclareVariable Parameter3;
    public String ReturnType;
    public Statement Body;
}

public class CallFunction : Statement 
{
    public string FunctionName;
    public Expression Parameter1;
    public Expression Parameter2;
    public Expression Parameter3;
    public Identifier ReturnVariable;
}

public class Write : Statement
{
    public Expression Expression;
}

public class Assignment : Statement
{
    public string Identifier;
    public Expression Expression;
}

public class For : Statement
{
    public string Identifier;
    public Expression From;
    public Expression To;
    public Statement Body;
}

public class IfThenElse : Statement
{
    public Expression If;
    public Statement Then;
    public Statement Else;
}

public class IfThen : Statement
{
    public Expression LeftExpression;
    public RelationalOperands Operand;
    public Expression RightExpression;
    public Statement ThenBody;
    public Statement ElseBody;
}

public class While : Statement
{
    public Expression LeftExpression;
    public RelationalOperands Operand;
    public Expression RightExpression;
    public Statement Body;
}

public class ReadInput : Statement
{
    public string Identifier;
}

public class StatementSequence : Statement
{
    public Statement Left;
    public Statement Right;
}

public abstract class Expression
{
}

public class AlphaNumericValue : Expression
{
    public string Value;
}

public class NumericValue : Expression
{
    public int Value;
}

public class Identifier : Expression
{
    public string IdentifierName;
}

public class ArithmaticExpression : Expression
{
    public Expression Left;
    public Expression Right;
    public ArithmaticOperands Operand;
}

public enum ArithmaticOperands
{
    Add,
    Subtract,
    Multiply,
    Division
}

public class LogicalExpression : Expression
{
    public Expression Left;
    public Expression Right;
    public LogicalOperands Operand;
}

public enum LogicalOperands
{
    Or,
    And,
    Xor,
    Not
}

public class RelationalExpression : Expression
{
    public Expression Left;
    public Expression Right;
    public RelationalOperands Operand;
}

public class CompareVariable : Statement
{
    public string Identifier;
    public Expression Expression;
    public RelationalOperands Operand;
}

public enum RelationalOperands
{
    EqualTo,
    NotEqualTo,
    LessThan,
    LessThanOrEqualTo,
    GreaterThan,
    GreaterThanOrEqualTo
}
