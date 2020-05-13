using CommandLine;
using Dyndle.Tools.Environments;

namespace Dyndle.Tools.CLI
{
    [Verb("list-environments", HelpText = "List environments")]
    public class ListEnvironmentOptions : Options, IEnvironmentsConfiguration
    {
        public string Name { get; set; }
        public string CMSUrl { get; set; }
        public string Username { get; set; }
        public string UserDomain { get; set; }
        public string Password { get; set; }
        public bool MakeDefault { get; set; }
    }
}