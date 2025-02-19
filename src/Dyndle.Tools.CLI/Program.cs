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
using Tridion.CoreService.Tools;

namespace Dyndle.Tools.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<ModelOptions, ViewOptions, AddEnvironmentOptions, UpdateEnvironmentOptions, DeleteEnvironmentOptions, ListEnvironmentOptions, InstallerOptions, CreateInstallPackageOptions>(args)
                  .WithParsed((ModelOptions opts) => ExportModels(opts))
                  .WithParsed((ViewOptions opts) => ExportViews(opts))
                  .WithParsed((AddEnvironmentOptions opts) => AddEnvironment(opts))
                  .WithParsed((UpdateEnvironmentOptions opts) => UpdateEnvironment(opts))
                  .WithParsed((DeleteEnvironmentOptions opts) => DeleteEnvironment(opts))
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

            var module = new InstallPackageCreator.InstallPackageCreator(opts);

            var output = module.Run();
            Console.WriteLine(output);
#endif
        }

        private static void AddEnvironment(AddEnvironmentOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Password))
            {
                opts.Password = GetPassword();
            }
            IToolsModule module = new AddEnvironment(opts);

            var result = module.Run();
            Console.WriteLine(result);
        }

        private static string GetPassword()
        {
            Console.Write("Please enter the password: ");
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.WriteLine();
            return pass;
        }

        private static void UpdateEnvironment(UpdateEnvironmentOptions opts)
        {
            if (string.IsNullOrEmpty(opts.Password) && ! string.IsNullOrEmpty(opts.Username))
            {
                opts.Password = GetPassword();
            }

            IToolsModule module = new UpdateEnvironment(opts);
            var result = module.Run();
            Console.WriteLine(result);
        }
        private static void ListEnvironments(ListEnvironmentOptions opts)
        {
            IToolsModule module = new ListEnvironments(opts);
            var result = module.Run();
            Console.WriteLine(result);
        }
        private static void DeleteEnvironment(DeleteEnvironmentOptions opts)
        {
            IToolsModule module = new DeleteEnvironment(opts);
            var result = module.Run();
            Console.WriteLine(result);
        }

    }
}
