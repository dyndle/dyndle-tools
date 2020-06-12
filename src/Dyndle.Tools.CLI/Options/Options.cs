using CommandLine;
using Dyndle.Tools.Core.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.CLI
{
    public class Options : ICoreConfiguration
    {

        [Option('v', "verbose", Required = false, HelpText = "Verbose")]
        public bool Verbose { get; set; }


        [Option("log-folder", Required = false, Default = ".", HelpText = "Folder where log file will be stored")]
        public string LogFolder { get; set; }


    }
}
