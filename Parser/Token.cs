using Microsoft.VisualStudio.Text;

namespace OdinLangIntegrationVS.Parser
{
    internal class Token
    {
        public Span SourceSpan { get; }
        public TokenType Type { get; }

        public Token(Span sourceSpan, TokenType type)
        {
            SourceSpan = sourceSpan;
            Type = type;
        }
    }
}
