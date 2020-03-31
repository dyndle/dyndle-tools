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
        string MainPublication { get; set; }
        string PagePublication { get; set; }
        string TBBFolder { get; set; }
        string PageTemplateFolder { get; set; }
        string ComponentTemplateFolder { get; set; }
        string SystemSG { get; set; }
    }
}
