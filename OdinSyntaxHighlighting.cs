using System.Collections.Generic;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using OdinLangIntegrationVS.Parser;

namespace OdinLangIntegrationVS
{
    internal class OdinSyntaxHighlighting
    {
        private readonly IClassificationType _keywordType;
        private readonly IClassificationType _identType;
        private readonly IClassificationType _typeType;
        private readonly IClassificationType _commentType;
        private readonly IClassificationType _operatorType;
        private readonly IClassificationType _punctuationType;
        private readonly IClassificationType _stringType;
        private readonly IClassificationType _symRefType;
        private readonly IClassificationType _symDefType;
        private readonly IClassificationType _numberType;

        private readonly OdinHLParser _parser;

        public OdinSyntaxHighlighting(IClassificationTypeRegistryService classificationTypeRegistry)
        {
            _keywordType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            _identType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            _punctuationType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Punctuation);
            _typeType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Type);
            _commentType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _operatorType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Operator);
            _numberType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Number);
            _symRefType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.SymbolReference);
            _stringType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.String);
            _symDefType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition);

            _parser = new OdinHLParser();
        }

        private IClassificationType MapTokenTypeToClassificationType(HLElementType type)
        {
            switch (type)
            {
                default:
                case HLElementType.IDENT:
                    return _identType;
                case HLElementType.KEYWORD:
                    return _keywordType;
                case HLElementType.TYPE:
                    return _typeType;
                case HLElementType.OPERATOR:
                    return _operatorType;
                case HLElementType.PUNCTUATION:
                    return _punctuationType;
                case HLElementType.COMMENT:
                    return _commentType;
                case HLElementType.SYM_DEF:
                    return _symDefType;
                case HLElementType.SYM_REF:
                    return _symRefType;
                case HLElementType.NUMBER:
                    return _numberType;
                case HLElementType.STRING_LITERAL:
                    return _stringType;
            }
        }

        public IList<ClassificationSpan> GetSyntaxHighlightingSpans(SnapshotSpan snapshotSpan)
        {
            var results = new List<ClassificationSpan>();

            foreach (var token in _parser.Tokenize(snapshotSpan.Snapshot, snapshotSpan.Span, false))
            {
                var tokenSnapshotSpan = new SnapshotSpan(snapshotSpan.Snapshot, token.SourceSpan.Start, token.SourceSpan.Length);
                results.Add(new ClassificationSpan(tokenSnapshotSpan, MapTokenTypeToClassificationType(token.Type)));
            }

            return results;
        }
    }
}
