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
    public class DeleteEnvironment : IToolsModule
    {
        private IEnvironmentsConfiguration Configuration { get; set; }
        public DeleteEnvironment(IEnvironmentsConfiguration configuration)
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
                EnvironmentManager.Delete(Configuration.Name);
            }
            catch (Exception e)
            {
                return "Failed - " + e.Message;
            }
            return "Successfully deleted environment";
        }
    }
}
