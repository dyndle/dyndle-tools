using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using static Dyndle.Tools.CLI.Options;
using Dyndle.Tools.Core.Generators;
using Dyndle.Tools.Core.Models;

namespace Dyndle.Tools.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ModelOptions, ViewOptions>(args)
              .WithParsed((ModelOptions opts) => ExportModels(opts))
              .WithParsed((ViewOptions opts) => ExportViews(opts))
              .WithNotParsed((errs) => HandleParseError(errs));
            // Console.ReadKey();
        }

      

        private static void HandleParseError(IEnumerable<Error> errs)
        {          
            Console.ReadLine();
        }

        private static void ExportModels(ModelOptions opts)
        {
            ModelGenerator modelGenerator = new ModelGenerator(GetConfiguration(opts));
            var packagePath = modelGenerator.Generate();
            Console.WriteLine("output created at " + packagePath);
        }

        private static void ExportViews(ViewOptions opts)
        {
            ViewGenerator viewGenerator = new ViewGenerator(GetConfiguration(opts));
            var packagePath = viewGenerator.Generate();
            Console.WriteLine("output created at " + packagePath);
        }

        private static IConfiguration GetConfiguration (ModelOptions opts)
        {
            return GetConfiguration((Options)opts, ExportType.Models);
        }

        private static IConfiguration GetConfiguration(ViewOptions opts)
        {
             return GetConfiguration((Options)opts, ExportType.Views);
        }
        
        private static IConfiguration GetConfiguration(Options opts, ExportType exportType)
        {
            IConfiguration c = new Configuration()
            {
                Author = opts.Author,
                Owner = opts.Owner,
                MinDD4TCoreVersion = opts.MinDD4TCoreVersion,
                MinDD4TMvcVersion = opts.MinDD4TMVCVersion,
                ModelNamespace = opts.ModelNamespace,
                FolderInProject = opts.FolderInProject,
                MvcVersion = opts.MvcVersion,
                PackageName = opts.PackageName,
                PackageVersion = opts.PackageVersion,
                PackageAddUniqueBuildNumber = opts.PackageAddUniqueBuildNumber,
                IndentNrOfSpaces = opts.IndentNrOfSpaces,
                LogFolder = ".",
                IdentifyModelsBySchemaTitle = opts.IdentifyByTitle,
                ModelAttributeName = opts.ModelAttribute,
                PageModelAttributeName = opts.PageModelAttribute,
                UsingNamespaces = opts.UsingStatements,
                BaseClasses = opts.BaseClasses,
                BaseClassesForMultimedia = opts.BaseClassesForMultimedia,
                BaseClassesForEmbedded = opts.BaseClassesForEmbedded,
                BaseClassesForPages = opts.BaseClassesForPages,
                NoRenderData = opts.NoRenderData,
                AnalyzeOnly = opts.AnalyzeOnly,
                OutputFolder = opts.OutputFolder,
                PublicationId = opts.PublicationId,
                FolderId = opts.FolderId,
                SchemaId = opts.SchemaId,
                PackageStyle = opts.ModeClasses ? PackageStyle.CsFile : PackageStyle.Nuget,
                ExportType = exportType
        };
            return c;
        }
    }
}
