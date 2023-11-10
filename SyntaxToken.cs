public sealed class SyntaxToken
{
    public SyntaxKind Kind { get; }
    public string Text;

    public SyntaxToken(SyntaxKind kind, string text)
    {
        Kind = kind;
        Text = text;
    }
}