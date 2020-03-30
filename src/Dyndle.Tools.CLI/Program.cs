using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.Configuration;
using Dyndle.Tools.Core.ToolsModules;
using Dyndle.Tools.Core.Utils;
using Dyndle.Tools.Environments;
using Dyndle.Tools.Generator;
using static Dyndle.Tools.CLI.Options;
using Dyndle.Tools.Generator.Generators;
using Dyndle.Tools.Generator.Models;

namespace Dyndle.Tools.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

            Parser.Default.ParseArguments<ModelOptions, ViewOptions, AddEnvironmentOptions, ListEnvironmentOptions>(args)
              .WithParsed((ModelOptions opts) => ExportModels(opts))
              .WithParsed((ViewOptions opts) => ExportViews(opts))
              .WithParsed((AddEnvironmentOptions opts) => AddEnvironment(opts))
              .WithParsed((ListEnvironmentOptions opts) => ListEnvironments(opts))
              .WithNotParsed((errs) => HandleParseError(errs));
                // Console.ReadKey();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }


        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.ReadLine();
        }

        private static void ExportModels(ModelOptions opts)
        {
            var configuration = ConfigurationFactory.ToConfiguration<GeneratorConfiguration>(opts);
            var env = EnvironmentManager.Get(opts.Environment);
            if (env == null)
            {
                Console.WriteLine(
                    "you must create an environment before you can generate models or views - try dyndle help add-environment");
                return;
            }
            CoreserviceClientFactory.SetEnvironment(env);
            var modelGenerator = new ModelGenerator(configuration);

            var packagePath = modelGenerator.Run();
            Console.WriteLine("output created at " + packagePath);
        }

        private static void ExportViews(ViewOptions opts)
        {
            var configuration = ConfigurationFactory.ToConfiguration<GeneratorConfiguration>(opts);
            
            var env = EnvironmentManager.Get(opts.Environment);
            if (env == null)
            {
                Console.WriteLine(
                    "you must create an environment before you can generate models or views - try dyndle help add-environment");
                return;
            }
            CoreserviceClientFactory.SetEnvironment(env);

            var viewGenerator = new ViewGenerator(configuration);


            var packagePath = viewGenerator.Run();
            Console.WriteLine("output created at " + packagePath);
        }

        private static void AddEnvironment(AddEnvironmentOptions opts)
        {
            var configuration = ConfigurationFactory.ToConfiguration<EnvironmentsConfiguration>(opts);
            IToolsModule module = new AddEnvironment(configuration);
            var result = module.Run();
            Console.WriteLine(result);
        }
        private static void ListEnvironments(ListEnvironmentOptions opts)
        {
            var configuration = ConfigurationFactory.ToConfiguration<EnvironmentsConfiguration>(opts);
            IToolsModule module = new ListEnvironments(configuration);
            var result = module.Run();
            Console.WriteLine(result);
        }

        //private static IConfiguration GetConfiguration(ModelOptions opts)
        //{
        //    return GetConfiguration((Options)opts, ExportType.Models);
        //}

        //private static IConfiguration GetConfiguration(ViewOptions opts)
        //{
        //    return GetConfiguration((Options)opts, ExportType.Views);
        //}

        //private static IConfiguration GetConfiguration(Options opts, ExportType exportType)
        //{
        //    var c = new Configuration()
        //    {
        //        Author = opts.Author,
        //        Owner = opts.Owner,
        //        MinDD4TCoreVersion = opts.MinDD4TCoreVersion,
        //        MinDD4TMvcVersion = opts.MinDD4TMVCVersion,
        //        ModelNamespace = opts.ModelNamespace,
        //        FolderInProject = opts.FolderInProject,
        //        MvcVersion = opts.MvcVersion,
        //        PackageName = opts.PackageName,
        //        PackageVersion = opts.PackageVersion,
        //        PackageAddUniqueBuildNumber = opts.PackageAddUniqueBuildNumber,
        //        IndentNrOfSpaces = opts.IndentNrOfSpaces,
        //        LogFolder = ".",
        //        IdentifyModelsBySchemaTitle = opts.IdentifyByTitle,
        //        ModelAttributeName = opts.ModelAttribute,
        //        PageModelAttributeName = opts.PageModelAttribute,
        //        UsingNamespaces = opts.UsingStatements,
        //        BaseClasses = opts.BaseClasses,
        //        BaseClassesForMultimedia = opts.BaseClassesForMultimedia,
        //        BaseClassesForEmbedded = opts.BaseClassesForEmbedded,
        //        BaseClassesForPages = opts.BaseClassesForPages,
        //        NoRenderData = opts.NoRenderData,
        //        AnalyzeOnly = opts.AnalyzeOnly,
        //        OutputFolder = opts.OutputFolder,
        //        PublicationId = opts.PublicationId,
        //        FolderId = opts.FolderId,
        //        SchemaId = opts.SchemaId,
        //        PackageStyle = opts.ModeClasses ? PackageStyle.CsFile : PackageStyle.Nuget,
        //        ExportType = exportType
        //    };            
        //    return c;
        //}
    }
}
