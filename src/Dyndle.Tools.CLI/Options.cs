using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.CLI
{
    public class Options
    {

        // for now, we will configure the Tridion CM through the app.config

        //[Option('u', "username", Required = true, HelpText = "Tridion Username")]
        //public string Username { get; set; }

        //[Option('p', "password", Required = true, HelpText = "Tridion Password")]
        //public string Password { get; set; }

        //[Option('d', "domain", Required = false, HelpText = "Tridion Domain")]
        //public string Domain { get; set; }

        //[Option('h', "hostname", Required = true, HelpText = "Tridion Hostname")]
        //public string Hostname { get; set; }

        [Option('n', "nuget", Required = false, HelpText = "Export as NuGet package")]
        public bool ModeNuGet { get; set; }

        [Option('c', "classes", Required = false, HelpText = "Export as cs file with classes")]
        public bool ModeClasses { get; set; }

        [Option("author", Required = false, HelpText = "Package author", Default = "John Doe")]
        public string Author { get; set; }

        [Option("owner", Required = false, HelpText = "Package owner", Default = "MyCompany")]
        public string Owner { get; set; }

        [Option("dd4t-core", Required = false, HelpText = "DD4T Core minimum version")]
        public string MinDD4TCoreVersion { get; set; }

        [Option("dd4t-mvc", Required = false, HelpText = "DD4T MVC minimum version")]
        public string MinDD4TMVCVersion { get; set; }

        [Option("package-name", Required = false, HelpText = "Package name")]
        public string PackageName { get; set; }

        [Option("package-version", Required = false, HelpText = "Package version", Default = "1.0.0")]
        public string PackageVersion { get; set; }

        [Option("prerelease", Required = false, HelpText = "Create a pre-release NuGet package")]
        public bool PackageAddUniqueBuildNumber { get; set; }

        [Option("indent", Required = false, HelpText = "Number of spaces to indent", Default = 4)]
        public int IndentNrOfSpaces  { get; set; }

        [Option("identify-by-title", Required = false, HelpText = "Use schema title to identify the ViewModel classes")]
        public bool IdentifyByTitle { get; set; }

        [Option("model-attribute", Required = false, HelpText = "Model attribute", Default = "ContentModel")]
        public string ModelAttribute { get; set; }

        [Option("using", Required = false, HelpText = "Extra namespaces to include with using statements")]
        public IEnumerable<string> UsingStatements { get; set; }

        [Option("output-folder", Required = false, HelpText = "Store generated output in this folder")]
        public string OutputFolder { get; set; }

        [Option("analyze", Required = false, HelpText = "Analyze only (do not export any classes)")]
        public bool AnalyzeOnly { get; set; }

        [Option('f', "folder", Required = false, HelpText = "Tridion folder id")]
        public string FolderId { get; set; }

        [Option('s', "schema", Required = false, HelpText = "Tridion schema id")]
        public string SchemaId { get; set; }

        [Option('p', "publication", Required = false, HelpText = "Tridion publication id")]
        public string PublicationId { get; set; }

        [Option("base", Required = false, HelpText = "Base classes / interfaces to extend / implement", Default = new[] { "ViewModelBase", "IRenderableViewModel" })]
        public IEnumerable<string> BaseClasses { get; set; }

        [Option("mmbase", Required = false, HelpText = "Base classes / interfaces to extend / implement for multimedia components", Default = new[] { "ViewModelBase", "IRenderableViewModel" })]
        public IEnumerable<string> BaseClassesForMultimedia { get; set; }

        [Option("embbase", Required = false, HelpText = "Base classes / interfaces to extend / implement for embedded fields", Default = new[] { "ViewModelBase" })]
        public IEnumerable<string> BaseClassesForEmbedded { get; set; }

        [Option("no-render-data", Required = false, HelpText = "Do not include the RenderData property (use this if the base class defines RenderData)")]
        public bool NoRenderData { get; set; }

        [Option("mvc", Required = false, HelpText = "Mvc version (MVC3, MVC4 or MVC5)", Default = "MVC5")]
        public string MvcVersion { get; set; }

        [Option("namespace", Required = false, HelpText = "Model namespace", Default = "My.Models")]
        public string ModelNamespace { get; set; }

        [Option("folder-in-project", Required = false, HelpText = "Folder within the project (after unpacking the NuGet package)")]
        public string FolderInProject { get; set; }

        [Verb("models", HelpText = "Export models")]
        public class ModelOptions : Options
        {
           

        }

        [Verb("views", HelpText = "Export views")]
        public class ViewOptions : Options
        {
           
        }


    }
}
