using System.IO;
using CommandLine;
using Dyndle.Tools.Environments;
using Dyndle.Tools.Installer.Configuration;

namespace Dyndle.Tools.CLI
{
#if DEBUG
    [Verb("install-package", HelpText = "Create install package for Dyndle")]
#endif
    public class CreateInstallPackageOptions : InstallerBaseOptions
    {
    }
}