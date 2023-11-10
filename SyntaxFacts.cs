public static class SyntaxFacts
{
	public static Dictionary<string, SyntaxKind> Keywords = new()
	{
		{ "if", SyntaxKind.IfKeyword },
		{ "for", SyntaxKind.ForKeyword },
		{ "while", SyntaxKind.WhileKeyword },
		{ "let", SyntaxKind.LetKeyword },
		{ "return", SyntaxKind.ReturnKeyword },
		{ "break", SyntaxKind.BreakKeyword },
		{ "func", SyntaxKind.FuncKeyword },
		{ "const", SyntaxKind.ConstKeyword },
		{ "void", SyntaxKind.VoidKeyword },
		{ "var", SyntaxKind.VarKeyword },
		{ "elseif", SyntaxKind.ElseIfKeyword },
		{ "else", SyntaxKind.ElseKeyword },
	};

	public static readonly Dictionary<string, SyntaxKind> Symbols = new()
    {
        { "||", SyntaxKind.DoublePipeToken },
        { "&&", SyntaxKind.DoubleAmpersandToken },
        { "->", SyntaxKind.ArrowToken },
        { "!=", SyntaxKind.BangEqualsToken },
        { "==", SyntaxKind.DoubleEqualsToken },
        { "+", SyntaxKind.PlusToken },
        { "-", SyntaxKind.MinusToken },
        { "*", SyntaxKind.StarToken },
        { "/", SyntaxKind.SlashToken },
        { "(", SyntaxKind.LeftParenthesisToken },
        { ")", SyntaxKind.RightParenthesisToken },
        { "{", SyntaxKind.LeftBraceToken },
        { "}", SyntaxKind.RightBraceToken },
        { "[", SyntaxKind.LeftBracketToken },
        { "]", SyntaxKind.RightBracketToken },
        { "=", SyntaxKind.EqualsToken },
        { ",", SyntaxKind.CommaToken },
        { ";", SyntaxKind.SemiColonToken },
        { ":", SyntaxKind.ColonToken },
        { "<", SyntaxKind.LessThanToken },
        { ">", SyntaxKind.GreaterThanToken },
        { ".", SyntaxKind.PeriodToken },
    };

	public static int GetPrecedence(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.DoublePipeToken => 1,
			SyntaxKind.DoubleAmpersandToken => 2,

			SyntaxKind.DoubleEqualsToken => 3,
			SyntaxKind.BangEqualsToken => 3,
			
			SyntaxKind.PlusToken => 4,
			SyntaxKind.MinusToken => 4,
			SyntaxKind.StarToken => 5,
			SyntaxKind.SlashToken => 5,

			_ => 0,
		};
	}
}