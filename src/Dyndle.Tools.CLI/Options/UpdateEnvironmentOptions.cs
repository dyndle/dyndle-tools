using CommandLine;
using Dyndle.Tools.Environments;

namespace Dyndle.Tools.CLI
{
    /// <summary>
    /// Update environment has the same options as 'Add Environment' but all settings except name are optional
    /// </summary>
    [Verb("update-environment", HelpText = "Update environment")]
    public class UpdateEnvironmentOptions : Options, IAddOrUpdateEnvironmentsConfiguration
    {
        [Option("name", Required = true, HelpText = "Name of the environment (choose any name)")]
        public string Name { get; set; }
        [Option("username", Required = false, HelpText = "Tridion Username")]
        public string Username { get; set; }

        [Option("password", Required = false, HelpText = "Tridion Password")]
        public string Password { get; set; }

        [Option("domain", Required = false, HelpText = "Tridion Domain")]
        public string UserDomain { get; set; }

        [Option("url", Required = false, HelpText = "Tridion CMS URL")]
        public string CMSUrl { get; set; }
        [Option('d', "default", Required = false, HelpText = "Make this the default environment")]
        public bool MakeDefault { get; set; }
    }
}