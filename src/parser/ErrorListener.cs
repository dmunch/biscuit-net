namespace biscuit_net.Parser;

using Antlr4.Runtime;

class ErrorListener : IAntlrErrorListener<IToken>
{
    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {

    }
}
