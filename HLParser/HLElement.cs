using Microsoft.VisualStudio.Text;

namespace OdinLangIntegrationVS.Parser
{
    internal class HLElement
    {
        public Span SourceSpan { get; }
        public HLElementType Type { get; }

        public HLElement(Span sourceSpan, HLElementType type)
        {
            SourceSpan = sourceSpan;
            Type = type;
        }
    }
}
