using System;
using System.Collections.Generic;
using System.IO;
using Dyndle.Tools.Core.Configuration;
using Dyndle.Tools.Generator.Models;

namespace Dyndle.Tools.Generator
{
    public enum ExportType { Models, Views }
    public interface IGeneratorConfiguration : ICoreConfiguration
    {
        //public static readonly string DEFAULT_MIN_DD4T_CORE_VERSION = "2.5";
        //public static readonly string DEFAULT_MIN_DD4T_MVC_VERSION = "2.5";
        //public static readonly string DEFAULT_PATH_MODELS = "/Generated/Models";
        //public static readonly string DEFAULT_PATH_VIEWS = "/Areas/Core/Views";

        string Environment { get; set; }
        string OutputFolder { get; set; }
        string ModelNamespace { get; set; }
        string FolderInProject { get; set; }
        string MvcVersion { get; set; }
        string MinDD4TCoreVersion { get; set; }
        string MinDD4TMvcVersion { get; set; }
        string Author { get; set; }
        string PackageName { get; set; }
        string PackageVersion { get; set; }
        bool PackageAddUniqueBuildNumber { get; set; }
        int IndentNrOfSpaces { get; set; }
        bool IdentifyModelsBySchemaTitle { get; set; }
        bool AnalyzeOnly { get; set; }
        string ModelAttributeName { get; set; }
        string PageModelAttributeName { get; set; }
        IEnumerable<string> UsingNamespaces { get; set; }
        IEnumerable<string> BaseClasses { get; set; }
        IEnumerable<string> BaseClassesForMultimedia { get; set; }
        IEnumerable<string> BaseClassesForEmbedded { get; set; }
        IEnumerable<string> BaseClassesForPages { get; set; } 
        bool NoRenderData { get; set; }
        string Owner { get; set; }
        string PublicationId { get; set; }
        string FolderId { get; set; }
        string SchemaId { get; set; }
        string TemplateId { get; set; }
        PackageStyle PackageStyle { get; set; }
        ExportType ExportType { get; set; }
        DirectoryInfo WorkFolder { get; set; }
    }
}
