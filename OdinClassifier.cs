using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;

namespace OdinLangIntegrationVS
{
    internal class OdinClassifier : IClassifier
    {

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        private readonly OdinSyntaxHighlighting _syntaxHighlighting;

        private void HandleTextBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            foreach (var textChange in args.Changes)
            {
                SnapshotSpan changeSpan = new SnapshotSpan(args.After, textChange.NewPosition, textChange.NewLength);
                ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(changeSpan));
            }
        }

        public OdinClassifier(ITextBuffer buffer ,OdinSyntaxHighlighting syntaxHighlighting)
        {
            _syntaxHighlighting = syntaxHighlighting;
            buffer.Changed += HandleTextBufferChanged;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            return _syntaxHighlighting.GetSyntaxHighlightingSpans(span);
        }
    }

}
