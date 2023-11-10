using Types;

public abstract class Result { }

public sealed class Return : Result
{
    public ValueObject Value { get; }

    public Return(ValueObject value)
    {
        Value = value;
    }
}

public sealed class Break : Result { }

public sealed class Variable
{
    public ValueObject Value { get; private set; }
    public TypeSymbol Type { get; }
    public bool IsConstant { get; }

    public Variable(ValueObject value, TypeSymbol type, bool isConstant = false)
    {
        Type = type;
        Value = value;
        IsConstant = isConstant;
    }

    public void Set(ValueObject value)
    {
        Value = value;
    }
}

public sealed class Interpreter
{
    private StatementSyntax _root { get; }
    private Environment _scope = new(null);

    public Interpreter(StatementSyntax root)
    {
        _root = root;

        FunctionTypeSymbol inputType = new(TypeSymbol.String, new() { TypeSymbol.String });
        FunctionTypeSymbol errorType = new(TypeSymbol.Void, new() { TypeSymbol.String });
        FunctionTypeSymbol printType = new(TypeSymbol.Void, new() { TypeSymbol.Object });
        FunctionTypeSymbol lengthType = new(TypeSymbol.Float, new() { TypeSymbol.Array(TypeSymbol.Object) });
        FunctionTypeSymbol getType = new(TypeSymbol.Object, new() { TypeSymbol.Array(TypeSymbol.Object), TypeSymbol.Float });

        ValueObject inputFunction = new((List<ValueObject> arguments) =>
        {
            string message = (string)arguments[0].Value;
            Console.Write(message);
            string input = Console.ReadLine() ?? "";

            return new Return(ValueObject.String(input));
        }, inputType);

        ValueObject errorFunction = new((Func<List<ValueObject>, Result?>)((List<ValueObject> arguments) =>
        {
            throw new Exception((string)arguments[0].Value);
        }), errorType);

        ValueObject printFunction = new((List<ValueObject> arguments) =>
        {
            Console.WriteLine(arguments[0].Value);
            return new Return(ValueObject.Void);
        }, printType);

        ValueObject lengthFunction = new((List<ValueObject> arguments) =>
        {
            int length = ((List<ValueObject>)arguments[0].Value).Count;

            return new Return(ValueObject.Float(length));
        }, lengthType);

        ValueObject getFunction = new((List<ValueObject> arguments) =>
        {
            List<ValueObject> list = (List<ValueObject>)arguments[0].Value;
            int index = Convert.ToInt32(arguments[1].Value);

            return new Return(list[index]);
        }, getType);

        ValueObject clearFunction = new((List<ValueObject> arguments) => {
            Console.Clear();

            return new Return(ValueObject.Void);
        }, TypeSymbol.Function(TypeSymbol.Void, new() { }));

        _scope.Declare("input", inputType, inputFunction, true);
        _scope.Declare("error", errorType, errorFunction, true);
        _scope.Declare("print", printType, printFunction, true);
        _scope.Declare("length", lengthType, lengthFunction, true);
        _scope.Declare("get", getType, getFunction, true);
        _scope.Declare("clear", clearFunction.Type, clearFunction, true);
    }

    private void EnterScope()
    {
        _scope = new Environment(_scope);
    }

    private void LeaveScope()
    {
        if (_scope.Parent is null)
            throw new Exception("Cannot leave a scope with no parent");

        _scope = _scope.Parent;
    }

    public void Interpret()
    {
        Result? result = InterpretStatement(_root);

        if (result is Return)
            throw new Exception("Cannot return from outside a function.");
        else if (result is Break)
            throw new Exception("Cannot break from outside a loop.");
    }

    private Result? InterpretStatement(StatementSyntax statement)
    {
        if (statement is VariableDeclarationStatement declareStatement)
            InterpretVariableDeclarationStatement(declareStatement);
        else if (statement is CallStatement callStatement)
            InterpretCallStatement(callStatement);
        else if (statement is BlockStatement blockStatement)
            return InterpretBlockStatement(blockStatement);
        else if (statement is IfStatement ifStatement)
            return InterpretIfStatement(ifStatement);
        else if (statement is ForStatement forStatement)
            return InterpretForStatement(forStatement);
        else if (statement is AssignmentStatement assignmentStatement)
            InterpretAssignmentStatement(assignmentStatement);
        else if (statement is WhileStatement whileStatement)
            return InterpretWhileStatement(whileStatement);
        else if (statement is ReturnStatement returnStatement)
            return InterpretReturnStatement(returnStatement);
        else if (statement is BreakStatement breakStatement)
            return InterpretBreakStatement(breakStatement);
        else if (statement is FunctionDeclaration functionDeclaration)
            InterpretFunctionDeclaration(functionDeclaration);

        return null;
    }

    private Result? InterpretReturnStatement(ReturnStatement returnStatement)
    {
        if (returnStatement.Expression is not null)
        {
            ValueObject value = EvaluateExpression(returnStatement.Expression);

            return new Return(value);
        }

        return new Return(ValueObject.Void);
    }

    private Result? InterpretBreakStatement(BreakStatement breakStatement)
    {
        return new Break();
    }

    private Result? InterpretBlockStatement(BlockStatement blockStatement)
    {
        EnterScope();

        foreach (StatementSyntax statement in blockStatement.Statements)
        {
            Result? result = InterpretStatement(statement);

            if (result is not null)
            {
                LeaveScope();
                return result;
            }
        }

        LeaveScope();

        return null;
    }

    private Result? InterpretWhileStatement(WhileStatement whileStatement)
    {
        while (true)
        {
            ValueObject condition = EvaluateExpression(whileStatement.Condition);

            if (!condition.Type.Matches(TypeSymbol.Boolean))
                throw new Exception("Condition must be a boolean.");

            if ((bool)condition.Value == false)
                break;

            Result? result = InterpretStatement(whileStatement.Body);

            if (result is Break)
                break;
            else if (result is Return)
                return result;
        }

        return null;
    }

    private void InterpretAssignmentStatement(AssignmentStatement assignmentStatement)
    {
        ValueObject value = EvaluateExpression(assignmentStatement.Expression);

        _scope.Set(assignmentStatement.Identifier.Text, value);
    }

    private Result? InterpretIfStatement(IfStatement ifStatement)
    {
        foreach (IfClause clause in ifStatement.Clauses)
        {
            ValueObject condition = EvaluateExpression(clause.Condition);

            if (!condition.Type.Matches(TypeSymbol.Boolean))
                throw new Exception("Condition must be a boolean.");

            if (!(bool)condition.Value)
                continue;

            Result? result = InterpretStatement(clause.Body);

            return result;
        }

        if (ifStatement.ElseClause is not null)
        {
            Result? result = InterpretStatement(ifStatement.ElseClause);

            if (result is Return || result is Break)
                return result;
        }

        return null;
    }

    private Result? InterpretForStatement(ForStatement forStatement)
    {
        ValueObject lowerBoundObject = EvaluateExpression(forStatement.LowerBound);
        ValueObject upperBoundObject = EvaluateExpression(forStatement.UpperBound);

        if (!lowerBoundObject.Type.Matches(TypeSymbol.Float) || !upperBoundObject.Type.Matches(TypeSymbol.Float))
            throw new Exception("Bounds must be floats.");

        double lowerBound = (double)lowerBoundObject.Value, upperBound = (double)upperBoundObject.Value;
        double iterator = lowerBound;

        EnterScope();
        _scope.Declare(forStatement.Identifier.Text, TypeSymbol.Float, ValueObject.Float(iterator));

        while (iterator <= upperBound)
        {
            _scope.Set(forStatement.Identifier.Text, ValueObject.Float(iterator));

            Result? result = InterpretStatement(forStatement.Body);

            if (result is Break)
                break;
            else if (result is Return)
            {
                LeaveScope();
                return result;
            }

            iterator += 1;
        }

        LeaveScope();

        return null;
    }

    private void InterpretCallStatement(CallStatement callStatement)
    {
        List<ValueObject> arguments = new();

        foreach (ExpressionSyntax argument in callStatement.Arguments)
            arguments.Add(EvaluateExpression(argument));

        ValueObject valueObject = _scope.Get(callStatement.Identifier.Text);

        if (valueObject.Type is not FunctionTypeSymbol functionType)
            throw new Exception("Only functions can be called.");

        if (arguments.Count != functionType.ParameterTypes.Count)
            throw new Exception("Incorrect number of arguments.");

        for (int i = 0; i < functionType.ParameterTypes.Count; i++)
        {
            if (!arguments[i].Type.Matches(functionType.ParameterTypes[i]))
                throw new Exception("Function parameter types do not match.");
        }

        ((Func<List<ValueObject>, Result?>)valueObject.Value)(arguments);
    }

    private void InterpretVariableDeclarationStatement(VariableDeclarationStatement declarationStatement)
    {
        string identifier = declarationStatement.Identifier.Text;
        ValueObject value = EvaluateExpression(declarationStatement.Expression);

        if (declarationStatement.Type is InferredTypeSyntax)
            _scope.Declare(identifier, value.Type, value, declarationStatement.IsConstant);
        else
        {
            TypeSymbol typeSymbol = TypeSymbol.FromSyntax(declarationStatement.Type);

            _scope.Declare(identifier, typeSymbol, value, declarationStatement.IsConstant);
        }
    }

    private ValueObject EvaluateExpression(ExpressionSyntax expression)
    {
        if (expression is VariableExpression variableExpression)
            return _scope.Get(variableExpression.Identifier.Text);
        else if (expression is BinaryExpressionSyntax binaryExpression)
            return EvaluateBinaryExpression(binaryExpression);
        else if (expression is UnaryExpressionSyntax unaryExpression)
            return EvaluateUnaryExpression(unaryExpression);
        else if (expression is LiteralExpressionSyntax literalExpression)
            return ValueObject.Infer(literalExpression.Value);
        else if (expression is CallExpression callExpression)
            return EvaluateCallExpression(callExpression);
        else if (expression is ArrayExpression arrayExpression)
            return EvaluateArrayExpression(arrayExpression);

        throw new Exception("Invalid expression.");
    }

    private ValueObject EvaluateArrayExpression(ArrayExpression arrayExpression)
    {
        List<ValueObject> elements = new();

        foreach (ExpressionSyntax element in arrayExpression.Elements)
            elements.Add(EvaluateExpression(element));

        ValueObject? firstElement = elements.FirstOrDefault();

        if (firstElement is null)
            throw new Exception("Array must have more than one element");

        return ValueObject.Array(elements, TypeSymbol.Array(firstElement.Type));
    }

    private void InterpretFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
        List<ParameterSyntax> parameters = functionDeclaration.Parameters;
        Environment definedScope = _scope;

        Result? function(List<ValueObject> arguments)
        {
            if (arguments.Count != parameters.Count)
                throw new Exception($"{parameters.Count} arguments expected, got {arguments.Count}.");

            Environment calledScope = _scope;
            _scope = new Environment(definedScope);

            for (int i = 0; i < parameters.Count; i++)
                _scope.Declare(parameters[i].Identifier.Text, arguments[i].Type, arguments[i]);

            Result? result = InterpretBlockStatement(functionDeclaration.Body);

            _scope = calledScope;

            if (result is Return)
                return result;
            else if (result is Break)
                throw new Exception("Cannot break from a function");

            return new Return(ValueObject.Void);
        }

        TypeSymbol returnType = TypeSymbol.FromSyntax(functionDeclaration.ReturnType);
        List<TypeSymbol> parameterTypes = new();

        foreach (ParameterSyntax parameter in parameters)
            parameterTypes.Add(TypeSymbol.FromSyntax(parameter.Type));

        FunctionTypeSymbol functionType = TypeSymbol.Function(returnType, parameterTypes);
        ValueObject valueObject = ValueObject.Function(function, functionType);

        _scope.Declare(functionDeclaration.Identifier.Text, functionType, valueObject);
    }

    private ValueObject EvaluateCallExpression(CallExpression callExpression)
    {
        List<ValueObject> arguments = new();

        foreach (ExpressionSyntax argument in callExpression.Arguments)
            arguments.Add(EvaluateExpression(argument));

        ValueObject valueObject = _scope.Get(callExpression.Identifier.Text);

        if (valueObject.Type is not FunctionTypeSymbol functionType)
            throw new Exception("Only functions can be called.");

        if (arguments.Count != functionType.ParameterTypes.Count)
            throw new Exception("Incorrect number of arguments.");

        for (int i = 0; i < functionType.ParameterTypes.Count; i++)
        {
            if (!arguments[i].Type.Matches(functionType.ParameterTypes[i]))
                throw new Exception("Function parameter types do not match.");
        }

        Result? result = ((Func<List<ValueObject>, Result?>)valueObject.Value)(arguments);

        if (result is Return r && r.Value is not null)
        {
            if (!r.Value.Type.Matches(functionType.ReturnType))
                throw new Exception($"Function did not return the correct type.");

            return r.Value;
        }

        return ValueObject.Void;
    }

    private ValueObject EvaluateUnaryExpression(UnaryExpressionSyntax unaryExpression)
    {
        ValueObject operand = EvaluateExpression(unaryExpression.Operand);

        if (unaryExpression.Operator.Kind == SyntaxKind.MinusToken && operand.Type == TypeSymbol.Float)
            return ValueObject.Float(-(double)operand.Value);

        throw new Exception($"No unary operator exists for types {operand.Type}.");
    }

    private ValueObject EvaluateBinaryExpression(BinaryExpressionSyntax b)
    {
        ValueObject leftObject = EvaluateExpression(b.Left);
        ValueObject rightObject = EvaluateExpression(b.Right);

        if (leftObject.Type.Matches(TypeSymbol.Float) && rightObject.Type.Matches(TypeSymbol.Float))
        {
            double left = (double)leftObject.Value;
            double right = (double)rightObject.Value;

            switch (b.Operator.Kind)
            {
                case SyntaxKind.PlusToken:
                    return ValueObject.Float(left + right);
                case SyntaxKind.MinusToken:
                    return ValueObject.Float(left - right);
                case SyntaxKind.StarToken:
                    return ValueObject.Float(left * right);
                case SyntaxKind.SlashToken:
                    return ValueObject.Float(left / right);
            }
        }

        if (leftObject.Type.Matches(TypeSymbol.Boolean) && rightObject.Type.Matches(TypeSymbol.Boolean))
        {
            bool left = (bool)leftObject.Value;
            bool right = (bool)rightObject.Value;

            switch (b.Operator.Kind)
            {
                case SyntaxKind.DoublePipeToken:
                    return ValueObject.Boolean(left || right);
                case SyntaxKind.DoubleAmpersandToken:
                    return ValueObject.Boolean(left && right);
            }
        }

        if (b.Operator.Kind == SyntaxKind.DoubleEqualsToken)
        {
            if (!leftObject.Type.Matches(rightObject.Type))
                throw new Exception("Cannot compare values of different types.");

            return ValueObject.Boolean(leftObject.Value.Equals(rightObject.Value));
        }
        else if (b.Operator.Kind == SyntaxKind.BangEqualsToken)
        {
            if (!leftObject.Type.Matches(rightObject.Type))
                throw new Exception("Cannot compare values of different types.");

            return ValueObject.Boolean(!leftObject.Value.Equals(rightObject.Value));
        }
        else if (b.Operator.Kind == SyntaxKind.PlusToken)
        {
            if (leftObject.Type.Matches(TypeSymbol.String) && rightObject.Type.Matches(TypeSymbol.String))
                return ValueObject.String((string)leftObject.Value + (string)rightObject.Value);
        }

        throw new Exception($"No binary operator exists for types {leftObject.Type} and {rightObject.Type}.");
    }
}