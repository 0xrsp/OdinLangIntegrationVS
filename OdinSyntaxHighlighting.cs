using System;
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
        private readonly IClassificationType _plaintextType;
        private readonly IClassificationType _typeType;
        private readonly IClassificationType _whitespaceType;
        private readonly IClassificationType _commentType;
        private readonly IClassificationType _operatorType;
        private readonly IClassificationType _punctuationType;
        private readonly OdinParser _parser;

        public OdinSyntaxHighlighting(IClassificationTypeRegistryService classificationTypeRegistry)
        {
            _keywordType = classificationTypeRegistry.GetClassificationType("OdinKeyword");
            _identType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            _plaintextType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Text);
            _punctuationType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Punctuation);
            _typeType = classificationTypeRegistry.GetClassificationType("OdinType");
            _commentType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _operatorType = classificationTypeRegistry.GetClassificationType("OdinOperator");
            _whitespaceType = classificationTypeRegistry.GetClassificationType(PredefinedClassificationTypeNames.WhiteSpace);

            _parser = new OdinParser();
        }

        private IClassificationType MapTokenTypeToClassificationType(TokenType type)
        {
            switch (type)
            {
                case TokenType.IDENTIFIER:
                    return _identType;
                case TokenType.KEYWORD:
                    return _keywordType;
                case TokenType.TYPE:
                    return _typeType;
                case TokenType.OPERATOR:
                    return _operatorType;
                case TokenType.PUNCTUATION:
                    return _punctuationType;
                case TokenType.COMMENT:
                    return _commentType;
                default:
                    return _plaintextType;
            }
        }

        public IList<ClassificationSpan> GetSyntaxHighlightingSpans(SnapshotSpan snapshotSpan)
        {
            var results = new List<ClassificationSpan>();
            var text = snapshotSpan.GetText();
            
            foreach (var token in _parser.Tokenize(text, false))
            {
                var tokenSnapshotSpan = new SnapshotSpan(snapshotSpan.Snapshot, snapshotSpan.Start + token.SourceSpan.Start, Math.Min(snapshotSpan.Length, token.SourceSpan.Length));
                results.Add(new ClassificationSpan(tokenSnapshotSpan, MapTokenTypeToClassificationType(token.Type)));
            }

            return results;
        }
    }
}
