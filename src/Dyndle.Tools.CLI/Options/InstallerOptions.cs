using System.IO;
using CommandLine;
using Dyndle.Tools.Environments;
using Dyndle.Tools.Installer.Configuration;

namespace Dyndle.Tools.CLI
{
    [Verb("install", HelpText = "Install Dyndle in your content manager")]
    public class InstallerOptions : InstallerBaseOptions
    {

    }


}