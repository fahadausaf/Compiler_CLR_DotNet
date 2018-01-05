using System;
using System.Collections.Generic;
using System.Text;
// Added
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

public sealed class CodeGenerator
{
    
    Dictionary<string, System.Reflection.Emit.LocalBuilder> tblIdentifier;
    Dictionary<string, System.Reflection.Emit.LocalBuilder> tblArguments;
    TypeBuilder _typeBuilder = null;
    System.Reflection.Emit.ILGenerator _ILGenerator = null;
    MemberInfo WriteLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

    #region Statement
    public CodeGenerator(Statement statement, string fileName)
    {
        string strFileName = Path.GetFileNameWithoutExtension(fileName);

        #region Define Assembly
        //AssemblyName _AssemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(fileName));
        AssemblyName _AssemblyName = new AssemblyName(strFileName);
        AppDomain currentDomain = AppDomain.CurrentDomain;
        AssemblyBuilder _AssemblyBuilder = currentDomain.DefineDynamicAssembly(_AssemblyName, AssemblyBuilderAccess.Save);
        #endregion

        #region Define Module
        ModuleBuilder _ModuleBuilder = _AssemblyBuilder.DefineDynamicModule(fileName);
        #endregion
        
        _typeBuilder = _ModuleBuilder.DefineType("CodeGenerator");
        MethodBuilder _MethodBuilder = _typeBuilder.DefineMethod
            ("Main", 
            MethodAttributes.Static, 
            typeof(void), 
            System.Type.EmptyTypes);

        _ILGenerator = _MethodBuilder.GetILGenerator();
        tblIdentifier = new Dictionary<string, System.Reflection.Emit.LocalBuilder>();
        tblArguments = new Dictionary<string, System.Reflection.Emit.LocalBuilder>();

        // CIL generation
        GenerateStatement(statement);

        _ILGenerator.Emit(OpCodes.Ret);
        _typeBuilder.CreateType();
        _ModuleBuilder.CreateGlobalFunctions();
        _AssemblyBuilder.SetEntryPoint(_MethodBuilder);
        _AssemblyBuilder.Save(fileName);
        tblIdentifier = null;
        _ILGenerator = null; 
    }

    public void GenerateStatement(Statement _statement)
    {
        if (_statement is StatementSequence)
        {
            StatementSequence _StatementSequence = (StatementSequence)_statement;
            GenerateStatement(_StatementSequence.Left);
            GenerateStatement(_StatementSequence.Right);
        }

        else if (_statement is DeclareVariable)
        {
            // declare a variable in symbol table
            DeclareVariable declare = (DeclareVariable)_statement;
            tblIdentifier[declare.Identifier] = 
                _ILGenerator.DeclareLocal(GetExpressionType(declare.Expression));

            // set the initial value
            Assignment assign = new Assignment();
            assign.Identifier = declare.Identifier;
            assign.Expression = declare.Expression;
            GenerateStatement(assign);
        }

        else if (_statement is Assignment)
        {
            Assignment assign = (Assignment)_statement;
            GenerateExpression(assign.Expression, 
                GetExpressionType(assign.Expression));
            if (GetExpressionType(assign.Expression) == typeof(ArithmaticExpression))
            {
                SaveIdentifier(assign.Identifier, typeof(Int32));
            }
            else 
            {
                SaveIdentifier(assign.Identifier, 
                    GetExpressionType(assign.Expression));
            }
            
        }

        else if (_statement is DeclareFunction) 
        {
            GenerateStatementMethod(_statement);
        }

        else if (_statement is Write)
        {
            // for print keyword, call .net method for printscreen
            GenerateExpression(((Write)_statement).Expression,
                typeof(string));
            _ILGenerator.Emit(OpCodes.Call,
                typeof(System.Console).GetMethod("WriteLine",
                new System.Type[] { typeof(string) }));
        }

        else if (_statement is ReadInput)
        {
            // call the readline method and parse input method
            _ILGenerator.Emit(OpCodes.Call,
                typeof(System.Console).GetMethod("ReadLine",
                BindingFlags.Public | BindingFlags.Static,
                null, new System.Type[] { }, null));
            _ILGenerator.Emit(OpCodes.Call,
                typeof(int).GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                null, new System.Type[] { typeof(string) }, null));
            // store the input value in local builder
            SaveIdentifier(((ReadInput)_statement).Identifier, typeof(int));
        }
        else if (_statement is IfThenElse)
        {

            //IfThenElse ifThenElse = (IfThenElse)stmt;
            //RelationalExpression relExpr = (RelationalExpression)ifThenElse.If;
            //// if, left side only
            //il.Emit(OpCodes.Stloc, symbolTable[relExpr.Left.ToString()]);
            //Label lblIf = il.DefineLabel();
            //il.Emit(OpCodes.Br, lblIf);
            //// then
            //Label lblThen = il.DefineLabel();
            //il.MarkLabel(lblThen);

        }

        else if (_statement is While)
        {
            While _while = (While)_statement;
            Label lblTest = _ILGenerator.DefineLabel();
            Label lblEnd = _ILGenerator.DefineLabel();

            if (_while.Operand == RelationalOperands.GreaterThan)
            {
                _ILGenerator.MarkLabel(lblTest);
                GenerateExpression(_while.LeftExpression, typeof(int));
                GenerateExpression(_while.RightExpression, typeof(int));
                _ILGenerator.Emit(OpCodes.Cgt);
                _ILGenerator.Emit(OpCodes.Brfalse, lblEnd);
                GenerateStatement(_while.Body);
                _ILGenerator.Emit(OpCodes.Br, lblTest);

                _ILGenerator.MarkLabel(lblEnd);
            }
            else if (_while.Operand == RelationalOperands.EqualTo)
            {
                _ILGenerator.MarkLabel(lblTest);
                GenerateExpression(_while.LeftExpression, typeof(int));
                GenerateExpression(_while.RightExpression, typeof(int));
                _ILGenerator.Emit(OpCodes.Ceq);
                _ILGenerator.Emit(OpCodes.Brfalse, lblEnd);
                GenerateStatement(_while.Body);
                _ILGenerator.Emit(OpCodes.Br, lblTest);

                _ILGenerator.MarkLabel(lblEnd);
            }
            else if (_while.Operand == RelationalOperands.LessThan)
            {
                _ILGenerator.MarkLabel(lblTest);
                GenerateExpression(_while.LeftExpression, typeof(int));
                GenerateExpression(_while.RightExpression, typeof(int));
                _ILGenerator.Emit(OpCodes.Clt);
                _ILGenerator.Emit(OpCodes.Brfalse, lblEnd);
                GenerateStatement(_while.Body);
                _ILGenerator.Emit(OpCodes.Br, lblTest);

                _ILGenerator.MarkLabel(lblEnd);
            }
        }
        else if (_statement is IfThen)
        {
            #region
            //////Label body = il.DefineLabel();

            //////il.Emit(OpCodes.Ldc_I4, 1000);



            /*
            // var x = 0;
            // if x < 5 then
            //    print "less than 5";
            // endif;

            IfThen ifThen = (IfThen)stmt;
            // jump to test
            

            // **test** if x LessThan 5? (do the test)
            il.MarkLabel(test);
            GenExpr(ifThen.LeftExpression, typeof(int));
            
            */

            //Label greaterThan = il.DefineLabel();

            //IfThen ifThen = (IfThen)stmt;
            //GenExpr(ifThen.LeftExpression, typeof(int));
            //GenExpr(ifThen.RightExpression, typeof(int));
            //if (ifThen.Operand == RelationalOperands.GreaterThan) 
            //{
            //    
            //}
            #endregion

            IfThen ifThen = (IfThen)_statement;
            Label lblElse = _ILGenerator.DefineLabel();
            Label lblEnd = _ILGenerator.DefineLabel();

            #region GreaterThan
            if (ifThen.Operand == RelationalOperands.GreaterThan)
            {
                GenerateExpression(ifThen.LeftExpression, typeof(int));
                GenerateExpression(ifThen.RightExpression, typeof(int));
                _ILGenerator.Emit(OpCodes.Cgt);
                _ILGenerator.Emit(OpCodes.Brfalse, lblElse);
                GenerateStatement(ifThen.ThenBody);
                _ILGenerator.Emit(OpCodes.Br, lblEnd);

                _ILGenerator.MarkLabel(lblElse);
                GenerateStatement(ifThen.ElseBody);

                _ILGenerator.MarkLabel(lblEnd);
            }
            #endregion
            #region EqualTo
            else if (ifThen.Operand == RelationalOperands.EqualTo)
            {
                GenerateExpression(ifThen.LeftExpression, typeof(int));
                GenerateExpression(ifThen.RightExpression, typeof(int));
                _ILGenerator.Emit(OpCodes.Ceq);
                _ILGenerator.Emit(OpCodes.Brfalse, lblElse);
                GenerateStatement(ifThen.ThenBody);
                _ILGenerator.Emit(OpCodes.Br, lblEnd);

                _ILGenerator.MarkLabel(lblElse);
                GenerateStatement(ifThen.ElseBody);

                _ILGenerator.MarkLabel(lblEnd);
            }
            #endregion
            #region LessThan
            else if (ifThen.Operand == RelationalOperands.LessThan)
            {
                GenerateExpression(ifThen.LeftExpression, typeof(int));
                GenerateExpression(ifThen.RightExpression, typeof(int));
                _ILGenerator.Emit(OpCodes.Clt);
                _ILGenerator.Emit(OpCodes.Brfalse, lblElse);
                GenerateStatement(ifThen.ThenBody);
                _ILGenerator.Emit(OpCodes.Br, lblEnd);

                _ILGenerator.MarkLabel(lblElse);
                GenerateStatement(ifThen.ElseBody);

                _ILGenerator.MarkLabel(lblEnd);
            }
            #endregion

            #region
            /*
            Label gtTrue = il.DefineLabel();
            Label gtFalse = il.DefineLabel();
            
            
             */
            #endregion

        }
        else if (_statement is For)
        {
            // example: 
            // for x = 0 to 100 do
            //   print "hello";
            // end;

            // x = 0
            For forLoop = (For)_statement;
            Assignment assign = new Assignment();
            assign.Identifier = forLoop.Identifier;
            assign.Expression = forLoop.From;
            GenerateStatement(assign);
            // jump to the test
            Label test = _ILGenerator.DefineLabel();
            _ILGenerator.Emit(OpCodes.Br, test);

            // body statement
            Label body = _ILGenerator.DefineLabel();
            _ILGenerator.MarkLabel(body);
            GenerateStatement(forLoop.Body);

            // increase x
            _ILGenerator.Emit(OpCodes.Ldloc, tblIdentifier[forLoop.Identifier]);
            _ILGenerator.Emit(OpCodes.Ldc_I4, 1);
            _ILGenerator.Emit(OpCodes.Add);
            SaveIdentifier(forLoop.Identifier, typeof(int));

            // check if x is equal to 100
            _ILGenerator.MarkLabel(test);
            _ILGenerator.Emit(OpCodes.Ldloc, tblIdentifier[forLoop.Identifier]);
            GenerateExpression(forLoop.To, typeof(int));
            _ILGenerator.Emit(OpCodes.Blt, body);
        }

        else
        {
            ExceptionHandler("unable to generate " + _statement.GetType().Name);
        }
    }

    public void ExceptionHandler(string strException) 
    {
        throw new Exception(strException);
    }

    public void SaveIdentifier(string IdentifierName, System.Type IdentifierType)
    {
        if (tblIdentifier.ContainsKey(IdentifierName))
        {
            LocalBuilder locb = tblIdentifier[IdentifierName];

            if (locb.LocalType == IdentifierType)
            {
                _ILGenerator.Emit(OpCodes.Stloc, tblIdentifier[IdentifierName]);
            }
            else
            {
                ExceptionHandler("'" + IdentifierName + "' is of type " + locb.LocalType.Name + " but saving in different type " + IdentifierType.Name);
            }
        }
        else
        {
            ExceptionHandler("variable not declared '" + IdentifierName + "'");
        }
    }

    public void GenerateExpression(Expression _expression, System.Type expressionType)
    {
        System.Type typeOfExpression;

        if (_expression is AlphaNumericValue)
        {
            typeOfExpression = typeof(string);
            _ILGenerator.Emit(OpCodes.Ldstr, ((AlphaNumericValue)_expression).Value);
        }
        else if (_expression is NumericValue)
        {
            typeOfExpression = typeof(int);
            _ILGenerator.Emit(OpCodes.Ldc_I4, ((NumericValue)_expression).Value);
        }
        
        else if (_expression is ArithmaticExpression) 
        {
            typeOfExpression = GetExpressionType(_expression);

            ArithmaticExpression arithmaticExpression = (ArithmaticExpression)_expression;
            GenerateExpression(arithmaticExpression.Left, GetExpressionType(arithmaticExpression.Left));
            GenerateExpression(arithmaticExpression.Right, GetExpressionType(arithmaticExpression.Right));
            if (arithmaticExpression.Operand == ArithmaticOperands.Add) 
            {
                _ILGenerator.Emit(OpCodes.Add);
            }
            else if (arithmaticExpression.Operand == ArithmaticOperands.Subtract) 
            {
                _ILGenerator.Emit(OpCodes.Sub);
            }
            else if (arithmaticExpression.Operand == ArithmaticOperands.Multiply) 
            {
                _ILGenerator.Emit(OpCodes.Mul);
            }
            else if (arithmaticExpression.Operand == ArithmaticOperands.Division) 
            {
                _ILGenerator.Emit(OpCodes.Div);
            }
        }
        
        
        else if (_expression is Identifier)
        {
            string identifier = ((Identifier)_expression).IdentifierName;
            typeOfExpression = GetExpressionType(_expression);

            if (!tblIdentifier.ContainsKey(identifier))
            {
                ExceptionHandler("undeclared variable '" + identifier + "'");
            }

            _ILGenerator.Emit(OpCodes.Ldloc, tblIdentifier[identifier]);
        }
        
        else
        {
            ExceptionHandler("can't generate " + _expression.GetType().Name);
            typeOfExpression = null;
        }

        if (typeOfExpression != expressionType)
        {
            if (typeOfExpression == typeof(int) &&
                expressionType == typeof(string))
            {
                _ILGenerator.Emit(OpCodes.Box, typeof(int));
                _ILGenerator.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            }
            else
            {
                ExceptionHandler("can't convert " + typeOfExpression.Name + " to " + expressionType.Name);
            }
        }

    }

    public System.Type GetExpressionType(Expression _expression)
    {
        if (_expression is AlphaNumericValue)
        {
            return typeof(string);
        }
        else if (_expression is NumericValue)
        {
            return typeof(int);
        }
        else if (_expression is Identifier)
        {
            Identifier var = (Identifier)_expression;
            if (tblIdentifier.ContainsKey(var.IdentifierName))
            {
                LocalBuilder locb = tblIdentifier[var.IdentifierName];
                return locb.LocalType;
            }
            else
            {
                ExceptionHandler("variable not declared '" + var.IdentifierName + "'");
                return null;
            }
        }
        else if (_expression is ArithmaticExpression) 
        {
            return typeof(ArithmaticExpression);
        }
        else
        {
            ExceptionHandler("type cannot be generated for " + _expression.GetType().Name);
            return null;
        }
    }

    #endregion

    #region Method

    System.Reflection.Emit.ILGenerator _ILMethod = null;
    public void GenerateStatementMethod(Statement _statement)
    {
        #region Statement
        if (_statement is StatementSequence)
        {
            StatementSequence _StatementSequence = (StatementSequence)_statement;
            GenerateStatementMethod(_StatementSequence.Left);
            GenerateStatementMethod(_StatementSequence.Right);
        }
        #endregion

        #region Declare Variable
        else if (_statement is DeclareVariable)
        {
            // declare a variable in symbol table
            DeclareVariable declare = (DeclareVariable)_statement;
            tblIdentifier[declare.Identifier] =
                _ILMethod.DeclareLocal(GetExpressionTypeMethod(declare.Expression));

            // set the initial value
            Assignment assign = new Assignment();
            assign.Identifier = declare.Identifier;
            assign.Expression = declare.Expression;
            GenerateStatementMethod(assign);
        }
        #endregion

        #region Assignment
        else if (_statement is Assignment)
        {
            Assignment assign = (Assignment)_statement;
            GenerateExpressionMethod(assign.Expression, GetExpressionTypeMethod(assign.Expression));
            if (GetExpressionTypeMethod(assign.Expression) == typeof(ArithmaticExpression))
            {
                SaveIdentifierMethod(assign.Identifier, typeof(Int32));
            }
            else
            {
                SaveIdentifierMethod(assign.Identifier,
                    GetExpressionTypeMethod(assign.Expression));
            }

        }
        #endregion

        else if (_statement is DeclareFunction)
        {
            DeclareFunction _DeclareFunction = (DeclareFunction)_statement;

            string strFunctionName = _DeclareFunction.FunctionName;

            Type ParameterType1 = GetExpressionTypeMethod(_DeclareFunction.Parameter1.Expression);
            Type ParameterType2 = GetExpressionTypeMethod(_DeclareFunction.Parameter2.Expression);
            Type ParameterType3 = GetExpressionTypeMethod(_DeclareFunction.Parameter3.Expression);
            Type[] InputParameters = { ParameterType1, ParameterType2, ParameterType3 };
            
            Type ReturnType = typeof(void);
            if(_DeclareFunction.ReturnType == "void")
                ReturnType = typeof(void);
            else if (_DeclareFunction.ReturnType == "string")
                ReturnType = typeof(string);
            else if (_DeclareFunction.ReturnType == "numeric")
                ReturnType = typeof(int);

            

            //FieldBuilder Parameter1 = _typeBuilder.DefineField(_DeclareFunction.Parameter1.Identifier, ParameterType1, FieldAttributes.Private);
            //FieldBuilder Parameter2 = _typeBuilder.DefineField(_DeclareFunction.Parameter2.Identifier, ParameterType2, FieldAttributes.Private);
            //FieldBuilder Parameter3 = _typeBuilder.DefineField(_DeclareFunction.Parameter3.Identifier, ParameterType3, FieldAttributes.Private);            
            
            MethodBuilder NewMethod = 
                _typeBuilder.DefineMethod
                (strFunctionName,
                MethodAttributes.Static,
                ReturnType,
                InputParameters);

            //ParameterBuilder poolRefBuilder = NewMethod.DefineParameter(1, ParameterAttributes.In, _DeclareFunction.Parameter1.Identifier);

            _ILMethod = NewMethod.GetILGenerator();


            //tblArguments[_DeclareFunction.Parameter0.Identifier] = _ILMethod.

            tblArguments[_DeclareFunction.Parameter1.Identifier] = _ILMethod.DeclareLocal(ParameterType1);
            GenerateExpressionMethod(_DeclareFunction.Parameter1.Expression, ParameterType1);
            //_ILMethod.Emit(OpCodes.Starg, 0);

            tblArguments[_DeclareFunction.Parameter1.Identifier] = _ILMethod.DeclareLocal(ParameterType2);
            GenerateExpressionMethod(_DeclareFunction.Parameter1.Expression, ParameterType2);
            //_ILMethod.Emit(OpCodes.Starg, 1);

            tblArguments[_DeclareFunction.Parameter2.Identifier] = _ILMethod.DeclareLocal(ParameterType3);
            GenerateExpressionMethod(_DeclareFunction.Parameter2.Expression, ParameterType3);
            //_ILMethod.Emit(OpCodes.Starg, 2);

            //GenerateStatementMethod(_DeclareFunction.Body);
            //_ILMethod.Emit(OpCodes.Ret);
        }

        #region write-read
        else if (_statement is Write)
        {
            // for print keyword, call .net method for printscreen
            GenerateExpressionMethod(((Write)_statement).Expression,
                typeof(string));
            _ILMethod.Emit(OpCodes.Call,
                typeof(System.Console).GetMethod("WriteLine",
                new System.Type[] { typeof(string) }));
        }

        else if (_statement is ReadInput)
        {
            // call the readline method and parse input method
            _ILMethod.Emit(OpCodes.Call,
                typeof(System.Console).GetMethod("ReadLine",
                BindingFlags.Public | BindingFlags.Static,
                null, new System.Type[] { }, null));
            _ILMethod.Emit(OpCodes.Call,
                typeof(int).GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                null, new System.Type[] { typeof(string) }, null));
            // store the input value in local builder
            SaveIdentifierMethod(((ReadInput)_statement).Identifier, typeof(int));
        }
        #endregion

        #region While
        else if (_statement is While)
        {
            While _while = (While)_statement;
            Label lblTest = _ILMethod.DefineLabel();
            Label lblEnd = _ILMethod.DefineLabel();

            if (_while.Operand == RelationalOperands.GreaterThan)
            {
                _ILMethod.MarkLabel(lblTest);
                GenerateExpressionMethod(_while.LeftExpression, typeof(int));
                GenerateExpressionMethod(_while.RightExpression, typeof(int));
                _ILMethod.Emit(OpCodes.Cgt);
                _ILMethod.Emit(OpCodes.Brfalse, lblEnd);
                GenerateStatementMethod(_while.Body);
                _ILMethod.Emit(OpCodes.Br, lblTest);

                _ILMethod.MarkLabel(lblEnd);
            }
            else if (_while.Operand == RelationalOperands.EqualTo)
            {
                _ILMethod.MarkLabel(lblTest);
                GenerateExpressionMethod(_while.LeftExpression, typeof(int));
                GenerateExpressionMethod(_while.RightExpression, typeof(int));
                _ILMethod.Emit(OpCodes.Ceq);
                _ILMethod.Emit(OpCodes.Brfalse, lblEnd);
                GenerateStatementMethod(_while.Body);
                _ILMethod.Emit(OpCodes.Br, lblTest);

                _ILMethod.MarkLabel(lblEnd);
            }
            else if (_while.Operand == RelationalOperands.LessThan)
            {
                _ILMethod.MarkLabel(lblTest);
                GenerateExpressionMethod(_while.LeftExpression, typeof(int));
                GenerateExpressionMethod(_while.RightExpression, typeof(int));
                _ILMethod.Emit(OpCodes.Clt);
                _ILMethod.Emit(OpCodes.Brfalse, lblEnd);
                GenerateStatementMethod(_while.Body);
                _ILMethod.Emit(OpCodes.Br, lblTest);

                _ILMethod.MarkLabel(lblEnd);
            }
        }
        #endregion

        #region If-Then
        else if (_statement is IfThen)
        {
            IfThen ifThen = (IfThen)_statement;
            Label lblElse = _ILMethod.DefineLabel();
            Label lblEnd = _ILMethod.DefineLabel();

            #region GreaterThan
            if (ifThen.Operand == RelationalOperands.GreaterThan)
            {
                GenerateExpressionMethod(ifThen.LeftExpression, typeof(int));
                GenerateExpressionMethod(ifThen.RightExpression, typeof(int));
                _ILMethod.Emit(OpCodes.Cgt);
                _ILMethod.Emit(OpCodes.Brfalse, lblElse);
                GenerateStatementMethod(ifThen.ThenBody);
                _ILMethod.Emit(OpCodes.Br, lblEnd);

                _ILMethod.MarkLabel(lblElse);
                GenerateStatementMethod(ifThen.ElseBody);

                _ILMethod.MarkLabel(lblEnd);
            }
            #endregion
            #region EqualTo
            else if (ifThen.Operand == RelationalOperands.EqualTo)
            {
                GenerateExpressionMethod(ifThen.LeftExpression, typeof(int));
                GenerateExpressionMethod(ifThen.RightExpression, typeof(int));
                _ILMethod.Emit(OpCodes.Ceq);
                _ILMethod.Emit(OpCodes.Brfalse, lblElse);
                GenerateStatementMethod(ifThen.ThenBody);
                _ILMethod.Emit(OpCodes.Br, lblEnd);

                _ILMethod.MarkLabel(lblElse);
                GenerateStatementMethod(ifThen.ElseBody);

                _ILMethod.MarkLabel(lblEnd);
            }
            #endregion
            #region LessThan
            else if (ifThen.Operand == RelationalOperands.LessThan)
            {
                GenerateExpressionMethod(ifThen.LeftExpression, typeof(int));
                GenerateExpressionMethod(ifThen.RightExpression, typeof(int));
                _ILMethod.Emit(OpCodes.Clt);
                _ILMethod.Emit(OpCodes.Brfalse, lblElse);
                GenerateStatementMethod(ifThen.ThenBody);
                _ILMethod.Emit(OpCodes.Br, lblEnd);

                _ILMethod.MarkLabel(lblElse);
                GenerateStatementMethod(ifThen.ElseBody);

                _ILMethod.MarkLabel(lblEnd);
            }
            #endregion
        }
        #endregion

        #region for
        else if (_statement is For)
        {
            For forLoop = (For)_statement;
            Assignment assign = new Assignment();
            assign.Identifier = forLoop.Identifier;
            assign.Expression = forLoop.From;
            GenerateStatementMethod(assign);
            
            Label test = _ILMethod.DefineLabel();
            _ILMethod.Emit(OpCodes.Br, test);

            Label body = _ILMethod.DefineLabel();
            _ILMethod.MarkLabel(body);
            GenerateStatementMethod(forLoop.Body);

            _ILMethod.Emit(OpCodes.Ldloc, tblIdentifier[forLoop.Identifier]);
            _ILMethod.Emit(OpCodes.Ldc_I4, 1);
            _ILMethod.Emit(OpCodes.Add);
            SaveIdentifierMethod(forLoop.Identifier, typeof(int));

            _ILMethod.MarkLabel(test);
            _ILMethod.Emit(OpCodes.Ldloc, tblIdentifier[forLoop.Identifier]);
            GenerateExpressionMethod(forLoop.To, typeof(int));
            _ILMethod.Emit(OpCodes.Blt, body);
        }
        #endregion

        else
        {
            ExceptionHandler("unable to generate " + _statement.GetType().Name);
        }
    }

    public void SaveIdentifierMethod(string IdentifierName, System.Type IdentifierType)
    {
        if (tblIdentifier.ContainsKey(IdentifierName))
        {
            LocalBuilder locb = tblIdentifier[IdentifierName];

            if (locb.LocalType == IdentifierType)
            {
                _ILMethod.Emit(OpCodes.Stloc, tblIdentifier[IdentifierName]);
            }
            else
            {
                ExceptionHandler("'" + IdentifierName + "' is of type " + locb.LocalType.Name + " but saving in different type " + IdentifierType.Name);
            }
        }
        else
        {
            ExceptionHandler("variable not declared '" + IdentifierName + "'");
        }
    }

    public void DeclareArgument() { }

    public void GenerateExpressionMethod(Expression _expression, System.Type expressionType)
    {
        System.Type typeOfExpression;

        if (_expression is AlphaNumericValue)
        {
            typeOfExpression = typeof(string);
            _ILMethod.Emit(OpCodes.Ldstr, ((AlphaNumericValue)_expression).Value);
        }
        else if (_expression is NumericValue)
        {
            typeOfExpression = typeof(int);
            _ILMethod.Emit(OpCodes.Ldc_I4, ((NumericValue)_expression).Value);
        }

        else if (_expression is ArithmaticExpression)
        {
            typeOfExpression = GetExpressionTypeMethod(_expression);

            ArithmaticExpression arithmaticExpression = (ArithmaticExpression)_expression;
            GenerateExpressionMethod(arithmaticExpression.Left, GetExpressionTypeMethod(arithmaticExpression.Left));
            GenerateExpressionMethod(arithmaticExpression.Right, GetExpressionTypeMethod(arithmaticExpression.Right));
            if (arithmaticExpression.Operand == ArithmaticOperands.Add)
            {
                _ILMethod.Emit(OpCodes.Add);
            }
            else if (arithmaticExpression.Operand == ArithmaticOperands.Subtract)
            {
                _ILMethod.Emit(OpCodes.Sub);
            }
            else if (arithmaticExpression.Operand == ArithmaticOperands.Multiply)
            {
                _ILMethod.Emit(OpCodes.Mul);
            }
            else if (arithmaticExpression.Operand == ArithmaticOperands.Division)
            {
                _ILMethod.Emit(OpCodes.Div);
            }
        }


        else if (_expression is Identifier)
        {
            string identifier = ((Identifier)_expression).IdentifierName;
            typeOfExpression = GetExpressionTypeMethod(_expression);

            if (!tblIdentifier.ContainsKey(identifier) && !tblArguments.ContainsKey(identifier))
            {
                ExceptionHandler("undeclared variable '" + identifier + "'");
            }

            if (tblIdentifier.ContainsKey(identifier))
                _ILMethod.Emit(OpCodes.Ldloc, tblIdentifier[identifier]);
            else
                _ILMethod.Emit(OpCodes.Ldloc, tblArguments[identifier]);
        }

        else
        {
            ExceptionHandler("can't generate " + _expression.GetType().Name);
            typeOfExpression = null;
        }

        if (typeOfExpression != expressionType)
        {
            if (typeOfExpression == typeof(int) &&
                expressionType == typeof(string))
            {
                _ILMethod.Emit(OpCodes.Box, typeof(int));
                _ILMethod.Emit(OpCodes.Callvirt, typeof(object).GetMethod("ToString"));
            }
            else
            {
                ExceptionHandler("can't convert " + typeOfExpression.Name + " to " + expressionType.Name);
            }
        }

    }

    public System.Type GetExpressionTypeMethod(Expression _expression)
    {
        if (_expression is AlphaNumericValue)
        {
            return typeof(string);
        }
        else if (_expression is NumericValue)
        {
            return typeof(int);
        }
        else if (_expression is Identifier)
        {
            Identifier var = (Identifier)_expression;
            if (tblIdentifier.ContainsKey(var.IdentifierName))
            {
                LocalBuilder locb = tblIdentifier[var.IdentifierName];
                return locb.LocalType;
            }
            else if (tblArguments.ContainsKey(var.IdentifierName))
            {
                LocalBuilder locb = tblArguments[var.IdentifierName];
                return locb.LocalType;
            }
            else
            {
                ExceptionHandler("variable not declared '" + var.IdentifierName + "'");
                return null;
            }
        }
        else if (_expression is ArithmaticExpression)
        {
            return typeof(ArithmaticExpression);
        }
        else
        {
            ExceptionHandler("type cannot be generated for " + _expression.GetType().Name);
            return null;
        }
    }    
    #endregion
}

