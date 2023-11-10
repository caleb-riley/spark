using Language.Syntax;
using Language.Runtime;

namespace Language;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
            throw new Exception("Must supply a path to the source file.");
        
        string input = File.ReadAllText(args[0]);
        Execute(input);
    }

    public static void Execute(string source)
    {
        Lexer lexer = new(source);
        List<SyntaxToken> tokens = new();

        while (true)
        {
            SyntaxToken token = lexer.NextToken();

            if (token.Kind == SyntaxKind.EndOfFileToken)
            {
                tokens.Add(new SyntaxToken(SyntaxKind.EndOfFileToken, "\0"));
                break;
            }

            if (token.Kind != SyntaxKind.Whitespace && token.Kind != SyntaxKind.CommentToken)
                tokens.Add(token);
        }

        Parser parser = new(tokens);
        StatementSyntax body = parser.Parse();
        new Interpreter(body).Interpret();
    }
}
