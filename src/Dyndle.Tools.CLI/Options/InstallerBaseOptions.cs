using System.IO;
using CommandLine;
using Dyndle.Tools.Environments;
using Dyndle.Tools.Installer.Configuration;

namespace Dyndle.Tools.CLI
{

    public abstract class InstallerBaseOptions : Options, IInstallerConfiguration
    {
        [Option("environment", Required = false, HelpText = "Environment name (type dyndle list-environments for a list of available environments)")]
        public string Environment { get; set; }

        [Option("install-tbbs", Required = false, HelpText = "Install the Dyndle template building blocks")]
        public bool InstallTBBs { get; set; } = true;
        [Option("install-sitemap", Required = false, HelpText = "Install the Dyndle sitemap")]
        public bool InstallSitemap { get; set; }
        [Option("install-siteconfig", Required = false, HelpText = "Install the Dyndle site configuration")]
        public bool InstallSiteConfiguration { get; set; }
        [Option("dyndle-folder", Required = false, HelpText = "Folder where Dyndle items (except pages) must be created")]
        public string DyndleFolder { get; set; }
        [Option("root-sg", Required = false, HelpText = "Publication where the Dyndle root pages must be created")]
        public string RootStructureGroup { get; set; }
        [Option("system-sg", Required = false, HelpText = "Publication where the Dyndle system pages must be created")]
        public string SystemStructureGroup { get; set; }

        public DirectoryInfo WorkFolder { get; set; }
        [Option("path", Required = false, HelpText = "Path to the installation package folder", Default = "export")]
        public string InstallPackagePath { get; set; }
    }
}