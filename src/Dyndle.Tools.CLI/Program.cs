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
                Parser.Default.ParseArguments<ModelOptions, ViewOptions, AddEnvironmentOptions, ListEnvironmentOptions, InstallerOptions, CreateInstallPackageOptions>(args)
                  .WithParsed((ModelOptions opts) => ExportModels(opts))
                  .WithParsed((ViewOptions opts) => ExportViews(opts))
                  .WithParsed((AddEnvironmentOptions opts) => AddEnvironment(opts))
                  .WithParsed((ListEnvironmentOptions opts) => ListEnvironments(opts))
                  .WithParsed((CreateInstallPackageOptions opts) => CreateInstallPackage(opts))
                  .WithParsed((InstallerOptions opts) => Install(opts))
                  .WithNotParsed((errs) => HandleParseError(errs));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }


        private static void HandleParseError(IEnumerable<Error> errs)
        {
        }

        private static void ExportModels(ModelOptions opts)
        {
            var env = EnvironmentManager.Get(opts.Environment);
            if (env == null)
            {
                Console.WriteLine(
                    "you must create an environment before you can generate models or views - try dyndle help add-environment");
                return;
            }
            CoreserviceClientFactory.SetEnvironment(env);
            var module = new ModelGenerator(opts);

            var packagePath = module.Run();
            Console.WriteLine("output created at " + packagePath);
        }

        private static void ExportViews(ViewOptions opts)
        {

            var env = EnvironmentManager.Get(opts.Environment);
            if (env == null)
            {
                Console.WriteLine(
                    "you must create an environment before you can generate models or views - try dyndle help add-environment");
                return;
            }
            CoreserviceClientFactory.SetEnvironment(env);

            var module = new ViewGenerator(opts);


            var packagePath = module.Run();
            Console.WriteLine("output created at " + packagePath);
        }

        private static void Install(InstallerOptions opts)
        {

            var env = EnvironmentManager.Get(opts.Environment);
            if (env == null)
            {
                Console.WriteLine(
                    "you must create an environment first - try dyndle help add-environment");
                return;
            }
            CoreserviceClientFactory.SetEnvironment(env);

            var module = new Installer.Installer(opts);

            var output = module.Run();
            Console.WriteLine(output);
        }

        private static void CreateInstallPackage(CreateInstallPackageOptions opts)
        {
#if DEBUG
            var env = EnvironmentManager.Get(opts.Environment);
            if (env == null)
            {
                Console.WriteLine(
                    "you must create an environment first - try dyndle help add-environment");
                return;
            }
            CoreserviceClientFactory.SetEnvironment(env);

            var module = new InstallPackageCreator.InstallPackageCreator(opts);

            var output = module.Run();
            Console.WriteLine(output);
#endif
        }

        private static void AddEnvironment(AddEnvironmentOptions opts)
        {
            IToolsModule module = new AddEnvironment(opts);
            var result = module.Run();
            Console.WriteLine(result);
        }
        private static void ListEnvironments(ListEnvironmentOptions opts)
        {
            IToolsModule module = new ListEnvironments(opts);
            var result = module.Run();
            Console.WriteLine(result);
        }

    }
}
