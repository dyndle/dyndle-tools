using System.IO;
using CommandLine;
using Dyndle.Tools.Environments;
using Dyndle.Tools.Installer.Configuration;

namespace Dyndle.Tools.CLI
{
    [Verb("install", HelpText = "Install Dyndle in your content manager")]
    public class InstallerOptions : Options, IInstallerConfiguration
    {
        [Option("environment", Required = false, HelpText = "Environment name (type dyndle list-environments for a list of available environments)")]
        public string Environment { get; set; }

        [Option("install-tbbs", Required = false, HelpText = "Install the Dyndle template building blocks")]
        public bool InstallTBBs { get; set; } = true;
        [Option("install-sitemap", Required = false, HelpText = "Install the Dyndle sitemap")]
        public bool InstallSitemap { get; set; }
        [Option("install-siteconfig", Required = false, HelpText = "Install the Dyndle site configuration")]
        public bool InstallSiteConfiguration { get; set; }

        public DirectoryInfo WorkFolder { get; set; }
        [Option("publication", Required = false, HelpText = "Publication where the Dyndle items must be created")]
        public string MainPublication { get; set; }
        [Option("page-publication", Required = false, HelpText = "Publication where the Dyndle pages must be created")]
        public string PagePublication { get; set; }
        [Option("tbb-folder", Required = false, HelpText = "Folder where the Dyndle template building blocks must be created")]
        public string TBBFolder { get; set; }
        [Option("pt-folder", Required = false, HelpText = "Folder where the Dyndle page templates must be created")]
        public string PageTemplateFolder { get; set; }
        [Option("ct-folder", Required = false, HelpText = "Publication where the Dyndle component templates must be created")]
        public string ComponentTemplateFolder { get; set; }
        [Option("system-sg", Required = false, HelpText = "Publication where the Dyndle system pages must be created")]
        public string SystemSG { get; set; }
    }
}