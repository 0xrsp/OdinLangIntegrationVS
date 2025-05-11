using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace OdinLangIntegrationVS
{
    internal class OdinClassificationFormatDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("OdinKeyword")]
        [BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
        internal static ClassificationTypeDefinition OdinKeyword = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("OdinType")]
        [BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
        internal static ClassificationTypeDefinition OdinType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("OdinOperator")]
        [BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
        internal static ClassificationTypeDefinition OdinOperator = null;

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "OdinKeyword")]
        [UserVisible(true)]
        [Order(After = DefaultOrderings.Highest)]
        internal sealed class OdinKeywordClassificationFormat : ClassificationFormatDefinition
        {
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "OdinType")]
        [UserVisible(true)]
        [Order(After = DefaultOrderings.Highest)]
        internal sealed class OdinTypeClassificationFormat : ClassificationFormatDefinition
        {
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "OdinOperator")]
        [UserVisible(true)]
        [Order(After = DefaultOrderings.Highest)]
        internal sealed class OdinOperatorClassificationFormat : ClassificationFormatDefinition
        {
        }
    }
}
