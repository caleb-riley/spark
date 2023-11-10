namespace Language.Syntax;

public sealed class Parser
{
	private readonly List<SyntaxToken> _tokens;
	private int _position = 0;

	public Parser(List<SyntaxToken> tokens)
	{
		_tokens = tokens;
	}

	private SyntaxToken Peek(int offset) => _tokens[_position + offset];
	private SyntaxToken Current => Peek(0);
	private SyntaxToken Lookahead => Peek(1);

	private bool Match(SyntaxKind kind) => Current.Kind == kind;

	private bool Match(params SyntaxKind[] kinds)
	{
		int start = _position;

		for (int i = 0; i < kinds.Length; i++)
		{
			if (_tokens[start + i].Kind != kinds[i])
				return false;
		}

		return true;
	}

	private SyntaxToken Consume(SyntaxKind kind)
	{
		if (Current.Kind == kind)
		{
			SyntaxToken current = Current;
			_position++;
			return current;
		}

		throw new ArgumentException($"Mismatched token. Expected <{kind}>, got <{Current.Kind}>.");
	}

	public StatementSyntax Parse()
	{
		List<StatementSyntax> statements = new();

		while (!Match(SyntaxKind.EndOfFileToken))
			statements.Add(ParseStatement());

		Consume(SyntaxKind.EndOfFileToken);

		return new BlockStatement(statements);
	}

	private IfClause ParseIfClause(bool isFirst = true)
	{
		Consume(isFirst ? SyntaxKind.IfKeyword : SyntaxKind.ElseIfKeyword);
		Consume(SyntaxKind.LeftParenthesisToken);
		ExpressionSyntax condition = ParseExpression();
		Consume(SyntaxKind.RightParenthesisToken);
		StatementSyntax body = ParseStatement(canDeclare: false);

		return new IfClause(condition, body);
	}

	private IfStatement ParseIfStatement()
	{

		List<IfClause> clauses = new();

		clauses.Add(ParseIfClause());

		while (Match(SyntaxKind.ElseIfKeyword))
			clauses.Add(ParseIfClause(isFirst: false));

		StatementSyntax? elseClause = null;

		if (Match(SyntaxKind.ElseKeyword))
		{
			Consume(SyntaxKind.ElseKeyword);
			elseClause = ParseStatement(canDeclare: false);
		}

		return new IfStatement(clauses, elseClause);
	}

	private ExpressionSyntax ParseExpression()
	{
		return ParseBinaryExpression();
	}

	private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
	{
		ExpressionSyntax left = ParsePrimaryExpression();

		while (true)
		{
			int precedence = Current.Kind.GetPrecedence();

			if (precedence == 0 || precedence <= parentPrecedence)
				break;

			SyntaxToken op = Consume(Current.Kind);
			ExpressionSyntax right = ParseBinaryExpression(precedence);

			left = new BinaryExpressionSyntax(left, op, right);
		}

		return left;
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		if (Match(SyntaxKind.NumberToken))
		{
			SyntaxToken numberToken = Consume(SyntaxKind.NumberToken);
			return new LiteralExpressionSyntax(int.Parse(numberToken.Text), SyntaxKind.NumberToken);
		}
		else if (Match(SyntaxKind.BooleanToken))
		{
			SyntaxToken boolToken = Consume(SyntaxKind.BooleanToken);
			return new LiteralExpressionSyntax(boolToken.Text == "true", SyntaxKind.BooleanToken);
		}
		else if (Match(SyntaxKind.StringToken))
		{
			SyntaxToken stringToken = Consume(SyntaxKind.StringToken);
			return new LiteralExpressionSyntax(stringToken.Text.Substring(1, stringToken.Text.Length - 2), SyntaxKind.StringToken);
		}
		else if (Match(SyntaxKind.IdentifierToken))
		{
			SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);

			if (Match(SyntaxKind.LeftParenthesisToken))
			{
				Consume(SyntaxKind.LeftParenthesisToken);
				List<ExpressionSyntax> arguments = Match(SyntaxKind.RightParenthesisToken) ? new List<ExpressionSyntax>() : ParseValueList();
				Consume(SyntaxKind.RightParenthesisToken);

				return new CallExpression(identifier, arguments);
			}

			return new VariableExpression(identifier);
		}
		else if (Match(SyntaxKind.MinusToken))
		{
			SyntaxToken op = Consume(SyntaxKind.MinusToken);
			ExpressionSyntax expression = ParsePrimaryExpression();

			return new UnaryExpressionSyntax(expression, op);
		}
		else if (Match(SyntaxKind.LeftParenthesisToken))
		{
			Consume(SyntaxKind.LeftParenthesisToken);
			ExpressionSyntax expression = ParseExpression();
			Consume(SyntaxKind.RightParenthesisToken);

			return expression;
		}
		else if (Match(SyntaxKind.LeftBracketToken))
		{
			Consume(SyntaxKind.LeftBracketToken);
			List<ExpressionSyntax> elements = Match(SyntaxKind.RightBracketToken) ? new List<ExpressionSyntax>() : ParseValueList();
			Consume(SyntaxKind.RightBracketToken);

			return new ArrayExpression(elements);
		}

		CallExpression callExpression = ParseCallExpression();
		return callExpression;
	}

	private CallExpression ParseCallExpression()
	{
		SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
		Consume(SyntaxKind.LeftParenthesisToken);
		List<ExpressionSyntax> arguments = Match(SyntaxKind.RightParenthesisToken) ? new List<ExpressionSyntax>() : ParseValueList();
		Consume(SyntaxKind.RightParenthesisToken);
		Consume(SyntaxKind.SemiColonToken);

		return new CallExpression(identifier, arguments);
	}

	private List<ParameterSyntax> ParseParameterList()
	{
		List<ParameterSyntax> parameters = new();

		if (!Match(SyntaxKind.IdentifierToken))
			return parameters;

		parameters.Add(ParseParameter());

		while (Match(SyntaxKind.CommaToken))
		{
			Consume(SyntaxKind.CommaToken);
			parameters.Add(ParseParameter());
		}

		return parameters;
	}

	private ParameterSyntax ParseParameter()
	{
		SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
		Consume(SyntaxKind.ColonToken);
		TypeSyntax type = ParseTypeSymbol();

		return new ParameterSyntax(identifier, type);
	}

	private WhileStatement ParseWhileStatement()
	{
		Consume(SyntaxKind.WhileKeyword);
		Consume(SyntaxKind.LeftParenthesisToken);
		ExpressionSyntax condition = ParseExpression();
		Consume(SyntaxKind.RightParenthesisToken);
		StatementSyntax body = ParseStatement(canDeclare: false);

		return new WhileStatement(condition, body);
	}

	private ForStatement ParseForStatement()
	{
		Consume(SyntaxKind.ForKeyword);
		Consume(SyntaxKind.LeftParenthesisToken);
		Consume(SyntaxKind.LetKeyword);
		SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
		Consume(SyntaxKind.EqualsToken);
		ExpressionSyntax lowerBound = ParseExpression();
		Consume(SyntaxKind.CommaToken);
		ExpressionSyntax upperBound = ParseExpression();
		Consume(SyntaxKind.RightParenthesisToken);
		StatementSyntax body = ParseStatement(canDeclare: false);

		return new ForStatement(identifier, lowerBound, upperBound, body);
	}

	private VariableDeclarationStatement ParseVariableDeclarationStatement()
	{
		if (Match(SyntaxKind.VarKeyword))
		{
			Consume(SyntaxKind.VarKeyword);
			SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
			Consume(SyntaxKind.EqualsToken);
			ExpressionSyntax value = ParseExpression();
			Consume(SyntaxKind.SemiColonToken);

			return new VariableDeclarationStatement(identifier, new InferredTypeSyntax(), value, false);
		}
		else
		{
			bool isConstant = Match(SyntaxKind.ConstKeyword);
			Consume(isConstant ? SyntaxKind.ConstKeyword : SyntaxKind.LetKeyword);
			SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
			Consume(SyntaxKind.ColonToken);
			TypeSyntax type = ParseTypeSymbol();
			Consume(SyntaxKind.EqualsToken);
			ExpressionSyntax value = ParseExpression();
			Consume(SyntaxKind.SemiColonToken);

			return new VariableDeclarationStatement(identifier, type, value, isConstant);
		}
	}

	private ReturnStatement ParseReturnStatement()
	{
		Consume(SyntaxKind.ReturnKeyword);
		ExpressionSyntax? expression = Match(SyntaxKind.SemiColonToken) ? null : ParseExpression();
		Consume(SyntaxKind.SemiColonToken);

		return new ReturnStatement(expression);
	}

	private BreakStatement ParseBreakStatement()
	{
		Consume(SyntaxKind.BreakKeyword);
		Consume(SyntaxKind.SemiColonToken);

		return new BreakStatement();
	}

	private BlockStatement ParseBlockStatement()
	{
		Consume(SyntaxKind.LeftBraceToken);
		List<StatementSyntax> statements = new();

		while (!Match(SyntaxKind.RightBraceToken))
			statements.Add(ParseStatement());

		Consume(SyntaxKind.RightBraceToken);

		return new BlockStatement(statements);
	}

	private CallStatement ParseCallStatement()
	{
		SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
		Consume(SyntaxKind.LeftParenthesisToken);
		List<ExpressionSyntax> arguments = Match(SyntaxKind.RightParenthesisToken) ? new List<ExpressionSyntax>() : ParseValueList();
		Consume(SyntaxKind.RightParenthesisToken);
		Consume(SyntaxKind.SemiColonToken);

		return new CallStatement(identifier, arguments);
	}

	private List<ExpressionSyntax> ParseValueList()
	{
		List<ExpressionSyntax> values = new();

		values.Add(ParseExpression());

		while (Match(SyntaxKind.CommaToken))
		{
			Consume(SyntaxKind.CommaToken);
			values.Add(ParseExpression());
		}

		return values;
	}

	private AssignmentStatement ParseAssignmentStatement()
	{
		SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
		Consume(SyntaxKind.EqualsToken);
		ExpressionSyntax expression = ParseExpression();
		Consume(SyntaxKind.SemiColonToken);

		return new AssignmentStatement(identifier, expression);
	}

	private FunctionDeclaration ParseFunctionDeclaration()
	{
		Consume(SyntaxKind.FuncKeyword);
		SyntaxToken name = Consume(SyntaxKind.IdentifierToken);
		Consume(SyntaxKind.LeftParenthesisToken);
		List<ParameterSyntax> parameters = ParseParameterList();
		Consume(SyntaxKind.RightParenthesisToken);
		Consume(SyntaxKind.ColonToken);
		TypeSyntax returnType;

		if (Match(SyntaxKind.VoidKeyword))
			returnType = new ObjectTypeSyntax(Consume(SyntaxKind.VoidKeyword));
		else
			returnType = ParseTypeSymbol();

		BlockStatement body = ParseBlockStatement();

		return new FunctionDeclaration(name, parameters, returnType, body);
	}

	private StatementSyntax ParseStatement(bool canDeclare = true)
	{
		if (Match(SyntaxKind.IfKeyword))
			return ParseIfStatement();
		else if ((Match(SyntaxKind.LetKeyword) || Match(SyntaxKind.ConstKeyword) || Match(SyntaxKind.VarKeyword)) && canDeclare)
			return ParseVariableDeclarationStatement();
		else if (Match(SyntaxKind.ForKeyword))
			return ParseForStatement();
		else if (Match(SyntaxKind.LeftBraceToken))
			return ParseBlockStatement();
		else if (Match(SyntaxKind.WhileKeyword))
			return ParseWhileStatement();
		else if (Match(SyntaxKind.ReturnKeyword))
			return ParseReturnStatement();
		else if (Match(SyntaxKind.BreakKeyword))
			return ParseBreakStatement();
		else if (Match(SyntaxKind.IdentifierToken, SyntaxKind.EqualsToken))
			return ParseAssignmentStatement();
		else if (Match(SyntaxKind.FuncKeyword) && canDeclare)
			return ParseFunctionDeclaration();
		else
			return ParseCallStatement();
	}

	private List<TypeSyntax> ParseTypeList(SyntaxKind endToken)
	{
		List<TypeSyntax> types = new();

		if (Match(endToken))
			return types;

		types.Add(ParseTypeSymbol());

		while (Match(SyntaxKind.CommaToken))
		{
			Consume(SyntaxKind.CommaToken);
			types.Add(ParseTypeSymbol());
		}

		return types;
	}

	private TypeSyntax ParseTypeSymbol()
	{
		if (Match(SyntaxKind.LeftParenthesisToken))
		{
			Consume(SyntaxKind.LeftParenthesisToken);
			List<TypeSyntax> parameterTypes = ParseTypeList(SyntaxKind.RightParenthesisToken);
			Consume(SyntaxKind.RightParenthesisToken);
			Consume(SyntaxKind.ArrowToken);
			TypeSyntax returnType = ParseTypeSymbol();

			return new FunctionTypeSyntax(returnType, parameterTypes);
		}

		SyntaxToken identifier = Consume(SyntaxKind.IdentifierToken);
		TypeSyntax objectType = new ObjectTypeSyntax(identifier);

		if (!Match(SyntaxKind.LeftBracketToken))
			return objectType;

		Consume(SyntaxKind.LeftBracketToken);
		Consume(SyntaxKind.RightBracketToken);

		return new ArrayTypeSyntax(objectType);
	}
}
