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
    public class UpdateEnvironment : IToolsModule
    {
        private IAddOrUpdateEnvironmentsConfiguration Configuration { get; set; }
        public UpdateEnvironment(IAddOrUpdateEnvironmentsConfiguration configuration)
        {
            Configuration = configuration;
        }
        public string Run()
        {
            if (string.IsNullOrEmpty(Configuration.Name))
            {
                return
                    "Failed - you need to supply a name. Type dyndle help environment for help.";
            }

            try
            {
                EnvironmentManager.Update(Configuration.Name, Configuration.CMSUrl, Configuration.Username,
                    Configuration.UserDomain, Configuration.Password, Configuration.MakeDefault);
            }
            catch (Exception e)
            {
                return "Failed - " + e.Message;
            }
            return "Successfully updated environment";
        }
    }
}
