using System.IO;
using Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.Installer.Configuration
{
    public interface IInstallerConfiguration : ICoreConfiguration
    {
        string Environment { get; set; }
        bool InstallTBBs { get; set; }
        bool InstallSitemap { get; set; }
        bool InstallSiteConfiguration { get; set; }
        DirectoryInfo WorkFolder { get; set; }
        string DyndleFolder { get; set; }
        string RootStructureGroup { get; set; }
        string SystemStructureGroup { get; set; }
        string InstallPackagePath { get; set; }
    }
}
