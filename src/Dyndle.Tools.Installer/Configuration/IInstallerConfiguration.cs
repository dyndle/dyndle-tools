using System.IO;
using Dyndle.Tools.Core.Configuration;

namespace Dyndle.Tools.Installer.Configuration
{
    public interface IInstallerConfiguration : ICoreConfiguration
    {
        string Environment { get; set; }
        DirectoryInfo WorkFolder { get; set; }
        string DyndleFolder { get; set; }
        string DyndleStructureGroup { get; set; }
        string InstallPackagePath { get; set; }
    }
}
