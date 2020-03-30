using CommandLine;

namespace Dyndle.Tools.CLI
{
    [Verb("add-environment", HelpText = "Add environments")]
    public class AddEnvironmentOptions : Options
    {

        [Option("name", Required = true, HelpText = "Name of the environment (choose any name)")]
        public string Name { get; set; }
        [Option("username", Required = true, HelpText = "Tridion Username")]
        public string Username { get; set; }

        [Option("password", Required = true, HelpText = "Tridion Password")]
        public string Password { get; set; }

        [Option("domain", Required = false, HelpText = "Tridion Domain")]
        public string Domain { get; set; }

        [Option("url", Required = true, HelpText = "Tridion CMS URL")]
        public string CMSUrl { get; set; }
        [Option('d', "default", Required = false, HelpText = "Make this the default environment")]
        public bool MakeDefault { get; set; }

    }
}