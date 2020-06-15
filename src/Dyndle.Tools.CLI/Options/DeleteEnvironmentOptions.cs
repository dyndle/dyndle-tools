using CommandLine;
using Dyndle.Tools.Environments;

namespace Dyndle.Tools.CLI
{
    [Verb("delete-environment", HelpText = "Delete environment")]
    public class DeleteEnvironmentOptions : Options, IEnvironmentsConfiguration
    {

        [Option("name", Required = true, HelpText = "Name of the environment (choose any name)")]
        public string Name { get; set; }

    }
}