using CommandLine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyndle.Tools.CLI
{
    public class Options
    {

        // for now, we will configure the Tridion CM through the app.config

        //[Option('u', "username", Required = true, HelpText = "Tridion Username")]
        //public string Username { get; set; }

        //[Option('p', "password", Required = true, HelpText = "Tridion Password")]
        //public string Password { get; set; }

        //[Option('d', "domain", Required = false, HelpText = "Tridion Domain")]
        //public string Domain { get; set; }

        //[Option('h', "hostname", Required = true, HelpText = "Tridion Hostname")]
        //public string Hostname { get; set; }

  

        [Option("log-folder", Required = false, Default = ".", HelpText = "Folder where log file will be stored")]
        public string LogFolder { get; set; }


    }
}
