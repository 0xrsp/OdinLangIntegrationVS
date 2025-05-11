using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace OdinLangIntegrationVS
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("odin")]
    internal class OdinClassifierProvider : IClassifierProvider
    {
        private readonly OdinSyntaxHighlighting _syntaxHighlighting;

        [ImportingConstructor]
        public OdinClassifierProvider(IClassificationTypeRegistryService classificationTypeRegistry)
        {
            _syntaxHighlighting = new OdinSyntaxHighlighting(classificationTypeRegistry);
        }

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() =>
                new OdinClassifier(buffer, _syntaxHighlighting));
        }
    }
}
