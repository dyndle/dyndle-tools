using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyndle.Tools.Core;
using Dyndle.Tools.Core.ToolsModules;
using Newtonsoft.Json;
using Environment = Dyndle.Tools.Core.Models.Environment;

namespace Dyndle.Tools.Environments
{
    public class AddEnvironment : IToolsModule
    {
        private IEnvironmentsConfiguration Configuration { get; set; }
        public AddEnvironment(IEnvironmentsConfiguration configuration)
        {
            Configuration = configuration;
        }
        public string Run()
        {
            if (string.IsNullOrEmpty(Configuration.Name) ||
                string.IsNullOrEmpty(Configuration.CMSUrl) ||
                string.IsNullOrEmpty(Configuration.Username) ||
                string.IsNullOrEmpty(Configuration.Password))
            {
                return
                    "Failed - you need to supply a name, url, username and password. Type dyndle help environment for help.";
            }

            try
            {
                EnvironmentManager.Add(Configuration.Name, Configuration.CMSUrl, Configuration.Username,
                    Configuration.UserDomain, Configuration.Password, Configuration.MakeDefault);
            }
            catch (Exception e)
            {
                return "Failed - " + e.Message;
            }
            return "Successfully added environment";
        }
    }
}
