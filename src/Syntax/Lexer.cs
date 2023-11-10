namespace Language.Syntax;

public sealed class Lexer
{
    private readonly string Source;
    private int Position = 0;

    public Lexer(string source)
    {
        Source = source;
    }

    private char Current => Peek(0);
    private char Peek(int offset) => Position + offset < Source.Length ? Source[Position + offset] : '\0';

    SyntaxToken ScanIdentifier()
    {
        int start = Position;

        while (char.IsLetter(Current) || Current == '_')
            Position++;

        string text = Source.Substring(start, Position - start);

        if (text == "true" || text == "false")
            return new SyntaxToken(SyntaxKind.BooleanToken, text);

        if (SyntaxFacts.Keywords.TryGetValue(text, out SyntaxKind kind))
            return new SyntaxToken(kind, text);

        return new SyntaxToken(SyntaxKind.IdentifierToken, text);
    }

    SyntaxToken ScanNumber()
    {
        int start = Position;

        while (char.IsDigit(Current))
            Position++;

        return new SyntaxToken(SyntaxKind.NumberToken, Source[start..Position]);
    }

    SyntaxToken ScanWhitespace()
    {
        int start = Position;

        while (char.IsWhiteSpace(Current))
            Position++;

        return new SyntaxToken(SyntaxKind.Whitespace, Source[start..Position]);
    }

    SyntaxToken ScanString()
    {
        int start = Position;
        Position++;

        while (Current != '"')
        {
            if (Current == '\0')
                throw new Exception("Unterminated string");

            Position++;
        }

        Position++;

        string text = Source[start..Position];

        return new SyntaxToken(SyntaxKind.StringToken, text);
    }

    public SyntaxToken ScanComment()
    {
        int start = Position;

        while (Current != '\n' && Current != '\0')
            Position++;
        
        string text = Source[start..Position];

        return new SyntaxToken(SyntaxKind.CommentToken, text);
    }

    public SyntaxToken NextToken()
    {
        if (Position >= Source.Length)
            return new SyntaxToken(SyntaxKind.EndOfFileToken, Current.ToString());

        foreach (KeyValuePair<string, SyntaxKind> pair in SyntaxFacts.Symbols)
        {
            if (Position + pair.Key.Length <= Source.Length && Source.Substring(Position, pair.Key.Length) == pair.Key)
            {
                Position += pair.Key.Length;
                return new SyntaxToken(pair.Value, pair.Key);
            }
        }

        if (char.IsLetter(Current) || Current == '_')
            return ScanIdentifier();
        else if (char.IsDigit(Current))
            return ScanNumber();
        else if (char.IsWhiteSpace(Current))
            return ScanWhitespace();
        else if (Current == '"')
            return ScanString();
        else if (Current == '#')
            return ScanComment();

        throw new ArgumentException($"Invalid character: {Current}");
    }
}
