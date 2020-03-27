using Dyndle.Tools.Core.Models;
using System.Collections.Generic;
using System.IO;
using static Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.Core
{
    public interface IConfiguration
    {
        DirectoryInfo WorkFolder { get; }
        string OutputFolder { get; }
        string ModelNamespace { get; }
        string FolderInProject { get; }
        string MvcVersion { get; }
        string MinDD4TCoreVersion { get; }
        string MinDD4TMvcVersion { get; }
        string Author { get; }
        string PackageName { get; }
        string PackageVersion { get; }
        bool PackageAddUniqueBuildNumber { get; }
        int IndentNrOfSpaces { get; }
        string LogFolder { get; }
        bool IdentifyModelsBySchemaTitle { get; }
        bool AnalyzeOnly { get; }
        string ModelAttributeName { get; }
        string PageModelAttributeName { get; }
        IEnumerable<string> UsingNamespaces { get; }
        IEnumerable<string> BaseClasses { get; }
        bool NoRenderData { get; }
        string Owner { get; }
        IEnumerable<string> BaseClassesForMultimedia { get; }
        IEnumerable<string> BaseClassesForEmbedded { get; }
        IEnumerable<string> BaseClassesForPages { get; set; }

        string PublicationId { get; }
        string FolderId { get; }
        string SchemaId { get; }
        string TemplateId { get; }
        PackageStyle PackageStyle { get; }
        ExportType ExportType { get; set; }
    }
}
