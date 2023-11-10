using Types;

public abstract class SyntaxNode { }

public abstract class ExpressionSyntax : SyntaxNode { }
public abstract class StatementSyntax : SyntaxNode { }

public sealed class ReturnStatement : StatementSyntax
{
    public ExpressionSyntax? Expression { get; }

    public ReturnStatement(ExpressionSyntax? expression)
    {
        Expression = expression;
    }
}

public sealed class ArrayExpression : ExpressionSyntax
{
    public List<ExpressionSyntax> Elements { get; }

    public ArrayExpression(List<ExpressionSyntax> elements)
    {
        Elements = elements;
    }
}

public class FunctionDeclaration : StatementSyntax
{
    public SyntaxToken Identifier { get; }
    public List<ParameterSyntax> Parameters { get; }
    public TypeSyntax ReturnType { get; }
    public BlockStatement Body { get; }

    public FunctionDeclaration(SyntaxToken identifier, List<ParameterSyntax> parameters, TypeSyntax returnType, BlockStatement body)
    {
        Identifier = identifier;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
    }
}

public sealed class BreakStatement : StatementSyntax { }

public sealed class IfStatement : StatementSyntax
{
    public List<IfClause> Clauses { get; }
    public StatementSyntax? ElseClause { get; }

    public IfStatement(List<IfClause> clauses, StatementSyntax? elseClause)
    {
        Clauses = clauses;
        ElseClause = elseClause;
    }
}

public sealed class IfClause : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public StatementSyntax Body { get; }

    public IfClause(ExpressionSyntax condition, StatementSyntax body)
    {
        Condition = condition;
        Body = body;
    }
}

public sealed class WhileStatement : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public StatementSyntax Body { get; }

    public WhileStatement(ExpressionSyntax condition, StatementSyntax body)
    {
        Condition = condition;
        Body = body;
    }
}

public class ForStatement : StatementSyntax
{
    public SyntaxToken Identifier { get; }
    public ExpressionSyntax LowerBound { get; }
    public ExpressionSyntax UpperBound { get; }
    public StatementSyntax Body { get; }

    public ForStatement(SyntaxToken identifier, ExpressionSyntax lowerBound, ExpressionSyntax upperBound, StatementSyntax body)
    {
        Identifier = identifier;
        LowerBound = lowerBound;
        UpperBound = upperBound;
        Body = body;
    }
}

public sealed class VariableDeclarationStatement : StatementSyntax
{
    public SyntaxToken Identifier { get; }
    public TypeSyntax Type { get; }
    public ExpressionSyntax Expression { get; }
    public bool IsConstant { get; }

    public VariableDeclarationStatement(SyntaxToken identifier, TypeSyntax type, ExpressionSyntax expression, bool isConstant)
    {
        Identifier = identifier;
        Type = type;
        Expression = expression;
        IsConstant = isConstant;
    }
}

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken Operator { get; }
    public ExpressionSyntax Right { get; }

    public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken op, ExpressionSyntax right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Operand { get; }
    public SyntaxToken Operator { get; }

    public UnaryExpressionSyntax(ExpressionSyntax operand, SyntaxToken op)
    {
        Operand = operand;
        Operator = op;
    }
}

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    public object Value { get; }
    public SyntaxKind Kind { get; }

    public LiteralExpressionSyntax(object value, SyntaxKind kind)
    {
        Value = value;
        Kind = kind;
    }
}

public sealed class CallExpression : ExpressionSyntax
{
    public SyntaxToken Identifier { get; }
    public List<ExpressionSyntax> Arguments { get; }

    public CallExpression(SyntaxToken identifier, List<ExpressionSyntax> arguments)
    {
        Identifier = identifier;
        Arguments = arguments;
    }
}

public sealed class CallStatement : StatementSyntax
{
    public SyntaxToken Identifier { get; }
    public List<ExpressionSyntax> Arguments { get; }

    public CallStatement(SyntaxToken identifier, List<ExpressionSyntax> arguments)
    {
        Identifier = identifier;
        Arguments = arguments;
    }
}

public sealed class VariableExpression : ExpressionSyntax
{
    public SyntaxToken Identifier { get; }

    public VariableExpression(SyntaxToken identifier)
    {
        Identifier = identifier;
    }
}

public class AssignmentStatement : StatementSyntax
{
    public SyntaxToken Identifier;
    public ExpressionSyntax Expression;

    public AssignmentStatement(SyntaxToken identifier, ExpressionSyntax expression)
    {
        Identifier = identifier;
        Expression = expression;
    }
}

public sealed class BlockStatement : StatementSyntax
{
    public List<StatementSyntax> Statements;

    public BlockStatement(List<StatementSyntax> statements)
    {
        Statements = statements;
    }
}

public sealed class ParameterSyntax : SyntaxNode
{
    public SyntaxToken Identifier { get; }
    public TypeSyntax Type { get; }

    public ParameterSyntax(SyntaxToken identifier, TypeSyntax type)
    {
        Identifier = identifier;
        Type = type;
    }
}