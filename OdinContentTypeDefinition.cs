using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace OdinLangIntegrationVS
{
    internal static class OdinContentTypeDefinition
    {
        [Export(typeof(ContentTypeDefinition))]
        [Name("odin")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition OdinContentType = null;

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType("odin")]
        [FileExtension(".odin")]
        internal static FileExtensionToContentTypeDefinition OdinFileExtension = null;
    }
}
